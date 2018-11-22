using System;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;
using MvvmHelpers;
using tickets.API;

namespace tickets.Models
{
    public class chatViewModel : BaseViewModel
    {
        public ObservableRangeCollection<Message> ListMessages { get; }
        public ICommand SendCommand { get; set; }
        private Server server = new Server();
        private string ticketID;
        private chatTicket chatfile;
        public List<(string, byte[])> Files = new List<(string, byte[])>();

        private ContentView loading = new ContentView
        {
            IsVisible = false ,
            BackgroundColor = Color.Gray,
            Padding = 10
            
        };

        private ActivityIndicator activityIndicator = new ActivityIndicator
        {
            WidthRequest = 110, 
            HeightRequest = 70, 
            IsRunning = true,
            IsVisible = true,
            Color = Color.Red,
            HorizontalOptions = LayoutOptions.CenterAndExpand ,
            VerticalOptions = LayoutOptions.CenterAndExpand
        };

        public chatViewModel(string ticket, List<(string, byte[])> files)
        {
            this.ticketID = ticket;
            this.Files = files;
            ListMessages = new ObservableRangeCollection<Message>();

            //chatfile = new chatTicket();

            SendCommand = new Command(() =>
            {
                if (!String.IsNullOrWhiteSpace(OutText))
                {
                    var message = new Message
                    {
                        Text = OutText,
                        Files = Files,
                        IsTextIn = false,
                        MessageDateTime = DateTime.Now
                    };

                    activityIndicator.IsVisible = true;
                    sendMessage(message);
                    //activityIndicator.IsVisible = false;
                    //ListMessages.Add(message);
                    //OutText = "";
                }

            });
            
        }
        public async void sendMessage(Message message)
        {
            string status = await server.replyTicket(message.Text,message.Files, this.ticketID);
            if(status.Equals("ok"))
            {
                ListMessages.Add(message);
                OutText = "";
            }
            else
            {
                //OutText = this.ticketID;
            }
        }



        public string OutText
        {
            get { return _outText; }
            set { SetProperty(ref _outText, value); }
        }
        string _outText = string.Empty;
    }

   
}

