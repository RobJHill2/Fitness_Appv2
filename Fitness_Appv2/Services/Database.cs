using Fitness_Appv2.Views;
using SQLite;
using Syncfusion.XForms.TextInputLayout;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
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
            _DbCon.CreateTableAsync<SetsTable>(); // Creates SetsTable on startup
            _DbCon.CreateTableAsync<SetMediansTable>();
            _DbCon.CreateTableAsync<XcisesTable>();
            _DbCon.CreateTableAsync<UserDataTable>();
            _DbCon.CreateTableAsync<RoutineComponentsTable>();
            _DbCon.CreateTableAsync<RoutinesTable>();
            MaintainSetsDataAsync();
        }
        private async void MaintainSetsDataAsync()
        {

            DateTime startOfThisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            IEnumerable<int> XcisesList = (await GetXciseIdsAsync()).Select(obj => obj.Id);
            foreach (int Xcise in XcisesList) {

                // Storing Daily Median
                List<DateTime> daysList = (await _DbCon.QueryAsync<SetsTable>("SELECT DISTINCT DateAttribute FROM SetsTable WHERE XciseIdAttribute = ? AND DateAttribute >= ? AND DateAttribute <> ? AND DailyMedianTaken = FALSE ORDER BY DateAttribute ASC;", Xcise, startOfThisMonth, DateTime.Today)).Select(obj => obj.DateAttribute).ToList();

                if (daysList.Count() != 0)
                {
                    foreach (DateTime day in daysList)
                    {
                        List<float> data = (await _DbCon.QueryAsync<SetsTable>("SELECT E1RMaxAttribute FROM SetsTable WHERE XciseIdAttribute = ? AND DateAttribute = ? AND DailyMedianTaken = FALSE;", Xcise, day)).Select(obj => obj.E1RMaxAttribute).ToList();
                        int len = data.Count;
                        float e1RMaxMedian;
                        if (len % 2 == 0) { e1RMaxMedian = (data[(len / 2) - 1] + data[len / 2]) / 2; } else { e1RMaxMedian = data[(len - 1) / 2]; }
                        Debug.WriteLine("Daily Median Inserted");
                        await _DbCon.InsertAsync(new SetMediansTable
                        {
                            XciseIdAttribute = Xcise,
                            DateAttribute = day,
                            E1RMaxAttribute = e1RMaxMedian,
                            IsDailyMedian = true,
                        });
                    }
                }

                // Storing Monthly Median
                IEnumerable<DateTime> daysEnum = (await _DbCon.QueryAsync<SetsTable>("SELECT DateAttribute FROM SetsTable WHERE XciseIdAttribute = ? ORDER BY DateAttribute ASC;", Xcise)).Select(obj => obj.DateAttribute).ToList(); ;
                if (daysEnum.Count() != 0)
                {
                    DateTime oldestEntry = daysEnum.ToList()[0];

                    List<DateTime> monthsList = monthlyData.Select(obj => obj.DateAttribute).Distinct().ToList();
                    // generates list of months between the 
                        foreach (DateTime month in monthsList)
                        {
                            DateTime LB = month;
                            DateTime UB = LB.AddMonths(1);
                            List<float> data = (await _DbCon.QueryAsync<SetsTable>("SELECT E1RMaxAttribute FROM SetsTable WHERE XciseIdAttribute = ? AND DateAttribute >= ? AND DateAttribute < ? ORDER BY E1RMaxAttribute ASC;", Xcise, LB, UB)).Select(item => item.E1RMaxAttribute).ToList();
                            if (data != null)
                            {
                                int len = data.Count;
                                float e1RMaxMedian;
                                if (len % 2 == 0) { e1RMaxMedian = (data[len / 2] + data[(len / 2) + 1]) / 2; } else { e1RMaxMedian = data[(len - 1) / 2]; }
                                await _DbCon.InsertAsync(new SetMediansTable
                                {
                                    XciseIdAttribute = Xcise,
                                    DateAttribute = LB,
                                    E1RMaxAttribute = data[(data.Count - 1) / 2],
                                    IsDailyMedian = false,
                                });
                            }
                        }
                    }
                }
            }
            await _DbCon.QueryAsync<SetsTable>("UPDATE SetsTable SET DailyMedianTaken = TRUE WHERE DateAttribute >= ? AND DateAttribute < ?;", startOfThisMonth, DateTime.Today);
            await _DbCon.QueryAsync<SetsTable>("DELETE FROM SetsTable WHERE DateAttribute < ?;", startOfThisMonth);
        }

        public async Task<List<SetsTable>> GetSetsDataAsync()
        {
            try
            {
                return await _DbCon.Table<SetsTable>().ToListAsync(); //returns db as list
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

        public Task<int> SaveSets(SetsTable saveData) //stores SetsTable object (aka a record from SetsTable class) in table, returns new PK
        {
            return _DbCon.InsertAsync(saveData);
        }
        public Task<int> SaveXcise(XcisesTable saveData)
        {
            return _DbCon.InsertAsync(saveData);
        }
        public Task<int> SaveUserData(UserDataTable saveData)
        {
            return _DbCon.InsertAsync(saveData);
        }
        public Task<int> SaveRoutineComponent(RoutineComponentsTable saveData)
        {
            return _DbCon.InsertAsync(saveData);
        }
        public Task<int> SaveRoutine(RoutinesTable saveData) 
        {
            return _DbCon.InsertAsync(saveData);
        }

        public async Task<List<SetMediansTable>> GetXciseMediansAsync(int XciseId)
        {
            try
            {
                return await _DbCon.QueryAsync<SetMediansTable>("SELECT E1RMaxAttribute, DateAttribute FROM SetMediansTable WHERE XciseIdAttribute = ? ORDER BY DateAttribute ASC;", XciseId);
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
                return (await _DbCon.QueryAsync<XcisesTable>("SELECT Id, XciseNameAttribute FROM XcisesTable;")).ToList();
            }
            catch
            {
                return null;
            }
        }
        public async Task<bool> GetIsBodyweightXcise(int Id)
        {
            return (await _DbCon.QueryAsync<XcisesTable>("SELECT IsBodyweightAttribute FROM XcisesTable WHERE Id = ?;", Id))[0].IsBodyweightAttribute;  
        }
        public async Task<UserDataTable> GetLatestUserDataAsync()
        {
            try
            {
                List<UserDataTable> UserDataList = (await _DbCon.QueryAsync<UserDataTable>("SELECT * FROM UserDataTable;"));
                return UserDataList.Last();
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
        
        public Task<List<SetMediansTable>> CustomMethod()
        {
            return _DbCon.QueryAsync<SetMediansTable>("DELETE FROM RoutineComponentsTable;");
        }
    }
} 

