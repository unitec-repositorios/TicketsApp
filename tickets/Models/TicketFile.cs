using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace tickets.Models
{
    class TicketFile
    {
        public string TicketID { get; set; }
        public string Filename { get; set; }
        public byte[] FileData { get; set; }


        
    }
}
