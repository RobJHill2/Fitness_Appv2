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
        private readonly SQLiteAsyncConnection _dbcon; // create database connection obj
        public Database(string dbPath) // Database class constructor
        {
            _dbcon = new SQLiteAsyncConnection(dbPath);
            _dbcon.CreateTableAsync<SetsTable>(); // Creates SetsTable on startup
            _dbcon.CreateTableAsync<SetMediansTable>();
            _dbcon.CreateTableAsync<XcisesTable>();
            _dbcon.CreateTableAsync<UserDataTable>();
            MaintainSetsDataAsync();
        }
        private async void MaintainSetsDataAsync()
        {
            DateTime startOfThisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            IEnumerable<int> XcisesList = (await GetXciseIdsAsync()).Select(obj => obj.Id);
            foreach (int Xcise in XcisesList) {
                
                // Storing Daily Median
                List<DateTime> daysList = (await _dbcon.QueryAsync<SetsTable>("SELECT DISTINCT DateAttribute FROM SetsTable WHERE XciseIdAttribute = ? AND DateAttribute >= ? AND DateAttribute <> ? AND DailyMedianTaken = FALSE ORDER BY DateAttribute ASC;", Xcise, startOfThisMonth,DateTime.Today)).Select(obj => obj.DateAttribute).ToList();
                
                if (daysList.Count() != 0)
                {
                    foreach (DateTime day in daysList)
                    {
                        List<float> data = (await _dbcon.QueryAsync<SetsTable>("SELECT E1RMaxAttribute FROM SetsTable WHERE XciseIdAttribute = ? AND DateAttribute = ? AND DailyMedianTaken = FALSE;", Xcise, day)).Select(obj => obj.E1RMaxAttribute).ToList();
                        int len = data.Count;
                        float e1RMaxMedian;
                        if (len % 2 == 0) { e1RMaxMedian = (data[(len / 2)-1] + data[len / 2]) / 2; } else { e1RMaxMedian = data[(len - 1) / 2]; }
                        Debug.WriteLine("Daily Median Inserted");
                        await _dbcon.InsertAsync(new SetMediansTable
                        {
                            XciseIdAttribute = Xcise,
                            DateAttribute = day,
                            E1RMaxAttribute = e1RMaxMedian,
                            IsDailyMedian = true,
                        });
                    } 
                }

                // Storing Monthly Median
                IEnumerable<SetsTable> daysEnum = await _dbcon.QueryAsync<SetsTable>("SELECT DateAttribute FROM SetsTable WHERE XciseIdAttribute = ? ORDER BY DateAttribute ASC;", Xcise);
                if (daysEnum.Count() != 0)
                {
                    DateTime oldestEntry = daysEnum.ToList()[0].DateAttribute;

                    if (oldestEntry.Month != DateTime.Now.Month)
                    {
                        int monthsDiff = startOfThisMonth.Month - oldestEntry.Month + ((startOfThisMonth.Year - oldestEntry.Year) * 12);
                        IEnumerable<DateTime> monthsList = Enumerable.Range(0, monthsDiff).Select(x => new DateTime(oldestEntry.AddMonths(x).Year, oldestEntry.AddMonths(x).Month, 1));
                        foreach (DateTime month in monthsList)
                        {
                            DateTime LB = month;
                            DateTime UB = LB.AddMonths(1);
                            List<float> data = (await _dbcon.QueryAsync<SetsTable>("SELECT E1RMaxAttribute FROM SetsTable WHERE XciseIdAttribute = ? AND DateAttribute >= ? AND DateAttribute < ? ORDER BY E1RMaxAttribute ASC;", Xcise, LB, UB)).Select(item => item.E1RMaxAttribute).ToList();
                            if (data != null)
                            {
                                int len = data.Count;
                                float e1RMaxMedian;
                                if (len % 2 == 0) { e1RMaxMedian = (data[len / 2] + data[(len / 2) + 1]) / 2; } else { e1RMaxMedian = data[(len - 1) / 2]; }
                                await _dbcon.InsertAsync(new SetMediansTable
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
            await _dbcon.QueryAsync<SetsTable>("UPDATE SetsTable SET DailyMedianTaken = TRUE WHERE DateAttribute >= ? AND DateAttribute < ?;", startOfThisMonth, DateTime.Today);
            await _dbcon.QueryAsync<SetsTable>("DELETE FROM SetsTable WHERE DateAttribute < ?;", startOfThisMonth);
        }

        public async Task<List<SetsTable>> GetSetsDataAsync()
        {
            try
            {
                return await _dbcon.Table<SetsTable>().ToListAsync(); //returns db as list
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<List<SetMediansTable>> GetSetMediansAsync()
        {
            try
            {
                return await _dbcon.Table<SetMediansTable>().ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<List<XcisesTable>> GetXcisesAsync()
        {
            try
            {
                return await _dbcon.Table<XcisesTable>().ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<SetsTable> GetSetItemAsync(int id)
        {
            try
            {
                return await _dbcon.Table<SetsTable>().Where(i => i.Id == id).FirstOrDefaultAsync(); // i => i.Id == id is a lambda function with a comparson function in it
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public Task<int> SaveSets(SetsTable saveData) //stores SetsTable object (aka a record from SetsTable class) in table, returns new PK
        {
            return _dbcon.InsertAsync(saveData);
        }
        public Task<int> SaveXcise(XcisesTable saveData) 
        {
            return _dbcon.InsertAsync(saveData);
        }
        public Task<int> SaveUserData(UserDataTable saveData)
        {
            return _dbcon.InsertAsync(saveData);
        }
        public Task<int> DeleteDataAsync(SetsTable deleteItem)
        {
            return _dbcon.DeleteAsync(deleteItem);
        }
        public async Task<List<SetMediansTable>> GetXciseMediansAsync(int XciseId)
        {
            try
            {
                return await _dbcon.QueryAsync<SetMediansTable>("SELECT E1RMaxAttribute, DateAttribute FROM SetMediansTable WHERE XciseIdAttribute = ? ORDER BY DateAttribute ASC;", XciseId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<List<XcisesTable>> GetXciseIdsAsync()
        {
            try
            {
                return (await _dbcon.QueryAsync<XcisesTable>("SELECT Id FROM XcisesTable;")).ToList();
            }
            catch (Exception ex)
            {
                return null;
            } 
        }

        public async Task<List<XcisesTable>> GetXciseNamesAsync()
        {
            try
            {
                return (await _dbcon.QueryAsync<XcisesTable>("SELECT XciseNameAttribute FROM XcisesTable")).ToList();
            }
            catch
            {
                return null;
            }
        }

        public async Task<UserDataTable> GetLatestUserDataAsync()
        {
            try
            {
                List<UserDataTable> UserDataList = (await _dbcon.QueryAsync<UserDataTable>("SELECT * FROM UserDataTable"));
                int index = UserDataList.Count() - 1;
                return UserDataList[index];
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Task<List<SetMediansTable>> CustomMethod(){
            return _dbcon.QueryAsync<SetMediansTable>("DELETE FROM SetMediansTable;");
        }
    }
} 

