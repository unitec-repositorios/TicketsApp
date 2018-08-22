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

        public SQLiteAsyncConnection GetConnection()
        {
            return database;
        }

        /// <summary>
        /// Returns a <see cref="User"/> which is the current user
        /// </summary>
        public Task<User> GetCurrentUser()
        {
            try
            {
                return database.FindWithQueryAsync<User>("SELECT * from User WHERE IsCurrent = 1");
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a new user and sets it as the current user 
        /// (setting all others IsCurrent to false)
        /// </summary>
        /// <param name="user">
        /// User to add and set as current
        /// </param>
        public async Task<int> CreateNewCurrentUser(User user)
        {
            await database.ExecuteAsync("UPDATE User SET IsCurrent = 0");
            user.IsCurrent = true;
            return await SaveUserAsync(user);
        }

        /// <summary>
        /// Inserts or updates the user especified by id
        /// </summary>
        /// <param name="user">
        /// User to add or update
        /// </param>
        public Task<int> SaveUserAsync(User user)
        {
            if (user.ID != 0)
            {
                return database.UpdateAsync(user);
            }
            else
            {
                return database.InsertAsync(user);
            }
        }
    }
}
