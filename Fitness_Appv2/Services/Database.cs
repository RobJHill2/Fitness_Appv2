using Fitness_Appv2.Views;
using SQLite;
using Syncfusion.SfChart.XForms;
using Syncfusion.XForms.TextInputLayout;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Fitness_Appv2.Services
{
    public class Database
    {
        private readonly SQLiteAsyncConnection _DbCon; // create db connection obj
        public Database(string DbPath) // Db class constructor
        {
            _DbCon = new SQLiteAsyncConnection(DbPath);
            _DbCon.CreateTableAsync<PendingSetsTable>(); // Creates PendingSetsTable on startup
            _DbCon.CreateTableAsync<SetMediansTable>();
            _DbCon.CreateTableAsync<DailyMediansTable>();
            _DbCon.CreateTableAsync<MonthlyMediansTable>();
            _DbCon.CreateTableAsync<XcisesTable>();
            _DbCon.CreateTableAsync<UserDataTable>();
            _DbCon.CreateTableAsync<RoutineComponentsTable>();
            _DbCon.CreateTableAsync<RoutinesTable>();
            MaintainDBAsync();
        }
        private async void MaintainDBAsync()
        {
            // **** TESTING ****
            // **** TESTING ****

            DateTime startOfThisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            List<int> XcisesList = (await GetXciseIdsAsync()).Select(obj => obj.Id).ToList();
            foreach (int Xcise in XcisesList)
            {
                List<PendingSetsTable> data = await _DbCon.QueryAsync<PendingSetsTable>("SELECT E1RMaxAttribute, DateAttribute FROM PendingSetsTable WHERE XciseIdAttribute = ? AND DateAttribute <> ? ORDER BY DateAttribute ASC;", Xcise, DateTime.Today);

                // Storing Daily Median
                List<PendingSetsTable> dailyData = data.Where(obj => obj.DailyMedianTaken = false).ToList();
                if (dailyData.Count() != 0)
                {
                    List<DateTime> daysList = dailyData.Select(obj => obj.DateAttribute).Distinct().ToList();
                    foreach (DateTime day in daysList)
                    {
                        List<float> E1RMaxes = dailyData.Where(obj => obj.DateAttribute == day).Select(obj => obj.E1RMaxAttribute).ToList();
                        E1RMaxes.Sort();
                        int len = E1RMaxes.Count;
                        float e1RMaxMedian;
                        if (len % 2 == 0) { e1RMaxMedian = (E1RMaxes[(len / 2) - 1] + E1RMaxes[len / 2]) / 2; } else { e1RMaxMedian = E1RMaxes[(len - 1) / 2]; }
                        await _DbCon.InsertAsync(new DailyMediansTable
                        {
                            XciseIdAttribute = Xcise,
                            DateAttribute = day,
                            E1RMaxAttribute = e1RMaxMedian
                        });
                    }
                }

                // Storing Monthly Median
                List<PendingSetsTable> monthlyData = data.Where(obj => obj.DateAttribute < startOfThisMonth).ToList();
                if (monthlyData.Count() != 0)
                {
                    DateTime oldestEntry = monthlyData.ToList()[0].DateAttribute;

                    int monthsDiff = DateTime.Today.Month - oldestEntry.Month + (DateTime.Today.Year - oldestEntry.Year) * 12;
                    List<DateTime> monthsList = Enumerable.Range(0, monthsDiff).Select(x => new DateTime(oldestEntry.AddMonths(x).Year, oldestEntry.AddMonths(x).Month, 1)).ToList();
                    // generates list of months between the month of the oldest Entry and this month
                    foreach (DateTime month in monthsList)
                    {
                        DateTime LB = month;
                        DateTime UB = LB.AddMonths(1);
                        List<float> E1RMaxes = monthlyData.Where(obj => obj.DateAttribute >= LB && obj.DateAttribute < UB).Select(obj => obj.E1RMaxAttribute).ToList();
                        E1RMaxes.Sort();
                        int len = E1RMaxes.Count;
                        float e1RMaxMedian;
                        if (len % 2 == 0) { e1RMaxMedian = (E1RMaxes[(len / 2) - 1] + E1RMaxes[len / 2]) / 2; } else { e1RMaxMedian = E1RMaxes[(len - 1) / 2]; }
                        await _DbCon.InsertAsync(new MonthlyMediansTable
                        {
                            XciseIdAttribute = Xcise,
                            DateAttribute = month,
                            E1RMaxAttribute = e1RMaxMedian
                        });
                        GenerateGoal(Xcise); // Latest Monthly Median Changing --> new Goals
                    }
                }
            }
            await _DbCon.QueryAsync<PendingSetsTable>("UPDATE PendingSetsTable SET DailyMedianTaken = TRUE WHERE DailyMedianTaken = FALSE;");
            await _DbCon.QueryAsync<PendingSetsTable>("DELETE FROM PendingSetsTable WHERE DateAttribute < ?;", startOfThisMonth);
            await _DbCon.QueryAsync<DailyMediansTable>("DELETE FROM DailyMediansTable WHERE DateAttribute < ?;", startOfThisMonth);
        }
        public async void GenerateGoal(int Xcise)
        {
            const int NumPoints = 3; // how many months the regression backdates uses  
            const int GoalLength = 3; // how many months ahead the goal is predicting (3 means in 3 +- 0.5 months depending on the day)  
            List<SetMediansTable> monthMedians = await GetXciseMonthlyMediansAsync(Xcise);
            if (monthMedians.Count() >= NumPoints)
            {
                List<(int t, float e1RM)> datapoints = new List<(int, float)> { };
                for (int i = 1 - NumPoints; i <= 0; i++)
                {
                    datapoints.Add((i, monthMedians[monthMedians.Count() - 1 + i].E1RMaxAttribute));
                } // adds the last n monthMedians to datapoints
                int tSum = datapoints.Select(x => x.t).Sum();
                int t2Sum = Convert.ToInt16(datapoints.Select(x => Math.Pow(x.t, 2)).Sum());
                float e1RMSum = datapoints.Select(x => x.e1RM).Sum();
                float te1RMSum = datapoints.Select(x => x.t * x.e1RM).Sum();

                float gradient = ((NumPoints * te1RMSum) - (tSum * e1RMSum)) / ((NumPoints * t2Sum) - Convert.ToInt16(Math.Pow(tSum, 2))); // regression formula
                // linear regression formula: gradient = (n∑xy - ∑x*∑y) / (n∑x^2 - (∑x)^2)
                float goal;
                if (gradient > 0) { goal = datapoints.Last().e1RM + gradient * (GoalLength + 1); }
                else { goal = datapoints.Last().e1RM; }

                await _DbCon.QueryAsync<XcisesTable>("UPDATE XcisesTable SET GoalAttribute = ? WHERE Id = ?;", goal, Xcise);
            }
        }


        public async Task<List<PendingSetsTable>> GetPendingSets()
        {
            try
            {
                return await _DbCon.Table<PendingSetsTable>().ToListAsync(); //returns db as list
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<XcisesTable>> GetXcisesAsync()
        {
            try
            {
                return await _DbCon.Table<XcisesTable>().ToListAsync();
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<RoutinesTable>> GetRoutinesAsync()
        {
            try
            {
                return await _DbCon.Table<RoutinesTable>().ToListAsync();
            }
            catch
            {
                return null;
            }
        }

        public async void SaveSets(PendingSetsTable saveData) //stores PendingSetsTable object (aka a record from PendingSetsTable class) in table, returns new PK
        {
            float currPB = (await _DbCon.QueryAsync<XcisesTable>("SELECT PBAttribute FROM XcisesTable WHERE Id = ?;", saveData.XciseIdAttribute))[0].PBAttribute;
            if (saveData.E1RMaxAttribute > currPB)
            {
                await _DbCon.QueryAsync<XcisesTable>("UPDATE XcisesTable SET PBAttribute = ? WHERE Id = ?;", saveData.E1RMaxAttribute, saveData.XciseIdAttribute);
            }

            await _DbCon.InsertAsync(saveData);
        }
        public void SaveXcise(XcisesTable saveData)
        {
            _DbCon.InsertAsync(saveData);
        }
        public void SaveUserData(UserDataTable saveData)
        {
            _DbCon.InsertAsync(saveData);
        }
        public void SaveRoutineComponent(RoutineComponentsTable saveData)
        {
            _DbCon.InsertAsync(saveData);
        }
        public void SaveRoutine(RoutinesTable saveData) 
        {
            _DbCon.InsertAsync(saveData);
        }

        public async Task<List<SetMediansTable>> GetXciseDailyMediansAsync(int XciseId)
        {
            try
            {
                return await _DbCon.QueryAsync<SetMediansTable>("SELECT E1RMaxAttribute, DateAttribute FROM DailyMediansTable WHERE XciseIdAttribute = ? ORDER BY DateAttribute ASC;", XciseId);
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
                return await _DbCon.QueryAsync<SetMediansTable>("SELECT E1RMaxAttribute, DateAttribute FROM MonthlyMediansTable WHERE XciseIdAttribute = ? ORDER BY DateAttribute ASC;", XciseId);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<XcisesTable>> GetXciseIdsAsync()
        {
            try
            {
                return (await _DbCon.QueryAsync<XcisesTable>("SELECT Id FROM XcisesTable;")).ToList();
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
                return (await _DbCon.QueryAsync<XcisesTable>("SELECT Id, XciseNameAttribute FROM XcisesTable;"));
            }
            catch 
            {
                return null;
            }
        }
        public async Task<bool> GetIsBodyweightXciseAsync(int Id)
        {
            return (await _DbCon.QueryAsync<XcisesTable>("SELECT IsBodyweightAttribute FROM XcisesTable WHERE Id = ?;", Id))[0].IsBodyweightAttribute;  
        }
        public async Task<UserDataTable> GetLatestUserDataAsync()
        {
            try
            {
                return (await _DbCon.QueryAsync<UserDataTable>("SELECT * FROM UserDataTable ORDER BY DateAttribute DESC;"))[0];
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<RoutineComponentsTable>> GetRoutineComponentsAsync(int RoutineId)
        {
            try
            {
                return (await _DbCon.QueryAsync<RoutineComponentsTable>("SELECT XciseIdAttribute, SetsAttribute FROM RoutineComponentsTable WHERE RoutineAttribute = ?;", RoutineId)).ToList();
            }
            catch
            {
                return null;
            }
        }
        public async Task<string> GetRoutineNameAsync(int Id)
        {
            try
            {
                return (await _DbCon.QueryAsync<RoutinesTable>("SELECT NameAttribute FROM RoutinesTable WHERE Id = ?;", Id))[0].NameAttribute;
            } 
            catch 
            {
                return null;
            }
        }
        public async Task<List<RoutinesTable>> UpdateRoutineNameAsync(string name, int Id)
        {
            return await _DbCon.QueryAsync<RoutinesTable>("UPDATE RoutinesTable SET NameAttribute = ? WHERE Id = ?", name, Id);
        }

        public Task<List<RoutineComponentsTable>> DeleteRoutineComponents (int RoutineId)
        {
            return _DbCon.QueryAsync<RoutineComponentsTable>("DELETE FROM RoutineComponentsTable WHERE RoutineAttribute = ?;", RoutineId);
        }

        public Task<List<RoutinesTable>> DeleteRoutine(int Id)
        {
            return _DbCon.QueryAsync<RoutinesTable>("DELETE FROM RoutinesTable WHERE Id = ?;", Id);
        }
        
        public Task<List<DailyMediansTable>> CustomMethod()
        {
            return _DbCon.QueryAsync<DailyMediansTable>("DELETE FROM RoutineComponentsTable;");
        }
    }
} 

