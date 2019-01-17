using Android.Graphics.Drawables;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(tickets.TicketsLabel), typeof(tickets.Droid.CustomElementsRenderer.TicketsLabelAndroidRenderer))]


namespace tickets.Droid.CustomElementsRenderer
{
    public class TicketsLabelAndroidRenderer : LabelRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null)
            {
                Control.SetTextSize(Android.Util.ComplexUnitType.Pt, 7);
                Control.SetTextColor(Android.Graphics.Color.Argb(255, 0, 0, 0));
            }
        }
    }
}