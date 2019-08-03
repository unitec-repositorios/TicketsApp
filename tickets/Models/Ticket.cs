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

        public string Estado { get; set; }
        public string OpenImage { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdate { get; set; }
        public string Date { get; set; }
        string image { get; set; }
        public string Area { get; set; }
        public string Category { get; set; }
        public string CareerFacultyDepartment { get; set; }


        private byte[][] _files;

        private string fileName { get; set; }

        private int _limitFiles = AppSettings.GetLimiteArchivos();

        private char separatorFileName = '$';


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

       public void AddFileName(string _filename)
       {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = _filename;
            }
            else
            {
                fileName += $"{separatorFileName}{fileName}";
            }

       }

        public void AddFileName(string[] _filenameArray)
        {
            foreach(var item in _filenameArray)
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = item;
                }
                else
                {
                    fileName += $"{separatorFileName}{item}";
                }
            }
        }

        public string[] GetFilenames()
        {
            return fileName.Split(separatorFileName);
        }

        public void AddFileByteArray(byte[] fileData)
        {
            if (fileData.Length > AppSettings.GetSizeLimitFile())
            {
                return;
            }
                
            if (_files == null)
            {
                _files = new byte[_limitFiles][];
                _files[0] = fileData;

            }
            else
            {
                int pos = 0;
                foreach (var item in _files)
                {
                    if (item == null)
                        break;
                    pos++;
                }
                if (pos < _limitFiles)
                    _files[pos] = fileData;
            }
        }

        public void AddFileByteArray(byte[][] _dataFiles)
        {
            foreach (var item in _dataFiles)
            {
                if (item.Length > AppSettings.GetSizeLimitFile())
                {
                    return;
                }
                if (_files == null)
                {
                    _files = new byte[_limitFiles][];
                    _files[0] = item;
                }
                else
                {

                    int pos = 0;
                    foreach (var item2 in _files)
                    {
                        if (item2 == null)
                            break;
                        pos++;
                    }
                    if (pos < _limitFiles)
                        _files[pos] = item;
                }
            }
        }

        public  byte[] GetFileByteArray(int position = 0)
        {
            return _files[position];
        }

        public byte[][] GetFileByteArray()
        {
            return _files;
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
            if (Estado.Contains("Resuelto"))
                Open = false;
            else
                Open = true;

            if (Open)
                OpenImage = "";
            else
                OpenImage = "lock.png";
        }
    }
}
