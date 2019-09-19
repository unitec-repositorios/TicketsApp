using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using tickets.API;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace tickets.ViewModels
{
    public class DetalleTicketViewModel:Ticket
    {
        //private Database _database=new Database();
        //private Server _server=new Server();
        private Ticket _modelo;

        public Command ActualizarEstadoCommand { get; set; }
        public Command GoToWebCommand { get; set; }

        public DetalleTicketViewModel(Ticket _ticket=null)
        {
            _modelo = _ticket;
           // Console.WriteLine("ID Ticket"+ID);
           // Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject((Ticket)this));
            InitModel();
            ActualizarEstadoCommand = new Command(async () => await ActualizarEstadoTicket());
            GoToWebCommand = new Command(async () => await GoToWeb(), () => !IsBusy);
        }

        public bool IsBusy { get; set; }
       
        private async Task ActualizarEstadoTicket()
        {
            //  if (!IsBusy)
            //{
            //  IsBusy = true;
            //var isSucess = await _server.changeStatusTicket(ID);
            //if (isSucess)
            //{
            Console.WriteLine($"Cambiando Open de {IsOpen} a {!IsOpen}");
           
                    var actualValue = IsOpen;
                    IsOpen = !actualValue;
                  //  var _ticket = await _server.GetTicket(ID);
                    //await _database.ActualizarTicket(_ticket);
                    //_modelo = _ticket;
                    //InitModel();
        //    //    }
               //IsBusy = false;
         //   }

        }

        private async Task GoToWeb()
        {
            if (!IsBusy)
            {
                IsBusy = true;
               // var refresh = await _server.getRefreshCode();
                //var uri = $"{_server.GetBaseAdress()}/ticket.php?track={ID}&Refresh={refresh}";
                IsBusy = false;
                //await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
          
        }

        private void InitModel()
        {
            ID = _modelo.ID;
            Subject = _modelo.Subject;
            Estado = _modelo.Estado;

            IsOpen = _modelo.IsOpen;

            CreationDate= _modelo.CreationDate;
            UltimaRespuesta= _modelo.UltimaRespuesta;
            LastUpdate = _modelo.LastUpdate;

            Respuestas = _modelo.Respuestas;
            Category = _modelo.Category;
            Priority = _modelo.Priority;

            Message = _modelo.Message;

      
        }


    }
}
