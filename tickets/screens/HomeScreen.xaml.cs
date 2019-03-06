using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace tickets
{
    public partial class HomeScreen : TabbedPage
    {
        public HomeScreen()
        {
            InitializeComponent();
            CurrentPageChanged += CurrentPageHasChanged;
        }

        protected void CurrentPageHasChanged(object sender, EventArgs e) 
        { 
            Title = CurrentPage.Title; 
        }
    }
}
