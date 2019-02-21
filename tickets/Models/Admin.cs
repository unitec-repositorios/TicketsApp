using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using SQLite;
using System.Diagnostics;


namespace tickets
{
    public class Admin
    {
        [PrimaryKey, AutoIncrement]
        public string Username { get; set; }
        public string Password { get; set; }

        public bool IsValid()
        {
            return !(string.IsNullOrEmpty(Username)|| string.IsNullOrEmpty(Password));
        }
    }
}




