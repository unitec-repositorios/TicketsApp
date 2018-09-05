using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using tickets.API;
using tickets.Models;

namespace tickets.screens
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class chatTicket : ContentPage
	{
        private Server server = new Server();
        private string ticketID;
        private string messageRef = "<p><b>Mensaje:</b></p>";
        private string autorRef = "<td class=\"tickettd\">";

        private chatViewModel chatVM;
        public chatTicket ()
		{
			InitializeComponent ();
            BindingContext = chatVM = new chatViewModel();
            //set the ticketID
            //ticketID = "g86vpnm3tm";
            readTicket();
            chatVM.ListMessages.CollectionChanged += (sender, e) =>
            {
                var target = chatVM.ListMessages[chatVM.ListMessages.Count - 1];
                MessagesListView.ScrollTo(target, ScrollToPosition.End, true);
            };

        }
        public async void readTicket()
        {
            string html = await server.getTicket(ticketID);
            string autor = "";
            string message = "";
            string myName = null;
            int position = html.IndexOf(autorRef + "N");
            int index = position + autorRef.Count();
            while (position!=-1)
            {
                html = html.Substring(index);
                autor = getAutor(html);
                if (myName == null)
                {
                    myName = autor;
                }
                position = html.IndexOf(messageRef);
                index = position + messageRef.Count();
                if (position != -1)
                {
                    html = html.Substring(index);
                    message = getMessage(html);
                    bool typeText = true;
                    //add new message to the chat
                    if (autor.Equals(myName))
                    {
                        autor = "";
                        typeText = false;
                    }
                    else {
                        autor += ":\n";
                    }
                    var mymessage = new Message
                    {
                        Text = autor+message,
                        IsTextIn = typeText,
                        //need to correct the time message
                        MessageDateTime = DateTime.Now
                    };
                    chatVM.ListMessages.Add(mymessage);
                }
                position = html.IndexOf(autorRef + "N");
                index = position + autorRef.Count();
            }
        }
        public string getAutor(string html)
        {
            string autor = "";
            string supportString = "";
            int index = 0;
            while (index != -1)
            {
                if (supportString.Contains(autorRef))
                {
                    if (html[index] != '<')
                    {
                        autor += html[index];
                    }
                    else
                    {
                        index = -2;
                    }
                }
                else
                {
                    supportString += html[index];
                }
                index++;
            }
            return autor;
        }
        public string getMessage(string html)
        {
            string Mimessage = "";
            string supportString = "";
            int index = html.IndexOf("<p>") + 3;
            int endMessage = html.IndexOf("</p>");
            int endMessage2 = html.IndexOf("&nbsp;</p>");
            bool tag = false;
            if (endMessage2 < 0)
            {
                endMessage2 = endMessage;
            }
            while (index < endMessage && index < endMessage2)
            {
                if (html[index] == '<')
                {
                    supportString += html[index];
                    tag = true;
                }
                else if (html[index] == '>')
                {
                    supportString += html[index];
                    if (supportString.Equals("<br />"))
                    {
                        Mimessage += "\n";
                    }
                    supportString = "";
                    tag = false;
                }
                else if (tag)
                {
                    supportString += html[index];
                }
                else
                {
                    Mimessage += html[index];
                }
                index++;
            }
            return Mimessage;
        }
    }
}