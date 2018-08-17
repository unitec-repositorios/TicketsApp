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

        public async void EraseDatabase()
        {
            await database.DropTableAsync<User>();
            await database.ExecuteAsync("VACUUM");
        }


        public async Task<User> GetCurrentUser()
        {
            try
            {
                return await database.FindWithQueryAsync<User>("SELECT IsActive from User WHERE IsActive = true");
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }
    }
}
