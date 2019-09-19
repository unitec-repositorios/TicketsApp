using System;
using System.ComponentModel;
using SQLite;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace tickets
{
    public class Ticket : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        private string _id;
        [PrimaryKey]
        public string ID
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }
        private int _userID;

        public int UserID
        {
            get { return _userID; }
            set {
                _userID = value;
                OnPropertyChanged();
            }
        }

        private int _usersAffected;
        public int UsersAffected
        {
            get { return _usersAffected; }
            set {
                _usersAffected = value;
                OnPropertyChanged();
            }
        }

        private string _classification;
        public string Classification
        {
            get { return _classification; }
            set {
                _classification = value;
                OnPropertyChanged();
            }
        }

        private string _priority;

        public string Priority
        {
            get { return _priority; }
            set {
                _priority = value;
                OnPropertyChanged();
            }
        }

        private string _subject;

        public string Subject
        {
            get { return _subject; }
            set {
                _subject = value;
                OnPropertyChanged();
            }
        }

        private bool _isOpen;

        public bool IsOpen
        {
            get {
                if (Estado == "Resuelto" || Estado == "Cerrar Ticket" || !_isOpen)
                    return false;
                else
                    return false;
            }
            set
            {
                _isOpen = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(OpenImage));
            }
            
        }

        private string _openImage;

        public string OpenImage
        {
            get
            {
                if (IsOpen)
                    return "";
                else
                    return "lock.png";
            }
            set {
                _openImage = value;
                OnPropertyChanged();
             

            }
        }

        private DateTime _creationDate;

        public DateTime CreationDate
        {
            get { return _creationDate; }
            set {
                _creationDate = value;
                OnPropertyChanged();
            }
        }

        private bool _hasUpdate;

        public bool HasUpdate
        {
            get {
                return _hasUpdate;
            }
            set {
                _hasUpdate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UpdateImage));
            }
        }


        private DateTime _lastUpdate;

        public DateTime LastUpdate
        {
            get { return _lastUpdate; }
            set {
                _lastUpdate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UpdateImage));
            }
        }

        private string _category;

        public string Category
        {
            get { return _category; }
            set {
                _category = value;
                OnPropertyChanged();
            }
        }

        private string _updateImage;

        public string UpdateImage
        {

            get {
                if (HasUpdate)
                    return "bell.png";
                else
                    return "";
            }
           
        }

        private string _estado;

        public string Estado
        {
            get { return _estado; }
            set {
                _estado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsResuelto));
                OnPropertyChanged(nameof(IsOpen));
                OnPropertyChanged(nameof(OpenImage));
            }
        }

        private string _message;

        public string Message
        {
            get { return _message; }
            set {
                _message = value;
                OnPropertyChanged();

            }
        }

        private bool _isResuelto;

        public bool IsResuelto
        {
            get {
                _isResuelto = false;
                if (Estado == "Resuelto")
                    _isResuelto = true;
                return _isResuelto; }
       
        }

        private string _ultimaRespuesta;

        public string UltimaRespuesta
        {
            get {
                return _ultimaRespuesta;
            }
            set {
                _ultimaRespuesta = value;
                OnPropertyChanged();

            }
        }

        private int _respuestas;

        public int Respuestas
        {
            get { return _respuestas; }
            set { _respuestas = value; }
        }





        public string Date { get; set; }
       
        public string Area { get; set; }
        
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
        
       

        
        public void Check()
        {
            Console.WriteLine("Check()");
            Console.WriteLine("|| =====> Estado: " + Estado);
      
        }
    }
}
