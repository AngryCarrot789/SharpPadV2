using System.Windows;
using System.Windows.Documents;

namespace SharpPadV2.TextEditor {
    public class CaretRenderAdorner : Adorner {
        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.Register(
                "Index",
                typeof(int),
                typeof(Caret),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty SelectionLengthProperty =
            DependencyProperty.Register(
                "SelectionLength",
                typeof(int),
                typeof(Caret),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

        public int Index {
            get => (int) this.GetValue(IndexProperty);
            set => this.SetValue(IndexProperty, value);
        }

        public int SelectionLength {
            get => (int) this.GetValue(SelectionLengthProperty);
            set => this.SetValue(SelectionLengthProperty, value);
        }

        public CaretRenderAdorner(UIElement adornedElement) : base(adornedElement) {

        }
    }
}