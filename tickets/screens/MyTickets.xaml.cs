using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using tickets.API;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Timers;
using Acr.UserDialogs;
using SQLite;
using System.Net.Http;
using System.Text;
using tickets.ViewModels;

namespace tickets
{
    public partial class MyTickets : ContentPage
    {
        private Server server = new Server();

        private ObservableCollection<Ticket> tickets;
        
      //  SendTicket view_sendTicket = new SendTicket();

        public  MyTickets()
        {
            try
            {

                                                            
               // App.Database.ClearTicket();
                InitializeComponent();
                 
                InitOtherComponents();
               
                tickets = new ListTicketsViewModel().ListTickets;
               
                 if (tickets == null)
                {
                    tickets = new ListTicketsViewModel().ListTickets;
                  //  GetTickets();
                }
               

                TicketsListView.ItemsSource=tickets ;
                GetTickets();
                this.BindingContext =this;
                

                 UserDialogs.Instance.HideLoading();


            }
            catch(Exception ex)
            {
                throw ex;
            }

            
        }


        private void InitOtherComponents()
        {
            var newTicket = new ToolbarItem
            {
                Icon = "nuevo.jpg",
                Command = new Command(async () => await  Navigation.PushAsync(new SendTicket())),

                Order = ToolbarItemOrder.Primary

            };

            var settings = new ToolbarItem
            {
                Text = "Ajustes",
                Command = new Command(async (s) => await Navigation.PushAsync(new AppSettingsPage())),
                Order = ToolbarItemOrder.Secondary
            };

            var addTicketTool = new ToolbarItem
            {
                Text = "Agregar Ticket",
                Command = new Command(async() => await addTicketIdAsync()),
                Order = ToolbarItemOrder.Secondary
            };


            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    ToolbarItems.Add(newTicket);
                    break;
                case Device.Android:
                    ToolbarItems.Add(newTicket);
                    ToolbarItems.Add(settings);
                    ToolbarItems.Add(addTicketTool);
                    break;
                case Device.UWP:
                    ToolbarItems.Add(newTicket);
                    ToolbarItems.Add(settings);
                    break;
            }
        }

