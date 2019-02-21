using System;
using System.ComponentModel;
using SQLite;
using System.Diagnostics;


namespace tickets
{
    public class AdminUser
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        
        public AdminUser() {}

        public AdminUser(string Username, string Password) {}
        public bool IsValid()
        {
            return !(string.IsNullOrEmpty(Username)
                     || string.IsNullOrEmpty(Password));
        }
    }
}
