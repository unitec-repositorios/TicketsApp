using System.Collections.Generic;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;
using System;
using System.Text;
using tickets.Data;
using System.Collections.ObjectModel;
using tickets.Models;
using Xamarin.Forms;
using SQLite.Net;

namespace tickets
{
    public class Database:IDisposable
    {
        readonly SQLite.SQLiteAsyncConnection database;

        public Database(string mode="")
        {
            var configuracionDB = DependencyService.Get<IConfigurationDB>();
            if (string.Equals(mode,"Test",StringComparison.OrdinalIgnoreCase))  ///IgnoreCase
            {
                database = new SQLite.SQLiteAsyncConnection(Path.Combine(configuracionDB.directorio, "TicketsAppTest.db3"));
            }
            else
            {
                database = new SQLite.SQLiteAsyncConnection(Path.Combine(configuracionDB.directorio, "TicketApp.db3"));
            }
            database.CreateTableAsync<User>().Wait();
            database.CreateTableAsync<Ticket>().Wait();
            database.CreateTableAsync<Message>().Wait();
            database.CreateTableAsync<Comment>().Wait();
            database.CreateTableAsync<AdminUser>().Wait();
            database.CreateTableAsync<TicketFile>().Wait();
        }

        public SQLite.SQLiteAsyncConnection GetConnection()
        {
            return database;
        }

        /**
         * Clear Ticket
         * 
         * 
        *///
        public void ClearTicket()
        {
            database.ExecuteAsync("DELETE FROM Ticket").Wait();
        }



        /// <summary>
        /// Clears the database.
        /// </summary>
        public void ClearDatabase()
        {
            database.ExecuteAsync("DELETE FROM User").Wait();
            database.ExecuteAsync("DELETE FROM Ticket").Wait();
            database.ExecuteAsync("DELETE FROM Comment").Wait();
            database.ExecuteAsync("DELETE FROM AdminUser").Wait();
        }
        
        /// <summary>
        /// Gets the current Admin user.
        /// </summary>
        /// <returns>The current user not async.</returns>

        public AdminUser GetCurrentAdminUserNotAsync()
        {
            try
            {
                return database.FindWithQueryAsync<AdminUser>("SELECT * from AdminUser WHERE IsCurrent = 1").Result;
            }
#pragma warning disable CS0168 // La variable 'ex' se ha declarado pero nunca se usa
            catch (System.Exception ex)
#pragma warning restore CS0168 // La variable 'ex' se ha declarado pero nunca se usa
            {
                return null;
            }
        }