        private async Task addTicketIdAsync()
        {
            Console.WriteLine("ADD TICKET FROM ID");
            var promptConfig = new PromptConfig()
            {
                InputType = InputType.Name,
                IsCancellable = true,
                Message = "Ingrese el ID del Ticket",
                Placeholder="Id Ticket"
            };
            var loading = new Task(() => UserDialogs.Instance.ShowLoading("Por favor espere"));
            var result = await UserDialogs.Instance.PromptAsync(promptConfig) ;
            
            if (result.Ok)
            {
                loading.Start();
               // await Task.Delay(200);
                if (string.IsNullOrEmpty(result.Text))
                {
                    UserDialogs.Instance.ShowError("Ingrese un id.");
                }
                else
                {
                   // await Task.Delay(200);
                    User currentUser = App.Database.GetCurrentUser();
                    Ticket t = await server.GetTicket(result.Text);
                    Ticket db_t = await App.Database.GetTicket(result.Text);
                  //  UserDialogs.Instance.HideLoading();
                    if (t == null)
                    {
                        UserDialogs.Instance.ShowError("No existe un ticket con el ID: " + result.Text);
                    }
                    else if (t.UserID != int.Parse(currentUser.Account))
                    {
                        UserDialogs.Instance.ShowError("No se agrego el ticket, su numero de cuenta no coincide con el numero de cuenta enlazado al ticket");
                    }
                    else if(db_t!=null)
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

        //FUNCIONES AGREGAR TICKET DESDE ID
        private string getDetailTicket(string html, string search)
        {
            int pos = html.IndexOf(search) + search.Length;
            html = html.Substring(pos);
            pos = 0;
            string detail = "";
            if (search == "Tema")
            {
                search = "<b>";
                pos = html.IndexOf(search) + search.Length;
            }
            else if (search == "<b>Mensaje:</b>")
            {
                search = "<br />";
                pos = html.IndexOf(search) + search.Length;
            }
            else
            {
                pos = pos + 1;
            }
            detail = server.getTextAux('<', html, pos);
            Console.WriteLine("Detalle: " + detail);
            return detail;
        }



        private bool getIDAccount(string html,string numberAccount)
        {
            return html.Contains("No. de talento Humano: " + numberAccount+"<");
        }


        //TERMINAN FUNCIONES



        protected override  void OnAppearing()

        {
            base.OnAppearing();

           
           /*
            Device.StartTimer(new TimeSpan(0, 0, AppSettings.RefreshTicketsTimeout), () =>
              {
                  if (!view_sendTicket.sentTicket)
                  {
                      Console.WriteLine("\n\n" + "sent ticket:\t" + view_sendTicket.sentTicket + "\n\n");
                      GetTickets();
                      
                  }
                  return true;

              });*/
        }

        protected override void OnDisappearing()

        {
            base.OnDisappearing();
        }

        async void goToViewTicket(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                var ticket = tickets.FirstOrDefault(t => t.ID == ((Ticket)e.SelectedItem).ID);
                if (ticket != null)
                {
                    UserDialogs.Instance.ShowLoading("Cargando Ticket...");
                    Debug.WriteLine("Opening messages for ticket with id = " + ticket.ID);
                    ticket.Date = await server.getUpdateDate(ticket.ID);
                    ticket.Image = "";

                    ticket.OpenImage = "";

                    await App.Database.UpdateTicket(ticket);
                    await Navigation.PushAsync(new chatTicket()
                    {
                        BindingContext = ticket.ID
                    });
                    //TicketsListView.SelectedItem = null;
                    
                    UserDialogs.Instance.HideLoading();
                }
                else
                {
                    Debug.WriteLine("Ticket is null");
                }
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
                foreach(Ticket t in temp_ticket)
                {

                    Ticket sv_ticket =  await server.GetTicket(t.ID);

                    if (sv_ticket == null)
                    {
                        App.Database.EliminarTicket(t);
                        continue;
                    }

                        Console.WriteLine("\nMyTickets.xaml.cs/GetTicket");
                        Console.WriteLine("\nID TICKET : "+t.ID+"\tSubject: "+t.Subject+"\nServer-Ticket.Open = "+sv_ticket.Open+"\tDB-Ticket.Open = "+t.Open);
                    if (t.Open != sv_ticket.Open){
                            t.Open = sv_ticket.Open;
                            if (t.Open){
                                t.OpenImage = "";
                            }
                            else
                            {
                            t.OpenImage = "lock.png";
                            }
                            App.Database.ActualizarTicket(t);
                        }
                        if (t.Open == false && t.OpenImage.Equals(""))
                        {
                            t.OpenImage = "lock.png";
                            App.Database.ActualizarTicket(t);
                        }
                        if (t.LastUpdate != sv_ticket.LastUpdate)
                        {
                            sv_ticket.Image = "bell.png";
                            App.Database.ActualizarTicket(sv_ticket);
                        }   
                }
                tickets = new ListTicketsViewModel().ListTickets;
                TicketsListView.ItemsSource = tickets;


                ///OLD CODE
                /*
                List<Ticket> dbtickets;
                
             //   tickets.Clear();
                dbtickets = await App.Database.GetTicketsAsync();
               // Ticket ticket = await server.GetTicket("J95JZ468VQ");
             //   Console.WriteLine("\nID TICKET: " + ticket.ID + "\nUser ID: " + ticket.UserID+ "\nFecha Creacion: "+ticket.CreationDate+"\nUltima actualizacion: "+ticket.LastUpdate);

                    for (int i = 0; i < dbtickets.Count; i++) {
                        //    Console.WriteLine("DBTICKETS COUNT: " +dbtickets.Count);
                        String updateDate = await server.getUpdateDate(dbtickets[i].ID);
                        Console.WriteLine("Recibiendo del sevidor para notificacion: " + updateDate.ToString());
                        Console.WriteLine("\nTEST GET TICKET ");

                        
                        if (!updateDate.Equals(dbtickets[i].Date) && updateDate != "error") {

                            dbtickets[i].Image = "bell.png";
                            dbtickets[i].Date = updateDate;
                            await App.Database.UpdateTicket(dbtickets[i]);
                        }
                        else
                            dbtickets[i].Image = "";

                        bool open = await server.getOpenTicket(dbtickets[i].ID);
                        Console.WriteLine("Recibiendo del sevidor: " + open.ToString());
                        if (!open) {
                            dbtickets[i].OpenImage = "lock.png";
                            dbtickets[i].Open = open;
                            await App.Database.UpdateTicket(dbtickets[i]);
                        }
                        else
                            dbtickets[i].OpenImage = "";


                        var exists = tickets.FirstOrDefault(t => t.ID == dbtickets[i].ID);

                        if (exists == null) {
                            tickets.Add(dbtickets[i]);
                        }
                        else {
                            exists.Image = dbtickets[i].Image;
                            exists.OpenImage = dbtickets[i].OpenImage;
                            if (!updateDate.Equals(exists)) {
                                exists.Date = updateDate;
                            }

                        }
                    }
               
                tickets = new ListTicketsViewModel().ListTickets;
               
                TicketsListView.ItemsSource = tickets;               
                *///
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
