using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;


namespace tickets
{
    public class Database
    {
        readonly SQLiteAsyncConnection database;

        public Database(string dbPath)
        {
            database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<User>().Wait();
        }

        public User GetCurrentUser()
        {
            try
            {
                return database.FindWithQueryAsync<User>("SELECT IsActive from User WHERE IsActive = true").Result;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }
    }
}
