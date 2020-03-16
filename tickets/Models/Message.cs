using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MvvmHelpers;

namespace tickets.Models
{
    public class Message : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private string autor;
        private DateTime date;
        private string text;
        private bool esPropio;



        private string _idTicket;

        public string IdTicket
        {
            get
            {
                return _idTicket;
            }
            set {
                _idTicket = value;
                OnPropertyChanged();
            }
        }

        public bool EsPropio
        {
            get
            {
                return esPropio;
            }
            set {
                esPropio = value;
                OnPropertyChanged();
 }
        }


        public string Text
        {
            get
            {
                return text;
            }
            set {
                text = value;
                OnPropertyChanged();
            }
        }

        public DateTime Date
        {
            get
            {
                return date;
            }
            set
            {
                date = value;
                OnPropertyChanged();
            }
        }

        public string Autor
        {
            get {
                return autor;
            }
            set {
                autor = value;
                OnPropertyChanged();
            }

        }

        public string OutText
        {
            get
            {
                return Autor + "\n" + Text;
            }
        }

        
    }
}