        public AdminUser GetAdminUserAsync(string email)
        {
            try
            {
                return database.FindWithQueryAsync<AdminUser>("SELECT * from AdminUser WHERE Email LIKE ?", email).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Gets the current user not async.
        /// </summary>
        /// <returns>The current user not async.</returns>
        public Task<User> GetCurrentUserAsync()
        {
            try
            {
                return database.Table<User>().FirstOrDefaultAsync(user => user.IsCurrent==true);
          
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError Database:\n" + ex);
                return null;
            }
        }

        public Task<User> GetUserAsync(string email)
        {
            try
            {
                return database.Table<User>().Where(user=>user.Email==email).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError Database:\n" + ex);
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
        public User GetCurrentUser()
        {
            try
            {
                return GetCurrentUserAsync().Result;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace + "\nMensaje: " + ex.Message);
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
        /// Creates a new Admin User and sets it as the current user 
        /// (setting all others IsCurrent to false)
        /// </summary>
        /// <param name="admin">
        /// Admin User to add and set as current
        /// </param>
        public async Task<int> CreateNewCurrentAdminUser(AdminUser admin)
        {
            await database.ExecuteAsync("UPDATE AdminUser SET IsCurrent = 1");
            admin.IsCurrent = true;
            return await SaveAdminUserAsync(admin);
        }

        /// <summary>
        /// Inserts or updates the AdminUser especified by id
        /// </summary>
        /// <param name="user">
        /// AdminUser to add or update
        /// </param>
        public Task<int> SaveAdminUserAsync(AdminUser admin)
        {
            if (admin.ID != 0)
            {
                return database.UpdateAsync(admin);
            }
            else
            {
                return database.InsertAsync(admin);
            }
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
        //public Task<List<Ticket>> GetTicketsAsync(User user)
        //{
        //    return database.QueryAsync<Ticket>("SELECT * FROM Ticket WHERE UserID = ? ORDER BY Image DESC", user.ID);
        //}
        public Task<List<Ticket>> GetTicketsAsync()
        {
            //return database.Table<Ticket>().ToListAsync();
           // this.ClearTicket();
            return database.QueryAsync<Ticket>("SELECT * FROM Ticket WHERE UserID = 1 ORDER BY Image DESC");
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

        public string encryptPassword (string textToEncrypt){
            string encriptionKey = "ticketsApp";
			var algorithm = GetAlgorithm(encriptionKey);

            //Anything to process?
            if (textToEncrypt==null || textToEncrypt=="") return "";

            byte[] encryptedBytes;
            using (ICryptoTransform encryptor = algorithm.CreateEncryptor(algorithm.Key, algorithm.IV))
            {
                byte[] bytesToEncrypt = Encoding.UTF8.GetBytes(textToEncrypt);
                encryptedBytes = InMemoryCrypt(bytesToEncrypt, encryptor);
            }
            return Convert.ToBase64String(encryptedBytes);
		}

        private static RijndaelManaged GetAlgorithm(string encryptionPassword)
        {
            // Create an encryption key from the encryptionPassword and salt.
            byte[] salt = Encoding.ASCII.GetBytes("0820222251");
            var key = new Rfc2898DeriveBytes(encryptionPassword, salt);

            // Declare that we are going to use the Rijndael algorithm with the key that we've just got.
            var algorithm = new RijndaelManaged();
            int bytesForKey = algorithm.KeySize / 8;
            int bytesForIV = algorithm.BlockSize / 8;
            algorithm.Key = key.GetBytes(bytesForKey);
            algorithm.IV = key.GetBytes(bytesForIV);
            return algorithm;
        }

        private static byte[] InMemoryCrypt(byte[] data, ICryptoTransform transform)
        {
            MemoryStream memory = new MemoryStream();
            using (Stream stream = new CryptoStream(memory, transform, CryptoStreamMode.Write))
            {
                stream.Write(data, 0, data.Length);
            }
            return memory.ToArray();
        }


        public Task<Ticket> GetTicket(string id)
        {
            return database.Table<Ticket>().FirstOrDefaultAsync(t => t.ID==id);
        }

        public async Task<List<Ticket>> GetTickets()
        {
            
            var lista_ticket = await database.Table<Ticket>().ToListAsync();

            return lista_ticket;
        }

        public async Task<List<Ticket>> GetTickets(int _idCurrentUser)
        {
            return await database.Table<Ticket>().Where(item => item.UserID==_idCurrentUser).ToListAsync();
        }

        public async Task ActualizarUsuario(User _user)
        {
            try
            {
                await database.UpdateAsync(_user);
            }
            catch(Exception)
            {
                await database.InsertOrReplaceAsync(_user);
                
            }

        }            

        public async Task AgregarTicket(Ticket ticket)
        {
            await database.InsertOrReplaceAsync(ticket);
        }

        public async Task ActualizarTicket(Ticket ticket)
        {
           await database.UpdateAsync(ticket);
        }

        public async void EliminarTicket(Ticket ticket)
        {
            await database.DeleteAsync(ticket);
           
        }

        public async void EliminarTicket(string id)
        {
            EliminarTicket(await this.GetTicket(id));
        }


        /// <summary>
        /// Repositorio Mensjaes
        /// </summary>
        
        public async Task AddMensaje(Message message )
        {
            await database.InsertOrReplaceAsync(message);
        
        }

        public async Task AddMensajes(List<Message> mensajes)
        {
            await database.InsertAllAsync(mensajes);
        }
        public async Task<List<Message>> GetMessages(string id)
        {
            return await database.Table<Message>().Where(item=>item.IdTicket==id).ToListAsync();
        }

        public async Task DeleteAllMessagesWith(string idTicket)
        {
            await database.Table<Message>().DeleteAsync(x => x.IdTicket == idTicket);
        }
       
            


        public void Dispose()
        {
            database.CloseAsync();
        }
    }
}
