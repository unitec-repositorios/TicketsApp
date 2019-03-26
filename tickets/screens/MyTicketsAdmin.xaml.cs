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
using SQLite;
using HtmlAgilityPack;
using System.Net.Http;

namespace tickets
{
    public partial class MyTicketsAdmin : TabbedPage
    {
        private Server server = new Server();
        private AdminLogin admin_log = AdminLogin.Instance;
        List<Ticket> tickets = new List<Ticket>();
        ObservableCollection<Ticket> tickets_assign = new ObservableCollection<Ticket>();
        private Timer refreshTicketsTimer;

        public MyTicketsAdmin()
        {
            try
            {
                InitializeComponent();
                //UserDialogs.Instance.ShowLoading("Cargando Tickets...");
                TicketsListView.ItemsSource = tickets;
                this.BindingContext = this;

                GetTickets();

                //UserDialogs.Instance.HideLoading();
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
                    Command = new Command(async (s) => await Navigation.PushAsync(new AppSettingsPageAdmin())),

                    Order = ToolbarItemOrder.Secondary

                };

                var addTicketTool = new ToolbarItem
                {

                    Text = "Agregar Ticket",
                    Command = new Command(execute: () => addTicketIdAsync()),

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
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void TimerFunction(object source, ElapsedEventArgs e)
        {
            GetTicketsAsign();
        }
        private void SetTimer()
        {
            refreshTicketsTimer = new Timer(AppSettings.RefreshTicketsTimeout * 1000);
            refreshTicketsTimer.Elapsed += TimerFunction;
            refreshTicketsTimer.AutoReset = true;
            refreshTicketsTimer.Enabled = true;
            GetTicketsAsign();
        }

        private void ClearTimer()
        {
            refreshTicketsTimer.Stop();
            refreshTicketsTimer.Dispose();
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
        private async Task addTicketIdAsync()
        {
            Console.WriteLine("ADD TICKET FROM ID");
            var promptConfig = new PromptConfig();
            promptConfig.InputType = InputType.Name;
            promptConfig.IsCancellable = true;
            promptConfig.Message = "INGRESE ID DE TICKET";
            var result = await UserDialogs.Instance.PromptAsync(promptConfig);
            if (result.Ok)
            {
                //string error = "No se agrego el ticket, su numero de cuenta no coincide con el numero de cuenta enlazado al ticket";
                if (result.Text == "")
                {
                    //error = "Ingrese un id";
                    UserDialogs.Instance.ShowError("Ingrese un id.");
                }
                else
                {
                    UserDialogs.Instance.ShowLoading("Por favor espere");
                    User current = await App.Database.GetCurrentUser();
                    string html = await server.getDetailsTicket(result.Text);
                    string date = await server.getInitDate(result.Text);
                    UserDialogs.Instance.HideLoading();
                    if (html == "Error")
                    {
                        //error = "No existe un ticket con ese numero de ID: " + result.Text;
                        UserDialogs.Instance.ShowError("No existe un ticket con ese number de ID: " + result.Text);
                    }
                    else
                    {
                        string account = getDetailTicket(html, "Numero de cuenta / No. de talento humano:");
                        if (account == current.Account)
                        {
                            string c = getDetailTicket(html, "Clasificacion:");
                            int clas = 5;
                            if (c == "Solicitud")
                            {
                                clas = 1;
                            }
                            else if (c == "Información")
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
                            if (prioridad == "Alto")
                            {
                                p = 1;
                            }
                            else if (prioridad == "Medio")
                            {
                                p = 2;
                            }
                            try
                            {
                                await App.Database.CreateNewTicket(new Ticket()
                                {
                                    ID = result.Text,
                                    UserID = current.ID,
                                    Affected = int.Parse(getDetailTicket(html, "Cantidad de usuarios afectados:")),
                                    Classification = clas,
                                    Priority = p,
                                    Subject = getDetailTicket(html, "Tema"),
                                    Message = getDetailTicket(html, "<b>Mensaje:</b>"),
                                    Date = date,
                                });
                                //error = "El ticket se agrego exitosamente";
                                UserDialogs.Instance.ShowSuccess("Ticket Agregado!");

                                GetTickets();
                            }
                            catch (SQLiteException)
                            {
                                //error = "No se agrergo el ticket, porque ya existe en la aplicacion";
                                UserDialogs.Instance.ShowError("No se agrego el ticket, porque ya existe en la base de datos.");
                            }
                        }
                        else
                        {
                            UserDialogs.Instance.ShowError("No se agrego el ticket, su numero de cuenta no coincide con el numero de cuenta enlazado al ticket");
                        }


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


        //TERMINAN FUNCIONES


        protected override async void OnAppearing()
        {
            GetTicketsAsign();
            base.OnAppearing();
            Device.StartTimer(new TimeSpan(0, 0, AppSettings.RefreshTicketsTimeout), () =>
            {
                //Get tickets every 1 minute.
                GetTickets();
                return true;
            });
        }
        async void goToViewTicketAdmin(object sender, SelectedItemChangedEventArgs e)
        {

        }
        /*protected override async void OnAppearingS()
        {
            //Llamado a GetTickets reemplaza a SetTimer() debido a que el request de sesión en GetTickets crea exception al repetirse
            GetTicketsAsign();
            //SetTimer();
        }*/
        private async void TicketsListView_RefreshingAdminAssign(object sender, EventArgs e)
        {
            GetTicketsAsign();
            // TicketsListViewAdmin.ItemsSource = null;
            // TicketsListViewAdmin.ItemsSource = tickets.Where(t => t.Date != "error").OrderByDescending(t => DateTime.ParseExact(t.Date, "yyyy-MM-dd HH:mm:ss", 
            //             System.Globalization.CultureInfo.InvariantCulture));
            TicketsListViewAdminAssign.EndRefresh();
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
        public async void GetTicketsAsign()
        {

            User user = await App.Database.GetCurrentUser();

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

            int hcount = 0;

            if (table != null)
            {
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
            }

            TicketsListViewAdminAssign.ItemsSource = tickets;
        }
        private async void SearchBar_TextChangedAdminAssign(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.NewTextValue))
            {
                var showTickets = tickets_assign.Where(t => t.Subject.Contains(e.NewTextValue)).ToList();
                TicketsListViewAdminAssign.ItemsSource = showTickets;
            }
            else
            {
                TicketsListViewAdminAssign.ItemsSource = tickets_assign;
            }
        }
        public async void GetTickets()
        {
            try
            {


                List<Ticket> dbtickets;
                dbtickets = await App.Database.GetTicketsAsync();


                for (int i = 0; i < dbtickets.Count; i++)
                {
                    String updateDate = await server.getUpdateDate(dbtickets[i].ID);
                    if (!updateDate.Equals(dbtickets[i].Date) && updateDate != "error")
                    {
                        dbtickets[i].OpenImage = "bell.png";
                        dbtickets[i].Date = updateDate;
                        await App.Database.UpdateTicket(dbtickets[i]);
                    }
                    else
                        dbtickets[i].Image = "";


                    bool open = await server.getOpenTicket(dbtickets[i].ID);
                    Console.WriteLine("Recibiendo del sevidor: " + open.ToString());
                    if (!open)
                    {
                        dbtickets[i].OpenImage = "lock.png";
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
                TicketsListView.ItemsSource = tickets;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
