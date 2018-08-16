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
    }
}
