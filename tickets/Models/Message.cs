﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmHelpers;

namespace tickets.Models
{
    public class Message
    {
        private string autor;
        private DateTime date;
        private string text;
        private bool esPropio;

        public bool EsPropio
        {
            get { return esPropio; }
            set { esPropio = value; }
        }


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

        public string Autor
        {
            get { return autor; }
            set { autor = value; }
        }


    }
}
