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
            this.BindingContext = this;

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
            TicketsListView.BeginRefresh();
        }

        async void goToViewTicket(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                Debug.WriteLine("Opening messages for ticket with id = " + ((Ticket)e.SelectedItem).ID);
                ((Ticket)e.SelectedItem).Date = await server.getUpdateDate(((Ticket)e.SelectedItem).ID);
                ((Ticket)e.SelectedItem).Image = "";
                App.Database.UpdateTicket(((Ticket)e.SelectedItem));
                Navigation.PushAsync(new chatTicket()
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
            List<Ticket> tickets = await App.Database.GetTicketsAsync(App.Database.GetCurrentUserNotAsync());

            if (!String.IsNullOrWhiteSpace(e.NewTextValue))
            {
                tickets = tickets.Where(t => t.Subject.Contains(e.NewTextValue)).ToList();
                TicketsListView.ItemsSource = tickets;
            } else
            {
                TicketsListView.ItemsSource = tickets;
            }
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
                    App.Database.UpdateTicket(tickets[i]);
                }
            }

            if (String.IsNullOrWhiteSpace(searchText))
                return tickets;
            return tickets.Where(c => c.Subject.StartsWith(searchText)).ToList();
        } 
    }
}
