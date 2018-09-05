using System;
using System.ComponentModel;
using SQLite;

namespace tickets
{
    public class Ticket
    {
        [PrimaryKey]
        public string ID { get; set; }
        public int UserID { get; set; }
        public int Affected { get; set; }
        public int Classification { get; set; }
        public int Priority { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }

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
