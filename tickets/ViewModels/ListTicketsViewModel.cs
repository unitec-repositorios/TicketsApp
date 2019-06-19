using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tickets.ViewModels
{
    public class ListTicketsViewModel:Ticket
    {
        private ObservableCollection<Ticket>listTickets;
       
        public ObservableCollection<Ticket> ListTickets
        {
            get {
                if (listTickets==null)
                {
                    GetTickets();
                }
      
                return listTickets;
            }

            set => listTickets = value; }

       

        private void GetTickets()

        {
            var temp_list = App.Database.GetTickets().Result;
            
            listTickets = new ObservableCollection<Ticket>(temp_list.OrderByDescending(t => t.CreationDate).OrderByDescending(t => t.Open));
            
        }
    }

    
}
