using System;
using System.ComponentModel;
using SQLite;
using System.Diagnostics;

namespace tickets
{
    public class Comment
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public int TicketID { get; set; }
        public DateTime Time { get; set; }
        public string Sender { get; set; }
        public string Body { get; set; }



        public void PrintData()
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(this);
                Debug.WriteLine("{0}={1}", name, value);
            }
        }
    }
}
