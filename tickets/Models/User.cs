using System;
using System.ComponentModel;
using SQLite;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace tickets
{
    public class User:INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(name));
        }

        private int _id;
        private string _name;
       
        private string _fullName;

        private string _email;
        private string _campus;
        private string _profile;
        private string _account;
        private string _career;
        private string _phoneNumber;
        private bool _isCurrent;
        private string _personalEmail;



        private bool _isBusy=false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set {
                _isBusy = value;
                OnPropertyChanged();
            }
        }


        [PrimaryKey, AutoIncrement]
        public int ID {
            get {
                return _id;
            }
            set {
                _id = value;
                OnPropertyChanged();
            }
        }
        public string Name {
            get {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }
        public string Email {
            get {
                return _email;
            }
            set {
                _email = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }
        public string Campus {
            get {
                return _campus; }
            set {
                _campus =value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }

        }
        public string Profile {
            get {
                return _profile;
            }
            set {
                _profile = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }
        public string Account {
            get {
                return _account;
            }
            set {
                _account = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            } }
        public string Career {
            get {
                return _career; }
            set {
                _career = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }
        public string PhoneNumber {
            get {
                return _phoneNumber;
            }
            set {
                _phoneNumber = value;
                OnPropertyChanged();
            }
        }
        public string PersonalEMail
        {
            get { return _personalEmail; }
            set
            {
                _personalEmail = value;
                OnPropertyChanged();
            }
        }
        public bool IsCurrent {
            get {
                return _isCurrent;
            }
            set {
                _isCurrent = value;
                OnPropertyChanged();
            }
        }
       
        public string FullName
        {
            get { return _fullName; }
            set {
                _fullName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }
        public bool IsValid
        {
            get
            {
                return !(string.IsNullOrEmpty(Name)     || string.IsNullOrEmpty(Email)      || string.IsNullOrEmpty(Campus)
                      || string.IsNullOrEmpty(Profile)  || string.IsNullOrEmpty(Account)    || string.IsNullOrEmpty(Career) || string.IsNullOrEmpty(PhoneNumber));
            }
            
        }
    }
}
  