 
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
    public partial class MyTicketsAdmin : TabbedPage
    {
        private Server server = new Server();
        ObservableCollection<Ticket> tickets = new ObservableCollection<Ticket>();
        
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
        }

        private void SetTimer()
        {

        }

        private void ClearTimer()
        {
        }

        protected override async void OnAppearing()
        {
        }

        protected override async void OnDisappearing()
        {
        }
        //Tickets Enviados
        async void goToViewTicketAdmin(object sender, SelectedItemChangedEventArgs e)
        {
        }

        private async void TicketsListView_RefreshingAdmin(object sender, EventArgs e)
        {

        }

        private async void SearchBar_TextChangedAdmin(object sender, TextChangedEventArgs e)
        {
        }
        //Tickets Asignados
        async void goToViewTicketAdminAsign(object sender, SelectedItemChangedEventArgs e)
        {
        }

        private async void TicketsListView_RefreshingAdminAsign(object sender, EventArgs e)
        {

        }

        private async void SearchBar_TextChangedAdminAsign(object sender, TextChangedEventArgs e)
        {
        }
        public async void GetTickets()
        {
        }
    }
}
