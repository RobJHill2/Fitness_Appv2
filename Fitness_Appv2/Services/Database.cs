using Fitness_Appv2.Views;
using SQLite;
using Syncfusion.XForms.TextInputLayout;
using System;
using System.Collections.Generic;
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
            _dbcon.CreateTableAsync<TestTable>(); // Creates TestTable on startup
            _dbcon.CreateTableAsync<TestMediansTable>();
            _dbcon.CreateTableAsync<XcisesTable>();
            MaintainTestDataAsync();
        }
        private async void MaintainTestDataAsync()
        {
            DateTime startOfThisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            IEnumerable<string> XcisesList = (await GetXciseNamesAsync()).Select(obj => obj.XciseNameAttribute);
            foreach (string Xcise in XcisesList) {
                
                // Storing Daily Median
                List<DateTime> daysList = (await _dbcon.QueryAsync<TestTable>("SELECT DISTINCT DateAttribute FROM TestTable WHERE XciseAttribute = ? AND DateAttribute >= ? AND DateAttribute <> ? AND DailyMedianTaken = FALSE ORDER BY DateAttribute ASC;", Xcise, startOfThisMonth,DateTime.Today)).Select(obj => obj.DateAttribute).ToList();
                foreach (DateTime day in daysList) {
                    List<float> data = (await _dbcon.QueryAsync<TestTable>("SELECT E1RMaxAttribute FROM TestTable WHERE XciseAttribute = ? AND DateAttribute = ? AND DailyMedianTaken = FALSE;", Xcise, day)).Select(obj => obj.E1RMaxAttribute).ToList();
                    int len = data.Count;
                    float e1RMaxMedian;
                    if (len % 2 == 0) { e1RMaxMedian = (data[len / 2] + data[(len / 2) + 1])/2;} else {e1RMaxMedian= data[(len-1)/2];}
                    Debug.WriteLine("Daily Median Inserted");
                    await _dbcon.InsertAsync(new TestMediansTable
                    {
                        XciseAttribute = Xcise,
                        DateAttribute = day,
                        E1RMaxAttribute = e1RMaxMedian,
                        IsDailyMedian = true,
                    });
                }

                // Storing Monthly Median
                IEnumerable<TestTable> DateEnum = await _dbcon.QueryAsync<TestTable>("SELECT DateAttribute FROM TestTable WHERE XciseAttribute = ? ORDER BY DateAttribute ASC;", Xcise);
                DateTime oldestEntry = DateEnum.ToList()[0].DateAttribute;

                if (oldestEntry.Month != DateTime.Now.Month) { 
                    int monthsDiff = startOfThisMonth.Month - oldestEntry.Month + ((startOfThisMonth.Year-oldestEntry.Year) * 12);
                    IEnumerable<DateTime> monthsList = Enumerable.Range(0, monthsDiff).Select(x => new DateTime(oldestEntry.AddMonths(x).Year, oldestEntry.AddMonths(x).Month, 1));
                    foreach (DateTime month in monthsList)
                    {
                        DateTime LB = month;
                        DateTime UB = LB.AddMonths(1);
                        List<float> data = (await _dbcon.QueryAsync<TestTable>("SELECT E1RMaxAttribute FROM TestTable WHERE XciseAttribute = ? AND DateAttribute >= ? AND DateAttribute < ? ORDER BY E1RMaxAttribute ASC;", Xcise, LB, UB)).Select(item => item.E1RMaxAttribute).ToList();
                        if (data != null)
                        {
                            int len = data.Count;
                            float e1RMaxMedian;
                            if (len % 2 == 0) { e1RMaxMedian = (data[len / 2] + data[(len / 2) + 1]) / 2; } else { e1RMaxMedian = data[(len - 1) / 2]; }
                            await _dbcon.InsertAsync(new TestMediansTable
                            {
                                XciseAttribute = Xcise,
                                DateAttribute = LB,
                                E1RMaxAttribute = data[(data.Count - 1) / 2],
                                IsDailyMedian = false,
                            });
                        }
                    }
                }
            }
            await _dbcon.QueryAsync<TestTable>("UPDATE TestTable SET DailyMedianTaken = TRUE WHERE DateAttribute >= ?;", startOfThisMonth);
            await _dbcon.QueryAsync<TestTable>("DELETE FROM TestTable WHERE DateAttribute < ?;", startOfThisMonth);
        }

        public async Task<List<TestTable>> GetTestDataAsync()
        { 
            return await _dbcon.Table<TestTable>().ToListAsync(); //returns db as list
        }
        public async Task<List<TestMediansTable>> GetTestMediansAsync()
        {
            return await _dbcon.Table<TestMediansTable>().ToListAsync();
        }
        public async Task<List<XcisesTable>> GetXcisesAsync()
        {
            return await _dbcon.Table<XcisesTable>().ToListAsync();
        }
        public async Task<TestTable> GetTestItemAsync(int id)
        {
            return await _dbcon.Table<TestTable>().Where(i => i.Id == id).FirstOrDefaultAsync(); // i => i.Id == id is a lambda function with a comparson function in it
        }
        public Task<int> SaveTestDataAsync(TestTable testSaveData) //stores TestTable object (aka a record from TestTable class) in table, returns new PK
        {
            return _dbcon.InsertAsync(testSaveData);
        }
        public Task<int> SaveXciseAsync(XcisesTable testSaveData) 
        {
            return _dbcon.InsertAsync(testSaveData);
        }
        public Task<int> DeleteTestDataAsync(TestTable deleteItem)
        {
            return _dbcon.DeleteAsync(deleteItem);
        }
        public async Task<List<TestMediansTable>> GetXciseTestMediansAsync(string XciseValue)
        {
            return await _dbcon.QueryAsync<TestMediansTable>("SELECT E1RMaxAttribute, DateAttribute FROM TestMediansTable WHERE XciseAttribute = ?;", XciseValue);
                // ? notation is specific to this function, normal interpolation doesn't seem to work. Can use with multiple ?, just add more parameters
        }
        public async Task<List<XcisesTable>> GetXciseNamesAsync()
        {
            return (await _dbcon.QueryAsync<XcisesTable>("SELECT XciseNameAttribute FROM XcisesTable;")).ToList();
        }
        public Task<List<TestMediansTable>> CustomMethod()
        {
            return _dbcon.QueryAsync<TestMediansTable>("DELETE FROM TestMediansTable;");
        }
    }
} 

