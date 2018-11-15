using Android.Graphics.Drawables;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(tickets.TicketsRoundedStroke), typeof(tickets.Droid.CustomElementsRenderer.TicketsRoundedStrokeAndroidRenderer))]

namespace tickets.Droid.CustomElementsRenderer
{
    public class TicketsRoundedStrokeAndroidRenderer : LabelRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null)
            {
                var strokeDrawable = new GradientDrawable();
                strokeDrawable.SetCornerRadius(60f);
                strokeDrawable.SetStroke(3, Android.Graphics.Color.Argb(255,0,0,0));
                strokeDrawable.SetColor(Android.Graphics.Color.Argb(255, 224,224,224));
                Control.SetBackground(strokeDrawable);
                Control.SetTextSize(Android.Util.ComplexUnitType.Pt, 7);
            }
        }
    }
}