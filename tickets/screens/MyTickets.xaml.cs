using Acr.UserDialogs;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using tickets.API;
using tickets.ViewModels;
using tickets.Views;
using Xamarin.Forms;

namespace tickets
{
    public partial class MyTickets : ContentPage
    {
        private Server server = new Server();

        private ObservableCollection<Ticket> tickets;
        public Command AddTicketCommand { get; set; }

        //  SendTicket view_sendTicket = new SendTicket();

        public MyTickets()
        {
            try
            {


                // App.Database.ClearTicket();
                InitializeComponent();
                App.Current.MainPage = new NavigationPage(new ListTicketsView());
                BindingContext = new ListTicketsViewModel();
       /*         AddTicketCommand = new Command(async() => await addTicketIdAsync());
         
                if (tickets == null)
                {
                    tickets = new ListTicketsViewModel().ListTickets;
                     GetTickets();
                }

                GetTickets();
                BindingContext = this;

                TicketsListView.ItemsSource = tickets;
              
                */

            }
            catch (Exception ex)
            {

                Application.Current.MainPage.DisplayAlert("Error", "Se ha producido un error insperado, daremos soporte lo mas antes posible\nDetalles Error:" + ex.InnerException, "Aceptar");

            }


        }

        /*
              private async Task addTicketIdAsync()
              {
                  try
                  {
                      UserDialogs.Instance.HideLoading();
                      var promptConfig = new PromptConfig()
                      {
                          InputType = InputType.Name,
                          IsCancellable = true,
                          Message = "Ingrese el ID del Ticket",
                          Placeholder = "Id Ticket"
                      };


                      var result = await UserDialogs.Instance.PromptAsync(promptConfig);

                      //  UserDialogs.Instance.ShowLoading("Por favor espere");

                      if (result.Ok)
                      {

                          // await Task.Delay(200);
                          if (string.IsNullOrEmpty(result.Text))
                          {
                              UserDialogs.Instance.ShowError("Ingrese un id.");
                          }
                          else
                          {

                              await Task.Delay(100);
                              Console.WriteLine("Buscando..");
                              var t = await server.GetTicket(result.Text);
                              Console.WriteLine("Ticket Encontrado");
                              Ticket db_t = await App.Database.GetTicket(result.Text);

                              UserDialogs.Instance.HideLoading();
                              if (t == null)
                              {
                                  UserDialogs.Instance.ShowError("No existe un ticket con el ID: " + result.Text);
                              }
                              else if (db_t != null)
                              {
                                  UserDialogs.Instance.ShowError("No se agrego el ticket, porque ya existe en la base de datos.");
                              }
                              else
                              {
                                  // t.Check();
                                 await App.Database.AgregarTicket(t);
                              //    Console.WriteLine("addTicket/addTicketDatabase");
                                  await Task.Delay(200);
                                  GetTickets();
                                  UserDialogs.Instance.ShowSuccess("Ticket Agregado!");
                              }
                          }
                      }
                  }
                  catch (Exception ex)
                  {
                      UserDialogs.Instance.Loading();
                      await Application.Current.MainPage.DisplayAlert("Error", "Se ha producido un error insperado, daremos soporte lo mas pronto posible\nDetalles Error:" + ex.Message, "Aceptar");
                      UserDialogs.Instance.HideLoading();
                  }

              }
      */

