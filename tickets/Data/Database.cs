using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;
using System.Diagnostics;

namespace tickets
{
    public class Database
    {
        readonly SQLiteAsyncConnection database;

        public Database(string dbPath)
        {
            database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<User>().Wait();
            database.CreateTableAsync<Ticket>().Wait();
            database.CreateTableAsync<Comment>().Wait();
        }

        public SQLiteAsyncConnection GetConnection()
        {
            return database;
        }

        /// <summary>
        /// Clears the database.
        /// </summary>
        public void ClearDatabase()
        {
            database.ExecuteAsync("DELETE FROM User").Wait();
            database.ExecuteAsync("DELETE FROM Ticket").Wait();
            database.ExecuteAsync("DELETE FROM Comment").Wait();
        }
        /// <summary>
        /// Gets the current user not async.
        /// </summary>
        /// <returns>The current user not async.</returns>
        public User GetCurrentUserNotAsync()
        {
            try
            {
                return database.FindWithQueryAsync<User>("SELECT * from User WHERE IsCurrent = 1").Result;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        public User GetUserAsync(string email)
        {
            try
            {
                return database.FindWithQueryAsync<User>("SELECT * from User WHERE Email LIKE ?", email).Result;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        public async void Logout()
        {
            await database.ExecuteAsync("Update user SET IsCurrent = 0");

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

        /// <summary>
        /// Creates the new ticket.
        /// </summary>
        /// <returns>The new ticket.</returns>
        /// <param name="ticket">Ticket.</param>
        public Task<int> CreateNewTicket(Ticket ticket)
        {
            ticket.PrintData();
            return database.InsertAsync(ticket);
        }

        /// <summary>
        /// Update the ticket.
        /// </summary>
        /// <returns>The new ticket.</returns>
        /// <param name="ticket">Ticket.</param>
        public Task<int> UpdateTicket(Ticket ticket)
        {
            ticket.PrintData();
            return database.UpdateAsync(ticket);
        }

        /// <summary>
        /// Gets the tickets async.
        /// </summary>
        /// <returns>The tickets async.</returns>
        public Task<List<Ticket>> GetTicketsAsync(User user)
        {
            return database.QueryAsync<Ticket>("SELECT * FROM Ticket WHERE UserID = ? ORDER BY Image DESC", user.ID);
        }


        /// <summary>
        /// Gets the comments for ticket async.
        /// </summary>
        /// <returns>The comments for ticket async.</returns>
        /// <param name="TicketID">Ticket identifier.</param>
        public Task<List<Comment>> GetCommentsForTicketAsync(string TicketID)
        {
            return database.QueryAsync<Comment>("SELECT * FROM Comment WHERE TicketID = ?", TicketID);
        }
    }
}
