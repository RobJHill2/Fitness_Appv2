using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fitness_Appv2.Services
{
    public class Database
    {
        private readonly SQLiteAsyncConnection _dbcon; // create database connection obj
        public Database(string dbPath) // Database class constructor
        {
            _dbcon = new SQLiteAsyncConnection(dbPath);
            _dbcon.CreateTableAsync<testTable>(); // Creates testTable on startup
        }
        public Task<List<testTable>> GetTestDataAsync()
        {
            return _dbcon.Table<testTable>().ToListAsync(); //returns db as list
        }
        public async Task<testTable> GetTestItemAsync(int id)
        {
            return await _dbcon.Table<testTable>().Where(i => i.Id == id).FirstOrDefaultAsync(); // i => i.Id == id is a lambda function with a comparson function in it
        }
        public Task<int> SaveTestDataAsync(testTable testSaveData) //stores testTable object (aka a record from testTable class) in table, returns new PK
        {
            return _dbcon.InsertAsync(testSaveData);
        }
        public async Task<int> DeleteTestDataAsync(testTable deleteItem)
        {
            return await _dbcon.DeleteAsync(deleteItem);
        }
    }
}

