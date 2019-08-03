using System;
using System.ComponentModel;
using SQLite;
using System.Diagnostics;


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
        public bool IsCurrent { get; set; }
        public string PersonalMail { get; internal set; }

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
            return !(string.IsNullOrEmpty(Name)
                     || string.IsNullOrEmpty(Email)
                     || string.IsNullOrEmpty(Campus)
                     || string.IsNullOrEmpty(Profile)
                     || string.IsNullOrEmpty(Account)
                     || string.IsNullOrEmpty(Career)
                     || string.IsNullOrEmpty(PhoneNumber));
        }
    }
}
