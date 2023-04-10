using System.Windows.Media;

namespace SharpPadV2.EditorV2.Strings {
    public interface ITextFragment {
        void Render(DrawingContext dc);
    }
}