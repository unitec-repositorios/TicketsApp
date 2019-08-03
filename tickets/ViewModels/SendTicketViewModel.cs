using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using tickets.API;

namespace tickets.ViewModels
{
    public class SendTicketViewModel:Ticket
    {
        
        public ObservableCollection<string> Gestiones { set; get; }
        public ObservableCollection<string> ComoPodemosAyudarte { set; get; }

        private Server _server;
        private Dictionary<string, object> _gestiones { set; get; }
        private Dictionary<string, string> _comoPodemosAyudarte { set; get; }

        public SendTicketViewModel()
        {
            _server = new Server();
        }
        private List<string> GetGestiones()
        {
            _gestiones = _server.GetDictionaryAreas();
            List<string> _tempGestiones=new List<string>();
            foreach(var item in _gestiones)
            {
                _tempGestiones.Add(item.Key);
            }
            return _tempGestiones;
        }

       
    }
}
