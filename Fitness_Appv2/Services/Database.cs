using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitness_Appv2.Services
{
    public class Database
    {
        private readonly SQLiteAsyncConnection DbCon; // db connection object initialised
        private readonly List<PendingSetsTable> undoRedoStack = new List<PendingSetsTable>() { null }; // top of stack should always be a null
        private int topPointer = 0; // points to next free index
        public Database(string DbPath) // Db class constructor
        {
            DbCon = new SQLiteAsyncConnection(DbPath); // db connection assigned, using location of db on disk as argument
            DbCon.CreateTableAsync<PendingSetsTable>(); // Creates PendingSetsTable on startup
            DbCon.CreateTableAsync<DailyMediansTable>();
            DbCon.CreateTableAsync<MonthlyMediansTable>();
            DbCon.CreateTableAsync<XcisesTable>();
            DbCon.CreateTableAsync<UserDataTable>();
            DbCon.CreateTableAsync<RoutineComponentsTable>();
            DbCon.CreateTableAsync<RoutinesTable>();
            MaintainDBAsync();
        }
        private async void MaintainDBAsync()
        {
            await CheckNeedNewUserDataAsync();
            DateTime startOfThisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            List<PendingSetsTable> data = await DbCon.QueryAsync<PendingSetsTable>("SELECT * FROM PendingSetsTable WHERE DateAttribute <> ? ORDER BY E1RMaxAttribute DESC;", DateTime.Today);
            if (data.Count() != 0)
            {
                UpdateWeeklyConsistencyAsync(data.Where(obj => obj.DailyMedianTakenAttribute == false).ToList());
            }

            List<int> XcisesList = (await GetXciseIdsAsync()).Select(obj => obj.Id).ToList();
            foreach (int Xcise in XcisesList)
            {
                List<PendingSetsTable> xciseData = data.Where(obj => obj.XciseIdAttribute == Xcise).ToList();

                // Storing Daily Median
                List<PendingSetsTable> dailyData = xciseData.Where(obj => obj.DailyMedianTakenAttribute == false).ToList();

                if (dailyData.Count() != 0)
                {
                    UpdateXcisePBAsync(dailyData[0].E1RMaxAttribute, Xcise); // daily data sorted in descending

                    List<DateTime> daysList = dailyData.Select(obj => obj.DateAttribute).Distinct().ToList();
                    foreach (DateTime day in daysList)
                    {
                        List<float> E1RMaxes = dailyData.Where(obj => obj.DateAttribute == day).Select(obj => obj.E1RMaxAttribute).ToList();
                        float e1RMaxMedian = Utilities.GetMedian(E1RMaxes);

                        await DbCon.InsertAsync(new DailyMediansTable
                        {
                            XciseIdAttribute = Xcise,
                            DateAttribute = day,
                            E1RMaxAttribute = e1RMaxMedian
                        });
                    }
                }

                // Storing Monthly Median
                List<PendingSetsTable> monthlyData = xciseData.Where(obj => obj.DateAttribute < startOfThisMonth).ToList();
                if (monthlyData.Count() != 0)
                {
                    List<DateTime> monthsList = monthlyData.Select(obj => new DateTime(obj.DateAttribute.Year, obj.DateAttribute.Month, 1)).Distinct().ToList();
                    foreach (DateTime month in monthsList)
                    {
                        DateTime LB = month;
                        DateTime UB = LB.AddMonths(1);
                        List<float> E1RMaxes = monthlyData.Where(obj => obj.DateAttribute >= LB && obj.DateAttribute < UB).Select(obj => obj.E1RMaxAttribute).ToList();
                        float e1RMaxMedian = Utilities.GetMedian(E1RMaxes);
                        await DbCon.InsertAsync(new MonthlyMediansTable
                        {
                            XciseIdAttribute = Xcise,
                            DateAttribute = month,
                            E1RMaxAttribute = e1RMaxMedian
                        });
                        GenerateGoalAsync(Xcise); // Latest Monthly Median Changing --> new Goals
                    }
                }
            }
            await DbCon.QueryAsync<PendingSetsTable>("UPDATE PendingSetsTable SET DailyMedianTakenAttribute = TRUE WHERE ((DailyMedianTakenAttribute = FALSE) AND (DateAttribute <> ?));", DateTime.Today);
            await DbCon.QueryAsync<PendingSetsTable>("DELETE FROM PendingSetsTable WHERE DateAttribute < ?;", startOfThisMonth);
            await DbCon.QueryAsync<DailyMediansTable>("DELETE FROM DailyMediansTable WHERE DateAttribute < ?;", startOfThisMonth);
        }
        public async void GenerateGoalAsync(int Xcise)
        {
            const int NumPoints = 3; // how many months the regression backdates uses  
            const int GoalLength = 3; // how many months ahead the goal is predicting (3 means in 3 +- 0.5 months depending on the day)  
            List<SetMediansTable> monthMedians = await GetXciseMonthlyMediansAsync(Xcise);
            if (monthMedians.Count() >= NumPoints)
            {
                List<(int t, float e1RM)> datapoints = new List<(int, float)> { };
                for (int i = 1 - NumPoints; i <= 0; i++)
                {
                    datapoints.Add((i, monthMedians[monthMedians.Count() - 1 + i].E1RMaxAttribute)); // for NumPoints = 3: i is -2, -1, 0 
                } // adds the last n monthMedians to datapoints
                int tSum = datapoints.Select(x => x.t).Sum();
                int t2Sum = Convert.ToInt16(datapoints.Select(x => Math.Pow(x.t, 2)).Sum());
                float e1RMSum = datapoints.Select(x => x.e1RM).Sum();
                float te1RMSum = datapoints.Select(x => x.t * x.e1RM).Sum();

                float gradient = ((NumPoints * te1RMSum) - (tSum * e1RMSum)) / ((NumPoints * t2Sum) - Convert.ToInt16(Math.Pow(tSum, 2))); // regression formula
                // linear regression formula: gradient = (n∑xy - ∑x*∑y) / (n∑x^2 - (∑x)^2)
                float goal;
                if (gradient > 0) { goal = datapoints.Last().e1RM + gradient * (GoalLength + 1); }
                else { goal = datapoints.Last().e1RM; } // Will not give a goal that is less than the last month

                await DbCon.QueryAsync<XcisesTable>("UPDATE XcisesTable SET GoalAttribute = ? WHERE Id = ?;", goal, Xcise);
            }
        }

        // **** PendingSetsTable Methods Start ****
            public async Task<List<DisplaySetsDataModel>> GetPendingSetsToViewAsync()
            {
                try
                {
                    return await DbCon.QueryAsync<DisplaySetsDataModel>(@"SELECT PendingSetsTable.*, XcisesTable.XciseNameAttribute 
                                                                          FROM PendingSetsTable INNER JOIN XcisesTable ON PendingSetsTable.XciseIdAttribute = XcisesTable.Id
                                                                          ORDER BY DateAttribute DESC, Id DESC;"); // sorts by Id within the same date
                }
                catch
                {
                    return null;
                }
            }
            public async Task SaveSetAsync(PendingSetsTable saveData)
            {
                undoRedoStack[topPointer] = saveData;
                topPointer++;
                if (topPointer == undoRedoStack.Count()) { undoRedoStack.Add(null); } // if at top of stack add null value (makes checking redo validity simpler)
                for (int i = topPointer; i < undoRedoStack.Count(); i++) { undoRedoStack[i] = null; } // clears anything above topPointer as new branch has started so redo should be impossible 

                await DbCon.InsertAsync(saveData); 
                // when this happens the respective object in the undoRedoStack has its Id updated (autoincrement has a delayed effect?)
            }

            public async void UndoSetAsync()
            {
                if (topPointer != 0)
                {
                    topPointer--;
                    await DbCon.QueryAsync<PendingSetsTable>("DELETE FROM PendingSetsTable WHERE Id = ?;", undoRedoStack[topPointer].Id);
                }
            }
            public async void RedoSetAsync()
            {
                if (undoRedoStack[topPointer] != null)
                {
                    await DbCon.InsertAsync(undoRedoStack[topPointer]);

                    topPointer++;
                    if (topPointer == undoRedoStack.Count()) { undoRedoStack.Add(null); }
                }
            }
        // **** PendingSetsTable Methods End ****

        // **** XcisesTable Methods Start ****
            public async Task<List<XcisesTable>> GetXcisesAsync()
            {
                try
                {
                    return await DbCon.Table<XcisesTable>().ToListAsync();
                }
                catch
                {
                    return null;
                }
            }

            public async Task SaveXciseAsync(XcisesTable saveData)
            {
                await DbCon.InsertAsync(saveData);
            }

            public async Task<List<XcisesTable>> GetXciseIdsAsync()
            {
                try
                {
                    return (await DbCon.QueryAsync<XcisesTable>("SELECT Id FROM XcisesTable;")).ToList();
                }
                catch
                {
                    return null;
                }
            }

            public async Task<List<XcisesTable>> GetXciseNamesAsync()
            {
                try
                {
                    return (await DbCon.QueryAsync<XcisesTable>("SELECT Id, XciseNameAttribute FROM XcisesTable;"));
                }
                catch
                {
                    return null;
                }
            }

            public async void UpdateXciseIsPinnedAsync(Boolean NewIsPinned, int Id)
            {
                await DbCon.QueryAsync<XcisesTable>("UPDATE XcisesTable SET IsPinnedAttribute = ? WHERE Id = ?;", NewIsPinned, Id);
            }

            public async Task<List<XcisesTable>> GetPinnedXcisesAsync()
            {
                return await DbCon.QueryAsync<XcisesTable>("SELECT * FROM XcisesTable WHERE IsPinnedAttribute = TRUE;");
            }

        // **** XcisesTable Methods End ****

        // **** RoutinesTable Methods Start ****
            public async Task<List<RoutinesTable>> GetRoutinesAsync()
            {
                try
                {
                    return await DbCon.Table<RoutinesTable>().ToListAsync();
                }
                catch
                {
                    return null;
                }
            }

            public async Task<int> SaveRoutineAsync(RoutinesTable saveData) // returns id of new routine
            {
                await DbCon.InsertAsync(saveData);
                return saveData.Id;
            }

            public async Task<string> GetRoutineNameAsync(int Id)
            {
                try
                {
                    return (await DbCon.QueryAsync<RoutinesTable>("SELECT NameAttribute FROM RoutinesTable WHERE Id = ?;", Id))[0].NameAttribute;
                }
                catch
                {
                    return null;
                }
            }

            public async void UpdateRoutineNameAsync(string name, int Id)
            {
                await DbCon.QueryAsync<RoutinesTable>("UPDATE RoutinesTable SET XciseNameAttribute = ? WHERE Id = ?;", name, Id);
            }

            public async void DeleteRoutineAsync(int Id)
            {
                await DbCon.QueryAsync<RoutinesTable>("DELETE FROM RoutinesTable WHERE Id = ?;", Id);
            }

        // **** RoutinesTable Methods End ****

        // **** RoutineComponentsTable Methods Start ****
            public async void SaveRoutineComponentAsync(RoutineComponentsTable saveData)
            {
                await DbCon.InsertAsync(saveData);
            }

            public async Task<List<LogRoutineComponentDataModel>> GetRoutineComponentsToLogAsync(int RoutineId)
            {
                try
                {
                    return (await DbCon.QueryAsync<LogRoutineComponentDataModel>(@"SELECT RoutineComponentsTable.XciseIdAttribute, RoutineComponentsTable.SetsAttribute, XcisesTable.XciseNameAttribute 
                                                                                   FROM RoutineComponentsTable INNER JOIN XcisesTable ON RoutineComponentsTable.XciseIdAttribute = XcisesTable.Id 
                                                                                   WHERE RoutineAttribute = ?;", RoutineId)).ToList();
                }
                catch
                {
                    return null;
                }
            }

            public async Task<List<DisplayComponentsDataModel>> GetRoutineComponentsToEditAsync(int RoutineId)
            {
                try
                {
                    return (await DbCon.QueryAsync<DisplayComponentsDataModel>(@"SELECT RoutineComponentsTable.XciseIdAttribute, RoutineComponentsTable.SetsAttribute, XcisesTable.XciseNameAttribute
                                                                                FROM RoutineComponentsTable INNER JOIN XcisesTable ON RoutineComponentsTable.XciseIdAttribute = XcisesTable.Id 
                                                                                WHERE RoutineAttribute = ?;", RoutineId)).ToList();
                }
                catch
                {
                    return null;
                }
            }
            public async void DeleteRoutineComponentsAsync(int RoutineId)
            {
                await DbCon.QueryAsync<RoutineComponentsTable>("DELETE FROM RoutineComponentsTable WHERE RoutineAttribute = ?;", RoutineId);
            }
        // **** RoutineComponentsTable Methods End ****

        // **** Daily/MonthlyMediansTable Methods Start ****
            public async Task<List<SetMediansTable>> GetSetsGraphDataAsync()
            {
                try
                {
                    return await DbCon.QueryAsync<SetMediansTable>("SELECT * FROM DailyMediansTable UNION SELECT * FROM MonthlyMediansTable ORDER BY DateAttribute ASC;");
                }
                catch
                {
                    return null;
                }
            }
            public async Task<List<SetMediansTable>> GetXciseMonthlyMediansAsync(int XciseId)
            {
                try
                {
                    return await DbCon.QueryAsync<SetMediansTable>("SELECT E1RMaxAttribute, DateAttribute FROM MonthlyMediansTable WHERE XciseIdAttribute = ? ORDER BY DateAttribute ASC;", XciseId);
                }
                catch
                {
                    return null;
                }
            }
        // **** Daily/MonthlyMediansTable Methods End ****

        // **** UserDataTable Methods Start ****
            public async Task<List<UserDataTable>> GetBodyweightGraphDataAsync()
            {
                try
                {
                    return await DbCon.QueryAsync<UserDataTable>("SELECT BodyweightAttribute, DateAttribute FROM UserDataTable;");
                }
                catch
                {
                    return null;
                }
            }

            public async Task<List<UserDataTable>> GetConsistencyGraphDataAsync()
            {
                try
                {
                    return await DbCon.QueryAsync<UserDataTable>("SELECT WeeklyConsistencyAttribute, DateAttribute FROM UserDataTable WHERE DateAttribute < ?;", Utilities.GetLastMonday());
                }
                catch
                {
                    return null;
                }
            }
            public async Task<bool> GetIsBodyweightXciseAsync(int Id)
            {
                return (await DbCon.QueryAsync<XcisesTable>("SELECT IsBodyweightAttribute FROM XcisesTable WHERE Id = ?;", Id))[0].IsBodyweightAttribute;
            }

            public async void UpdateBodyweightThisWeekAsync(float bodyweight)
            {
                await DbCon.QueryAsync<UserDataTable>("UPDATE UserDataTable SET BodyweightAttribute = ? WHERE DateAttribute = ?;", bodyweight, Utilities.GetLastMonday());
            }

            public async void UpdateConsistencyGoalThisWeekAsync(int consistency)
            {
                await DbCon.QueryAsync<UserDataTable>("UPDATE UserDataTable SET WeeklyConsistencyGoalAttribute = ? WHERE DateAttribute = ?;", consistency, Utilities.GetLastMonday());
            }

            public async Task<UserDataTable> GetThisWeeksUserDataAsync()
            {
                try
                {
                    return (await DbCon.QueryAsync<UserDataTable>("SELECT * FROM UserDataTable ORDER BY DateAttribute DESC;"))[0];
                    
                }
                catch
                {
                    return null;
                }
            }

            public async Task<List<UserDataTable>> GetUserDataAsync()
            {
            try
                {
                    return await DbCon.QueryAsync<UserDataTable>("SELECT * FROM UserDataTable ORDER BY DateAttribute DESC;");
                }
            catch
                {
                    return null;
                }
            }

        public async Task CheckNeedNewUserDataAsync()
            {
            List<UserDataTable> userdata;
            try
                {
                userdata = await DbCon.QueryAsync<UserDataTable>("SELECT * FROM UserDataTable ORDER BY DateAttribute DESC;");
                } 
            catch 
                {
                userdata = null;
                }  
                // can't use GetUserDataAsync as it calls this method
            if (userdata == null || userdata.Count() == 0)
            {
                // if no userdata for this week

                await DbCon.InsertAsync(new UserDataTable
                {
                    BodyweightAttribute = 0,
                    DateAttribute = Utilities.GetLastMonday(),
                    WeeklyConsistencyAttribute = 0,
                    WeeklyConsistencyGoalAttribute = 0
                });
            } else if (userdata[0].DateAttribute < Utilities.GetLastMonday())
            {
                await DbCon.InsertAsync(new UserDataTable
                {
                    BodyweightAttribute = userdata[0].BodyweightAttribute,
                    DateAttribute = Utilities.GetLastMonday(),
                    WeeklyConsistencyAttribute = 0,
                    WeeklyConsistencyGoalAttribute = userdata[0].WeeklyConsistencyGoalAttribute
                });
            }
        }

            public async void UpdateWeeklyConsistencyAsync(List<PendingSetsTable> newSets)
            {
                List<DateTime> weeksList = newSets.Select(obj => Utilities.GetLastMonday()).Distinct().ToList();
                List<UserDataTable> userdataList = await GetUserDataAsync();
                foreach (DateTime week in weeksList)
                {
                    DateTime LB = week;
                    DateTime UB = LB.AddDays(7);
                    int count = newSets.Where(obj => obj.DateAttribute >= LB && obj.DateAttribute < UB).Count();
                    List<UserDataTable> userdataThatWeek = userdataList.Where(obj => obj.DateAttribute == week).ToList();

                    int prevCount;
                    if (userdataThatWeek.Count() != 0) { prevCount = userdataThatWeek[0].WeeklyConsistencyAttribute; } else { prevCount = 0; }
                    await DbCon.QueryAsync<UserDataTable>("UPDATE UserDataTable SET WeeklyConsistencyAttribute = ? WHERE DateAttribute = ?;", count + prevCount, week);
                }
            }

            public async void UpdateXcisePBAsync(float bestSet, int Xcise)
            {
                float currPB = (await DbCon.QueryAsync<XcisesTable>("SELECT PBAttribute FROM XcisesTable WHERE Id = ?;", Xcise))[0].PBAttribute;
                if (bestSet > currPB)
                {
                    await DbCon.QueryAsync<XcisesTable>("UPDATE XcisesTable SET PBAttribute = ? WHERE Id = ?;", bestSet, Xcise);
                }
            }
        // **** UserDataTable Methods End ****
    }
}  