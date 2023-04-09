using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace SharpPadV2.TextEditor {
    public partial class RZTextEditor {
        public class LineBorderOutlineAdorder : Adorner {
            public static readonly DependencyProperty OutlineBackgroundProperty =       DependencyProperty.Register("OutlineBackground",      typeof(Brush),  typeof(LineBorderOutlineAdorder), new FrameworkPropertyMetadata(null,       FrameworkPropertyMetadataOptions.AffectsRender));
            public static readonly DependencyProperty OutlineBorderBrushProperty =      DependencyProperty.Register("OutlineBorderBrush",     typeof(Brush),  typeof(LineBorderOutlineAdorder), new FrameworkPropertyMetadata(null,       FrameworkPropertyMetadataOptions.AffectsRender));
            public static readonly DependencyProperty OutlineBorderThicknessProperty =  DependencyProperty.Register("OutlineBorderThickness", typeof(double), typeof(LineBorderOutlineAdorder), new FrameworkPropertyMetadata(2d,         FrameworkPropertyMetadataOptions.AffectsRender));
            public static readonly DependencyProperty RectangleProperty =               DependencyProperty.Register("Rectangle",              typeof(Rect),   typeof(LineBorderOutlineAdorder), new FrameworkPropertyMetadata(Rect.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

            public Brush OutlineBackground {
                get => (Brush) this.GetValue(OutlineBackgroundProperty);
                set => this.SetValue(OutlineBackgroundProperty, value);
            }

            public Brush OutlineBorderBrush {
                get => (Brush) this.GetValue(OutlineBorderBrushProperty);
                set => this.SetValue(OutlineBorderBrushProperty, value);
            }

            public double OutlineBorderThickness {
                get => (double) this.GetValue(OutlineBorderThicknessProperty);
                set => this.SetValue(OutlineBorderThicknessProperty, value);
            }

            public Rect Rectangle {
                get => (Rect) this.GetValue(RectangleProperty);
                set => this.SetValue(RectangleProperty, value);
            }

            public LineBorderOutlineAdorder(UIElement editor) : base(editor) {
                this.OutlineBackground = new SolidColorBrush(Colors.SlateGray) {
                    Opacity = 0.5d
                };

                this.OutlineBorderBrush = new SolidColorBrush(Colors.DimGray) {
                    Opacity = 1d
                };

                this.IsHitTestVisible = false;
            }

            protected override void OnRender(DrawingContext dc) {
                dc.DrawRectangle(this.OutlineBackground, new Pen(this.OutlineBorderBrush, this.OutlineBorderThickness), this.Rectangle);
            }
        }
    }
}