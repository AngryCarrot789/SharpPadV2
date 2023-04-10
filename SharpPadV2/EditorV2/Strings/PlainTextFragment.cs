using System.Windows.Media;

namespace SharpPadV2.EditorV2.Strings {
    public abstract class TextFragment : ITextFragment {
        public string Text { get; }

        public TextFragment(string text) {
            this.Text = text;
        }

        public void Render(DrawingContext dc) {
            dc.DrawText();
        }
    }
}