using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using tickets.API;
using tickets.Views;
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
        public Command DeleteTicketCommand { get; set; }

        public DetalleTicketViewModel(Ticket _ticket=null)
        {
            _modelo = _ticket;
            InitModel();
            ActualizarEstadoCommand = new Command(async () => await ActualizarEstadoTicket(),()=>!IsBusy);
            GoToWebCommand = new Command(async () => await GoToWeb(), () => !IsBusy);
            DeleteTicketCommand = new Command(async () => await DeleteTicket(), () => !IsBusy);
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
                //    IsOpen = !actualValue;
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
                var server = new Server();
                var uri = await server.GetURLTicket(_modelo.ID);
                IsBusy = false;
                await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
          
        }

        private async Task DeleteTicket()
        {
            if (!IsBusy)
            {
                IsBusy = true;
                bool answer = await Application.Current.MainPage.DisplayAlert("Alerta!", "¿Estas seguro que deseas eliminar este ticket?", "Si", "No");
                if (answer)
                {
                    App.Database.EliminarTicket(_modelo);
                    IsBusy = false;
                    App.Current.MainPage = new NavigationPage(new ListTicketsView());

                }
                IsBusy = false;
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
