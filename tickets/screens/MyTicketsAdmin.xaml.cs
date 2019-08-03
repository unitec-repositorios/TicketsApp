using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using tickets.API;
using tickets.Models;
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
        private AdminLogin admin_log = AdminLogin.Instance;
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
                    Command = new Command(async (s) => await Navigation.PushAsync(new AppSettingsPageAdmin())),

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
            //Llamado a GetTickets reemplaza a SetTimer() debido a que el request de sesión en GetTickets crea exception al repetirse
            GetTickets();
            //SetTimer();
        }

        protected override async void OnDisappearing()
        {
            // ClearTimer();
        }
        //Tickets Enviados
        async void goToViewTicketAdmin(object sender, SelectedItemChangedEventArgs e)
        {

        }

        private async void TicketsListView_RefreshingAdmin(object sender, EventArgs e)
        {
            GetTickets();
            // TicketsListViewAdmin.ItemsSource = null;
            // TicketsListViewAdmin.ItemsSource = tickets.Where(t => t.Date != "error").OrderByDescending(t => DateTime.ParseExact(t.Date, "yyyy-MM-dd HH:mm:ss", 
            //             System.Globalization.CultureInfo.InvariantCulture));
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

        public async void changeTicketStatus(string id)
        {
            var requestURI = @"http://138.197.198.67/admin/index.php";
            HttpClient httpClient = new HttpClient();
            var parameters = new Dictionary<string, string>();

            //Aquí se debe obtener la cookie en lugar de la sesión de login hardcoded que se realiza
            parameters["user"] = admin_log.username;
            parameters["pass"] = admin_log.password;
            parameters["remember_user"] = "NOTHANKS";
            parameters["a"] = "do_login";
            var response = await httpClient.PostAsync(requestURI, new FormUrlEncodedContent(parameters));
            var contents = await response.Content.ReadAsStringAsync();
            IEnumerable<String> headerVals;
            string session = string.Empty;
            if (response.Headers.TryGetValues("Set-Cookie", out headerVals))
            {
                session = headerVals.First();
            }
            await server.changeStatusTicket(id);

        }

        public async void GetTickets()
        {

            User user = await App.Database.GetCurrentUserAsync();

            var requestURI = @"http://138.197.198.67/admin/index.php";
            HttpClient httpClient = new HttpClient();
            var parameters = new Dictionary<string, string>();

            //Aquí se debe obtener la cookie en lugar de la sesión de login hardcoded que se realiza
            parameters["user"] = admin_log.username;
            parameters["pass"] = admin_log.password;
            parameters["remember_user"] = "NOTHANKS";
            parameters["a"] = "do_login";
            var response = await httpClient.PostAsync(requestURI, new FormUrlEncodedContent(parameters));
            var contents = await response.Content.ReadAsStringAsync();
            IEnumerable<String> headerVals;
            string session = string.Empty;
            if (response.Headers.TryGetValues("Set-Cookie", out headerVals))
            {
                session = headerVals.First();
            }

            //Modificar parametros del request para obtener tickets ordenados por columna
            requestURI = @"http://138.197.198.67/admin/show_tickets.php?status=6&sort=lastchange&category=0&s_my=1&s_ot=1&s_un=1&limit=10&asc=0";
            var res2 = await httpClient.GetAsync(requestURI);
            contents = await res2.Content.ReadAsStringAsync();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(contents);
            var table = htmlDoc.DocumentNode.SelectSingleNode("//table[@class=\"white\"]");
            tickets = new ObservableCollection<Ticket>();

            int hcount = 0;
            //Ciclar rows y crear tickets en la ObservableList
            foreach (HtmlNode row in table.SelectNodes("tr"))
            {
                if (hcount > 0)
                { //Ignore headers
                    int column = 0;
                    Ticket ticket = new Ticket();
                    foreach (HtmlNode cell in row.SelectNodes("th|td"))
                    {
                        //Por ahora solo se puede obtener ID, fecha de actualización y tema
                        //Los otros atributos están al ver un ticket específico, se necesita hacer otro request por cada
                        //ticket usando el ID para obtenerlos.
                        switch (column)
                        {
                            case 1:
                                ticket.ID = cell.InnerText;
                                break;
                            case 2:
                                ticket.Date = cell.InnerText;
                                break;
                            case 4:
                                ticket.Subject = cell.InnerText;
                                break;
                            default:
                                break;
                        }
                        column++;

                    }
                    tickets.Add(ticket);
                }
                else
                {
                    hcount = 1;
                }
            }

            TicketsListViewAdminAssign.ItemsSource = tickets;
        }
    }
}