using System;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;
using MvvmHelpers;
using tickets.API;
using System.ComponentModel;
using Acr.UserDialogs;

namespace tickets.Models
{
    public class chatViewModel : BaseViewModel
    {
        public ObservableRangeCollection<Message> ListMessages { get; }
        public ICommand SendCommand { get; set; }
        private Server server = new Server();
        private string ticketID;
        public string state { get; set; }
        private chatTicket chatfile;
        public List<(string, byte[])> Files = new List<(string, byte[])>();


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

                    
                    
                    sendMessage(message);


                    //ListMessages.Add(message);
                    //OutText = "";
                }
                else
                {
                    UserDialogs.Instance.Alert("Ingresar el mensaje","Chat Ticket");
                }

            });
            
        }



        public async void sendMessage(Message message)
        {
            UserDialogs.Instance.ShowLoading("Enviando Mensaje...");
            string status = await server.replyTicket(message.Text, message.Files, this.ticketID);
            Console.WriteLine(status);
            if (status.Equals("ok"))
            {
                UserDialogs.Instance.ShowSuccess("Mensaje Enviado!");
                ListMessages.Add(message);
                OutText = "";

            }
            else
            {
                UserDialogs.Instance.ShowError("Mensaje no fue enviado...");
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

