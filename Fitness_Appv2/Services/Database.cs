using Fitness_Appv2.Views;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<List<testTable>> GetTestDataAsync()
        {
            return await _dbcon.Table<testTable>().ToListAsync(); //returns db as list
        }
        public async Task<testTable> GetTestItemAsync(int id)
        {
            return await _dbcon.Table<testTable>().Where(i => i.Id == id).FirstOrDefaultAsync(); // i => i.Id == id is a lambda function with a comparson function in it
        }
        public Task<int> SaveTestDataAsync(testTable testSaveData) //stores testTable object (aka a record from testTable class) in table, returns new PK
        {
            return _dbcon.InsertAsync(testSaveData);
        }
        public Task<int> DeleteTestDataAsync(testTable deleteItem)
        {
            return _dbcon.DeleteAsync(deleteItem);
        }

        public async Task<List<graphItemsSource>> GetGraphTestDataAsync(string XciseValue)
        {
            return await _dbcon.QueryAsync<graphItemsSource>("SELECT (WeightAttribute * (36/(37-RepsAttribute))) AS OneRepMaxAttribute, DateAttribute FROM testTable WHERE XciseAttribute = ? ;", XciseValue);
            // w * (36/(37-r)) is the Brzyki Formula
            // saves to a child class of testTable, so 1RMaxAttribute has its own property
        }

        public Task<List<testTable>> CustomMethod()
        {
            return _dbcon.QueryAsync<testTable>("DELETE FROM testTable");
        }
    }
}

