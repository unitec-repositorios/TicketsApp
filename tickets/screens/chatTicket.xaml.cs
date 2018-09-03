﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using tickets.API;


namespace tickets.screens
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class chatTicket : ContentPage
	{
        private Server server = new Server();
        private string ticketID;
        private string messageRef = "<p><b>Mensaje:</b></p>";
        private string autorRef = "<td class=\"tickettd\">";
        public chatTicket ()
		{
			InitializeComponent ();
		}
        public async void readTicket(object sender, System.EventArgs e)
        {
            string html = await server.getTicket(ticketID);
            string autor = "";
            string message = "";
            int position = html.IndexOf(autorRef + "N");
            int index = position + autorRef.Count();
            while (position!=-1)
            {
                html = html.Substring(index);
                autor = getAutor(html);
                position = html.IndexOf(messageRef);
                index = position + messageRef.Count();
                if (position != -1)
                {
                    html = html.Substring(index);
                    message = getMessage(html);
                    //add new message to the chat
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
            string message = "";
            string supportString = "";
            int index = html.IndexOf("<p>")+3;
            int endMessage = html.IndexOf("&nbsp;</p>");
            while (index < endMessage)
            {
                if (html[index] == '<')
                {
                    supportString += html[index];
                }
                else if (supportString.Contains("/>"))
                {
                    if (supportString.Equals("<br />"))
                    {
                        message += "\n";
                    }
                    supportString = "";
                }
                else
                {
                    message += html[index];
                }
                index++;
            }
            return message;
        }
    }
}