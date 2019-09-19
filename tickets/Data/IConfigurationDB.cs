using SQLite.Net.Interop;
using System;
using System.Collections.Generic;
using System.Text;

namespace tickets.Data
{
    public interface IConfigurationDB
    {
        string directorio { get; }
        ISQLitePlatform plataforma { get; }
    }
}