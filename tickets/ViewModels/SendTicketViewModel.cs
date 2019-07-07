using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using tickets.API;

namespace tickets.ViewModels
{
    class SendTicketViewModel
    {
        public Ticket Ticket { set; get; }
        public ObservableCollection<string> Gestiones { set; get; }
        public ObservableCollection<string> ComoPodemosAyudarte { set; get; }

        private Server _server;
        private Dictionary<string, string> _gestiones;
        private Dictionary<string, string> _comoPodemosAyudarte;


        private List<string> GetGestiones()
        {
           

            return null;
        }
    }
}
