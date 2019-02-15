﻿ 
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
using HtmlAgilityPack;
using System.Net.Http;

namespace tickets
{
    public partial class MyTicketsAdmin : TabbedPage
    {
        private Server server = new Server();
        ObservableCollection<Ticket> tickets = new ObservableCollection<Ticket>();
        private Timer refreshTicketsTimer;
        
        public MyTicketsAdmin()
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
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void TimerFunction(object source, ElapsedEventArgs e)
        {
            GetTickets();
        }

        private void SetTimer()
        {
            refreshTicketsTimer = new Timer(AppSettings.RefreshTicketsTimeout * 1000);
            refreshTicketsTimer.Elapsed += TimerFunction;
            refreshTicketsTimer.AutoReset = true;
            refreshTicketsTimer.Enabled = true;
            GetTickets();
        }

        private void ClearTimer()
        {
            refreshTicketsTimer.Stop();
            refreshTicketsTimer.Dispose();
        }

        protected override async void OnAppearing()
        {
            SetTimer();
        }

        protected override async void OnDisappearing()
        {
            ClearTimer();
        }
        //Tickets Enviados
        async void goToViewTicketAdmin(object sender, SelectedItemChangedEventArgs e)
        {

        }

        private async void TicketsListView_RefreshingAdmin(object sender, EventArgs e)
        {
            GetTickets();
            TicketsListViewAdmin.ItemsSource = null;
            TicketsListViewAdmin.ItemsSource = tickets.Where(t => t.Date != "error").OrderByDescending(t => DateTime.ParseExact(t.Date, "yyyy-MM-dd HH:mm:ss", 
                        System.Globalization.CultureInfo.InvariantCulture));
            TicketsListViewAdmin.EndRefresh();
        }

        private async void SearchBar_TextChangedAdmin(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.NewTextValue))
            {
                var showTickets = tickets.Where(t => t.Subject.Contains(e.NewTextValue)).ToList();
                TicketsListViewAdmin.ItemsSource = showTickets;
            }
            else
            {
                TicketsListViewAdmin.ItemsSource = tickets;
            }
        }

        //Tickets Asignados
        async void goToViewTicketAdminAssign(object sender, SelectedItemChangedEventArgs e)
        {
            
        }

        private async void TicketsListView_RefreshingAdminAssign(object sender, EventArgs e)
        {
            GetTickets();
            TicketsListViewAdminAssign.EndRefresh();
        }

        private async void SearchBar_TextChangedAdminAssign(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.NewTextValue))
            {
                var showTickets = tickets.Where(t => t.Subject.Contains(e.NewTextValue)).ToList();
                TicketsListViewAdminAssign.ItemsSource = showTickets;
            }
            else
            {
                TicketsListViewAdminAssign.ItemsSource = tickets;
            }
        }

        public async void GetTickets()
        {
            //Por ahora obtiene todos los tickets abiertos, se debe cambiar a solo los tickets abiertos asignados al usuario admin
            
            User user = await App.Database.GetCurrentUser();

            var requestURI = @"http://138.197.198.67/admin/index.php";
            //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            HttpClient httpClient = new HttpClient();
            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            //httpClient.BaseAddress = new Uri("https://178.128.75.38/");
            var parameters = new Dictionary<string, string>();
            parameters["user"] = "administrator";
            parameters["pass"] = "admin";
            parameters["remember_user"] = "NOTHANKS";
            parameters["a"] = "do_login";
            var response = await httpClient.PostAsync(requestURI, new FormUrlEncodedContent(parameters));
            var contents = await response.Content.ReadAsStringAsync();
            Debug.WriteLine("Contents: {0}", contents);

            
            IEnumerable<String> headerVals;
            string session = string.Empty;
            if (response.Headers.TryGetValues("Set-Cookie", out headerVals))
            {
                session = headerVals.First();
            }
            Debug.WriteLine("!!!COOKIE: {0} !!!", session);
            // MultipartFormDataContent form = new MultipartFormDataContent();

            // String res = capture.Headers.ElementAt(3).Value.ElementAt(0).ToString();
            // String[] tokens = res.Split(';');
            // String cookie = tokens[0];

            // String[] tokensValue = cookie.Split('=');
            // String valueCookie = tokensValue[1];

            // var htmlDoc = new HtmlDocument();
            // htmlDoc.LoadHtml(await capture.Content.ReadAsStringAsync());

            // var node = htmlDoc.DocumentNode.SelectSingleNode("//input[@name='token']");
            List<Ticket> openTickets = await App.Database.GetOpenTicketsAsync();
            openTickets = new List<Ticket>(openTickets.Where(t => t.Date != "error").OrderByDescending(t => DateTime.ParseExact(t.Date, "yyyy-MM-dd HH:mm:ss",
                                                                                                  System.Globalization.CultureInfo.InvariantCulture)));
            for (int i = 0; i < openTickets.Count; i++)
            {
                String updateDate = await server.getUpdateDate(openTickets[i].ID);
                if (!updateDate.Equals(openTickets[i].Date) && updateDate != "error")
                {
                    openTickets[i].Image = "https://cdn.pixabay.com/photo/2015/12/16/17/41/bell-1096280_640.png";
                    openTickets[i].Date = updateDate;
                    await App.Database.UpdateTicket(openTickets[i]);
                }
                else
                    openTickets[i].Image = "";


                bool open = await server.getOpenTicket(openTickets[i].ID);
                Console.WriteLine("Recibiendo del sevidor: "+ open.ToString());
                if (!open)
                {
                    openTickets[i].OpenImage = "https://cdn.pixabay.com/photo/2015/12/08/19/08/castle-1083570_960_720.png";
                    openTickets[i].Open = open;
                    await App.Database.UpdateTicket(openTickets[i]);
                }
                else
                    openTickets[i].OpenImage = "";

                var exists = tickets.FirstOrDefault(t => t.ID == openTickets[i].ID);

                if (exists == null) // if no ticket was found with that id
                {
                    tickets.Add(openTickets[i]);
                }
                else
                {
                    exists.Image = openTickets[i].Image;

                    exists.OpenImage = openTickets[i].OpenImage;


                    if (!updateDate.Equals(exists))
                    {
                        exists.Date = updateDate;
                    }

                }
            }
            tickets = new ObservableCollection<Ticket>();
            //Tickets de prueba para verificar que el listado funciona
            //--------------------------------------------------------
            // Ticket t1 = new Ticket();
            // t1.Subject = "Hola";
            // t1.Priority = 1;
            // t1.Message = "ASasdad";
            // t1.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            // Ticket t2 = new Ticket();
            // t2.Subject = "Adios";
            // t2.Priority = 2;
            // t2.Message = "aver";
            // t2.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            // tickets.Add(t1);
            // tickets.Add(t2);
            TicketsListViewAdminAssign.ItemsSource = null;
            TicketsListViewAdminAssign.ItemsSource = tickets.Where(t => t.Date != "error").OrderByDescending(t => DateTime.ParseExact(t.Date, "yyyy-MM-dd HH:mm:ss", 
                        System.Globalization.CultureInfo.InvariantCulture));
        }
    }
}