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

namespace tickets
{
    public partial class MyTickets : ContentPage
    {
        private Server server = new Server();
        Ticket t;
        //ObservableCollection<Ticket> tickets = new ObservableCollection<Ticket>();
        List<Ticket> tickets = new List<Ticket>();


        public MyTickets()
        {
            try
            {
                InitializeComponent();
                
                TicketsListView.ItemsSource = tickets;
                this.BindingContext = this;
                GetTickets();


                var newTicket = new ToolbarItem
                {
                    Icon = "nuevo.jpg",
                    Command = new Command(async (s) => await Navigation.PushAsync(new SendTicket())),
                    Order = ToolbarItemOrder.Primary

                };
                //GetTickets();

                var settings = new ToolbarItem
                {
                    
                    Text = "Ajustes",
                    Command = new Command(async (s) => await Navigation.PushAsync(new AppSettingsPage())),
                      
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
                        break;
                    case Device.UWP:
                        ToolbarItems.Add(newTicket);
                        ToolbarItems.Add(settings);
                        break;
                }
                //TicketsListView.BeginRefresh();
                //GetTickets();
                //TicketsListView.ItemsSource = tickets;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Device.StartTimer(new TimeSpan(0, 0, AppSettings.RefreshTicketsTimeout), () =>
              {
                  //Get tickets every 1 minute.
                  GetTickets();
                  return true;
              });
        }

        protected override async void OnDisappearing()
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
                    TicketsListView.SelectedItem = null;
                    UserDialogs.Instance.HideLoading();
                }
                else
                {
                    Debug.WriteLine("Ticket is null");
                }
            }
        }

        private async void TicketsListView_Refreshing(object sender, EventArgs e)
        {

            //TicketsListView.ItemsSource = await GetTickets();
            GetTickets();
            TicketsListView.EndRefresh();
        }

        private async void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
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
            try {
               
                //List<Ticket> dbtickets = await App.Database.GetTicketsAsync(App.Database.GetCurrentUserNotAsync());
                List<Ticket> dbtickets;
                dbtickets = await App.Database.GetTicketsAsync();
                //dbtickets = new List<Ticket>(dbtickets.Where(t => t.Date != "error").OrderByDescending(t => DateTime.ParseExact(t.Date, "yyyy-MM-dd HH:mm:ss",
                //                                                                                System.Globalization.CultureInfo.InvariantCulture)));
                
                for (int i = 0; i < dbtickets.Count; i++)
                {
                    String updateDate = await server.getUpdateDate(dbtickets[i].ID);
                    if (!updateDate.Equals(dbtickets[i].Date) && updateDate != "error")
                    {
                        dbtickets[i].Image = "https://cdn.pixabay.com/photo/2015/12/16/17/41/bell-1096280_640.png";
                        dbtickets[i].Date = updateDate;
                        await App.Database.UpdateTicket(dbtickets[i]);
                    }
                    else
                        dbtickets[i].Image = "";


                    bool open = await server.getOpenTicket(dbtickets[i].ID);
                    Console.WriteLine("Recibiendo del sevidor: "+ open.ToString());
                    if (!open)
                    {
                        dbtickets[i].OpenImage = "https://cdn.pixabay.com/photo/2015/12/08/19/08/castle-1083570_960_720.png";
                        dbtickets[i].Open = open;
                        await App.Database.UpdateTicket(dbtickets[i]);
                    }
                    else
                        dbtickets[i].OpenImage = "";

                    var exists = tickets.FirstOrDefault(t => t.ID == dbtickets[i].ID);

                    if (exists == null) // if no ticket was found with that id
                    {
                        tickets.Add(dbtickets[i]);
                    }
                    else
                    {
                        exists.Image = dbtickets[i].Image;

                        exists.OpenImage = dbtickets[i].OpenImage;


                        if (!updateDate.Equals(exists))
                        {
                            exists.Date = updateDate;
                        }

                    }
                }
               
                //foreach (var t in dbtickets)
                //tickets.Add(t);
                TicketsListView.ItemsSource = null;
                TicketsListView.ItemsSource = tickets;
                //TicketsListView.ItemsSource = tickets.Where(t => t.Date != "error").OrderByDescending(t => DateTime.ParseExact(t.Date, "yyyy-MM-dd HH:mm:ss",
                //           System.Globalization.CultureInfo.InvariantCulture));
                //tickets = new ObservableCollection<Ticket>(
                //        tickets.Where(t => t.Date != "error").OrderByDescending(t => DateTime.ParseExact(t.Date, "yyyy-MM-dd HH:mm:ss",
                //            System.Globalization.CultureInfo.InvariantCulture))
                //        );

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
