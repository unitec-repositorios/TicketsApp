//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: global::Xamarin.Forms.Xaml.XamlResourceIdAttribute("tickets.screens.AppSettingsPageAdmin.xaml", "screens/AppSettingsPageAdmin.xaml", typeof(global::tickets.AppSettingsPageAdmin))]

namespace tickets {
    
    
    [global::Xamarin.Forms.Xaml.XamlFilePathAttribute("screens\\AppSettingsPageAdmin.xaml")]
    public partial class AppSettingsPageAdmin : global::Xamarin.Forms.ContentPage {
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private global::Xamarin.Forms.Slider pictureQualitySetting;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private global::Xamarin.Forms.EntryCell ticketsTimeoutSetting;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private void InitializeComponent() {
            global::Xamarin.Forms.Xaml.Extensions.LoadFromXaml(this, typeof(AppSettingsPageAdmin));
            pictureQualitySetting = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Xamarin.Forms.Slider>(this, "pictureQualitySetting");
            ticketsTimeoutSetting = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Xamarin.Forms.EntryCell>(this, "ticketsTimeoutSetting");
        }
    }
}
