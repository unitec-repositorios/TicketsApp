using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace tickets.Models
{
    public class File:INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        private  string _idTicket;

        public  string ID_Ticket
        {
            get {
                return _idTicket;
            }
            set {
                _idTicket = value;
                OnPropertyChanged();
            }
        }


        private string _title;
        public string Title
        {
            get { return _title; }
            set {
                _title = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Name));
            }
        }


        private string _type;

        public string Type
        {
            get { return _type; }
            set {
                _type = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(IconSource));
            }
        }


        private DateTime _date;

        public DateTime Date
        {
            get { return _date; }
            set {
                _date = value;
                OnPropertyChanged();
            }
        }

        private byte[] _data;

        public byte[] Data
        {
            get { return _data; }
            set {
                _data = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Length));
            }
        }

        public int Length
        {
            get {
                if (Data == null)
                    return 0;
                return Data.Length;
            }
        }

        private string _path;

       

        public string Path
        {
            get { return _path; }
            set {
                _path = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get
            {
                return $"{Title}.{Type}";    
            }
           
        }
        //.gif, .jpg, .png, .zip, .rar, .csv, .doc, .docx, .xls, .xlsx, .txt, .pdf, .jpeg

        public string IconSource
        {
            get{
                return $"{Type}_icon.png";
            }

        }
    }
}