        private async Task addTicketIdAsync()
        {
            Console.WriteLine("ADD TICKET FROM ID");
            var promptConfig = new PromptConfig()
            {
                InputType = InputType.Name,
                IsCancellable = true,
                Message = "Ingrese el ID del Ticket",
                Placeholder = "Id Ticket"
            };
            var loading = new Task(() => UserDialogs.Instance.ShowLoading("Por favor espere"));
            var result = await UserDialogs.Instance.PromptAsync(promptConfig);

            if (result.Ok)
            {
                Console.WriteLine("Presionaste OK");
                loading.Start();
                // await Task.Delay(200);
                if (string.IsNullOrEmpty(result.Text))
                {
                    UserDialogs.Instance.ShowError("Ingrese un id.");
                }
                else
                {
                    // await Task.Delay(200);
                    //User currentUser = App.Database.GetCurrentUser();
                    Ticket t = await server.GetTicket(result.Text);
                    Ticket db_t = await App.Database.GetTicket(result.Text);
                    //  UserDialogs.Instance.HideLoading();
                    if (t == null)
                    {
                        UserDialogs.Instance.ShowError("No existe un ticket con el ID: " + result.Text);
                    }
     
                    else if (db_t != null)
                    {
                        UserDialogs.Instance.ShowError("No se agrego el ticket, porque ya existe en la base de datos.");
                    }
                    else
                    {
                        // t.Check();
                        App.Database.AgregarTicket(t);
                        await Task.Delay(500);
                        GetTickets();
                        UserDialogs.Instance.ShowSuccess("Ticket Agregado!");
                    }
                }
            }

        }



        public async void goToViewTicket(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                goToViewTicketAsync((Ticket)e.SelectedItem);
            }
        }
        public async void goToViewTicketAsync(Ticket _ticket)
        {
            if (_ticket != null)
            {
             //   UserDialogs.Instance.Loading();
              
                TicketsListView.SelectedItem = null;
               
                    _ticket.HasUpdate = false;

           
                
              
                chatTicket _chatTicket = new chatTicket() { BindingContext = _ticket.ID };


                UserDialogs.Instance.HideLoading();
                await App.Database.UpdateTicket(_ticket);
                await Navigation.PushAsync(_chatTicket);
                  
                   
                    


             //   }
             //   else
            //    {
              //      Debug.WriteLine("Ticket is null");
              //  }
            }
        }

        private void TicketsListView_Refreshing(object sender, EventArgs e)
        {

            this.GetTickets();
            TicketsListView.EndRefresh();
        }
        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            //List<Ticket> tickets = await App.Database.GetTicketsAsync(App.Database.GetCurrentUserNotAsync());

            if (!String.IsNullOrWhiteSpace(e.NewTextValue))
            {
                var showTickets = tickets.Where(t => t.Subject.Contains(e.NewTextValue)).ToList();
                TicketsListView.ItemsSource = showTickets;
            }
            else
            {
                TicketsListView.ItemsSource = tickets;
            }
        }

        public async void GetTickets()
        {
            try
            {
                var temp_ticket = tickets;
                if (temp_ticket == null)
          //          temp_ticket = new ListTicketsViewModel().ListTickets;
                foreach (Ticket t in temp_ticket)
                {

                    Ticket sv_ticket = await server.GetTicket(t.ID);

                    if (sv_ticket == null)
                    {
                        App.Database.EliminarTicket(t);
                        continue;
                    }

                    if (t.IsOpen != sv_ticket.IsOpen)
                    {
                      //  t.IsOpen = sv_ticket.IsOpen;
                        if (t.IsOpen)
                        {
              //              t.OpenImage = "";
                        }
                        else
                        {
                 //           t.OpenImage = "lock.png";
                        }
                        await App.Database.ActualizarTicket(t);
                    }
                    if (t.IsOpen == false && t.OpenImage.Equals(""))
                    {
                  //      t.OpenImage = "lock.png";
                        await App.Database.ActualizarTicket(t);
                    }
                    if (t.LastUpdate != sv_ticket.LastUpdate)
                    {
                        sv_ticket.HasUpdate = true;
                        await App.Database.ActualizarTicket(sv_ticket);
                    }
                }
              //  tickets = new ListTicketsViewModel().ListTickets;
                TicketsListView.ItemsSource = tickets;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async void TicketsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                 goToViewTicketAsync((Ticket)e.SelectedItem);
            }
        }

        private void ToolbarItem_Nuevo(object sender, EventArgs e)
        {
              Navigation.PushAsync(new SendTicket());
        }

        private void ToolbarItem_Settings(object sender, EventArgs e)
        {
            Navigation.PushAsync(new AppSettingsPage());
        }

        ////////////////8654658457845724954
        ///erth5rfgerhfgvhbbfhrtbfhtgfuhrtgfyhdfghgfn845745785485728451

    }
}
