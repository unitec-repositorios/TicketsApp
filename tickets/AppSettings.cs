﻿using System;
using System.Text;

namespace tickets
{
    public static class AppSettings
    {
        //Servidor Oficial
        public const string BASE_ADDRESS = "https://cap.unitec.edu";


        //Servidor de Prueba
        //public const string BASE_ADDRESS = "http://157.230.130.35";


        public static int ImagesQuality = 50;
        public static int RefreshTicketsTimeout = 60;
        public const string Encoding = "windows-1252";
    }
}
