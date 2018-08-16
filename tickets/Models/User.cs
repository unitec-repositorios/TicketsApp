using System;
using SQLite;

namespace tickets
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Campus { get; set; }
        public string Profile { get; set; }
        public string Account { get; set; }
        public string Career { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime Date { get; set; }
    }
}
