using System;
using System.Collections.Generic;
using System.Text;

namespace tickets.Models
{
    class MessageTicket
    {
        private string from;
        private DateTime date;
        private string text;

        public string Text
        {
            get { return text; }
            set { text = value; }
        }


        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }


        public string FROM
        {
            get { return from; }
            set { from = value; }
        }



    }
}
