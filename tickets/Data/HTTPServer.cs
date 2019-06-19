using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Text;

namespace tickets.Data
{
    class HTTPServer : IConnection
    {
        private const string BASE_ADDRESS=AppSettings.BASE_ADDRESS;
        private HttpClient _client;
        public HTTPServer()
        {
            _client = new HttpClient();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }
        public Ticket getTicket(string id)
        {
            var uri = BASE_ADDRESS + "/print.php?track=" + id;
            var response = _client.GetByteArrayAsync(uri).Result;
            Encoding encoder = Encoding.GetEncoding(AppSettings.Encoding);
            string html = encoder.GetString(response, 0, response.Length - 1);
            var _id= html.IndexOf("ID de seguimiento: ").ToString();


            return new Ticket() {
                ID = _id,
                



            };
            throw new NotImplementedException();
        }

        public ObservableCollection<Ticket> getTickets()
        {
            throw new NotImplementedException();
        }

        public void postTicket(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public void putTicket(Ticket ticket)
        {
            throw new NotImplementedException();
        }
    }
}
