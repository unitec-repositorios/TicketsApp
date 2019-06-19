using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace tickets.Data
{
   public interface IConnection
    {
        //GET METHOD
        Ticket getTicket(string id);
        ObservableCollection<Ticket> getTickets();

        //POST METOD
        void postTicket(Ticket ticket);

        //PUT METHOD
        void putTicket(Ticket ticket);

    }
}
