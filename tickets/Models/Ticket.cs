using System;
using System.ComponentModel;
using SQLite;

namespace tickets.Models
{
    public class Ticket
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public int UserID { get; set; }

        public void PrintData()
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(this);
                Console.WriteLine("{0}={1}", name, value);
            }
        }
    }
}
