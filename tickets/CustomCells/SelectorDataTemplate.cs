using tickets.CustomCells;
using tickets.Models;
using Xamarin.Forms;

namespace tickets
{
    public class SelectorDataTemplate : DataTemplateSelector
    {
        private readonly DataTemplate textInDataTemplate;
        private readonly DataTemplate textOutDataTemplate;

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var messageVm = item as Message;
            if (messageVm == null)
                return null;
            else if (messageVm.EsPropio)
            {
                return textInDataTemplate;
            }
            else
            {
                return textOutDataTemplate;
            }
        }


        public SelectorDataTemplate()
        {
            textInDataTemplate = new DataTemplate(typeof(TextInViewCell));
            textOutDataTemplate = new DataTemplate(typeof(TextOutViewCell));
        }

    }
}
