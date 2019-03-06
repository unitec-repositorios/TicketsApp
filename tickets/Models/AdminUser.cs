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
        public bool IsCurrent { get; set; }
        public AdminUser() {}

        public AdminUser(string Username, string Password) {}

        public void PrintData()
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(this);
                Debug.WriteLine("{0}={1}", name, value);
            }
        }

        public bool IsValid()
        {
            return !(string.IsNullOrEmpty(Username)
                     || string.IsNullOrEmpty(Password));
        }
    }
}
