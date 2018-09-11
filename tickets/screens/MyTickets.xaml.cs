using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using tickets.API;
using Xamarin.Forms;

namespace tickets
{
    public partial class MyTickets : ContentPage
    {
        private Server server = new Server();

        public MyTickets()
        {
            InitializeComponent();

            var newTicket = new ToolbarItem
            {
                Text = "Nuevo",
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
            }

            //App.Database.CreateNewTicket(new Ticket()
            //{
            //    ID = "qwerty",
            //    UserID = App.Database.GetCurrentUserNotAsync().ID,
            //    Affected = 1,
            //    Classification = 1,
            //    Priority = 1,
            //    Subject = "Problemas con el correo",
            //    Message = "He tenido ciertos problemas cuando ...",
            //});
            //App.Database.CreateNewTicket(new Ticket()
            //{
            //    ID = "qwerty2",
            //    UserID = App.Database.GetCurrentUserNotAsync().ID,
            //    Affected = 1,
            //    Classification = 1,
            //    Priority = 1,
            //    Subject = "Problemas con el correo",
            //    Message = "He tenido ciertos problemas cuando ...",
            //});
            //App.Database.CreateNewTicket(new Ticket()
            //{
            //    ID = "qwerty3",
            //    UserID = App.Database.GetCurrentUserNotAsync().ID,
            //    Affected = 1,
            //    Classification = 1,
            //    Priority = 1,
            //    Subject = "Problemas con el correo",
            //    Message = "He tenido ciertos problemas cuando ...",
            //});
        }

        protected override async void OnAppearing()
        {
            TicketsListView.ItemsSource = await GetTickets();
        }

        async void goToViewTicket(object sender, SelectedItemChangedEventArgs e)
        {
            
            if (e.SelectedItem != null)
            {
                Debug.WriteLine("Opening messages for ticket with id = " + ((Ticket)e.SelectedItem).ID);
                ((Ticket)e.SelectedItem).Date = await server.getUpdateDate(((Ticket)e.SelectedItem).ID);
                ((Ticket)e.SelectedItem).Image = "";
                await App.Database.UpdateTicket(((Ticket)e.SelectedItem));
                await Navigation.PushAsync(new chatTicket()
                {
                    BindingContext = ((Ticket)e.SelectedItem).ID
                });
            }
        }

        private async void TicketsListView_Refreshing(object sender, EventArgs e)
        {
            
            TicketsListView.ItemsSource = await GetTickets();
            TicketsListView.EndRefresh();
        }

        private async void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            TicketsListView.ItemsSource = await GetTickets(e.NewTextValue);
        }

        async Task<List<Ticket>> GetTickets(string searchText = null)
        {
            List<Ticket> tickets = await App.Database.GetTicketsAsync(App.Database.GetCurrentUserNotAsync());
            for (int i = 0; i < tickets.Count; i++)
            {
                String updateDate = await server.getUpdateDate(tickets[i].ID);
                if (!updateDate.Equals(tickets[i].Date))
                {
                    tickets[i].Image = "https://cdn.pixabay.com/photo/2015/12/16/17/41/bell-1096280_640.png";
                }
            }

            if (String.IsNullOrWhiteSpace(searchText))
                return tickets;
            return tickets.Where(c => c.Subject.StartsWith(searchText)).ToList();
        }
    }
}
