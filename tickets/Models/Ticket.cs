using System;
using System.ComponentModel;
using SQLite;
using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace tickets
{
    public class Ticket : INotifyPropertyChanged
    {
        [PrimaryKey]
        public string ID { get; set; }
        public int UserID { get; set; }
        public int Affected { get; set; }
        public string Classification { get; set; }
        public int Priority { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool Open { get; set; }

        public string OpenImage { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdate { get; set; }
        public string Date { get; set; }
        string image { get; set; }
        public string Area { get; set; }
        public string Category { get; set; }
        public string CareerFacultyDepartment { get; set; }


        public Ticket()
        {
           
            
            this.image = "";

            this.OpenImage ="";
        }

        public void PrintData()
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(this);
                Debug.WriteLine("{0}={1}", name, value);
            }
        }

        public string Image
        {
            set
            {
                if (image != value)
                {
                    image = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Image"));
                    }
                }
            }
            get
            {
                return image;
            }
        }

       

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal void Check()
        {
            if (Open)
                OpenImage = "";
            else
                OpenImage = "lock.png";
        }
    }
}
