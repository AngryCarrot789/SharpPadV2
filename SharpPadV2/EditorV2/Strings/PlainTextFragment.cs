using System.Windows.Media;

namespace SharpPadV2.EditorV2.Strings {
    public abstract class PlainTextFragment : ITextFragment {
        public string Text { get; }

        public PlainTextFragment(string text) {
            this.Text = text;
        }

        public void Render(DrawingContext dc) {
            // dc.DrawText(new FormattedText(this.Text,CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface()));
        }
    }
}