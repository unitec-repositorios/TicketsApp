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

namespace tickets
{
    public partial class MyTickets : ContentPage
    {
        private Server server = new Server();
        ObservableCollection<Ticket> tickets = new ObservableCollection<Ticket>();

        public MyTickets()
        {
            try
            {
                InitializeComponent();
                this.BindingContext = this;
                
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

                var addTicketTool = new ToolbarItem
                {

                    Text = "Agregar Ticket",
                    Command  = new Command(execute: () => addTicketId()),

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
                //TicketsListView.BeginRefresh();
                //GetTickets();
                //TicketsListView.ItemsSource = tickets;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }


        private void addTicketId()
        {
            popupAddTicketId.IsVisible = true;
        }

        private void cancel_Popup(object sender, EventArgs e)
        {
            popupAddTicketId.IsVisible = false;
        }

        private string getDetailTicket(string html,string search)
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

        private async void addTicketID_Popup(object sender, EventArgs e)
        {
            User current = await App.Database.GetCurrentUser();
            Console.WriteLine("ID del entry del pop-up: " + entryId.Text);
            string html = await server.getDetailsTicket(entryId.Text);
            string account = getDetailTicket(html,"Numero de cuenta / No. de talento humano:");
            string error = "No se agrergo el ticket,los numeros de cuentas no coinciden";
            Console.WriteLine("Id ticket del server: " + account);
            Console.WriteLine("Id ticket de la base de datos local: " + current.Account);
            if (account==current.Account)
            {
                string date = await server.getInitDate(entryId.Text);
                string c = getDetailTicket(html, "Clasificacion:");
                int clas = 5;
                if (c == "Solicitud")
                {
                    clas = 1;
                }
                else if(c== "Información")
                {
                    clas = 2;
                }
                else if (c == "Queja")
                {
                    clas = 3;
                }
                else if (c == "Reclamo")
                {
                    clas = 4;
                }
                string prioridad = getDetailTicket(html, "Prioridad:");
                int p = 3;
                if(prioridad=="Alto")
                {
                    p = 1;
                }
                else if(prioridad=="Medio")
                {
                    p = 2;
                }
                try
                {
                    await App.Database.CreateNewTicket(new Ticket()
                    {
                        ID = entryId.Text,
                        UserID = current.ID,
                        Affected = int.Parse(getDetailTicket(html, "Cantidad de usuarios afectados:")),
                        Classification = clas,
                        Priority = p,
                        Subject = getDetailTicket(html, "Tema"),
                        Message = getDetailTicket(html, "<b>Mensaje:</b>"),
                        Date = date,
                    });
                    error = "El ticket se agrego exitosamente";
                }
                catch (SQLiteException )
                {
                    error = "No se agrergo el ticket,porque ya existe en la aplicacion";
                }
            }
            popupAddTicketId.IsVisible = false;
            await DisplayAlert("Agregar Ticket", error,"OK");
            entryId.Text = "";
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
                
                List<Ticket> dbtickets = await App.Database.GetTicketsAsync(App.Database.GetCurrentUserNotAsync());
                dbtickets = new List<Ticket>(dbtickets.Where(t => t.Date != "error").OrderByDescending(t => DateTime.ParseExact(t.Date, "yyyy-MM-dd HH:mm:ss",
                                                                                                  System.Globalization.CultureInfo.InvariantCulture)));

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
                TicketsListView.ItemsSource = null;
                TicketsListView.ItemsSource = tickets.Where(t => t.Date != "error").OrderByDescending(t => DateTime.ParseExact(t.Date, "yyyy-MM-dd HH:mm:ss",
                            System.Globalization.CultureInfo.InvariantCulture));
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
