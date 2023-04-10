using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace SharpPadV2.TextEditor {
    public class LineBorderOutlineAdorder : Adorner {
        public static readonly DependencyProperty OutlineBackgroundProperty = DependencyProperty.Register("OutlineBackground", typeof(Brush), typeof(LineBorderOutlineAdorder), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty OutlineBorderBrushProperty = DependencyProperty.Register("OutlineBorderBrush", typeof(Brush), typeof(LineBorderOutlineAdorder), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, (a, b) => ((LineBorderOutlineAdorder) a).InvalidatePen()));
        public static readonly DependencyProperty OutlineBorderThicknessProperty = DependencyProperty.Register("OutlineBorderThickness", typeof(double), typeof(LineBorderOutlineAdorder), new FrameworkPropertyMetadata(2d, FrameworkPropertyMetadataOptions.AffectsRender, (a, b) => ((LineBorderOutlineAdorder) a).InvalidatePen()));
        public static readonly DependencyProperty RectangleProperty = DependencyProperty.Register("Rectangle", typeof(Rect), typeof(LineBorderOutlineAdorder), new FrameworkPropertyMetadata(Rect.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

        [Category("Brush")]
        public Brush OutlineBackground {
            get => (Brush) this.GetValue(OutlineBackgroundProperty);
            set => this.SetValue(OutlineBackgroundProperty, value);
        }

        [Category("Brush")]
        public Brush OutlineBorderBrush {
            get => (Brush) this.GetValue(OutlineBorderBrushProperty);
            set => this.SetValue(OutlineBorderBrushProperty, value);
        }

        [Category("Appearance")]
        public double OutlineBorderThickness {
            get => (double) this.GetValue(OutlineBorderThicknessProperty);
            set => this.SetValue(OutlineBorderThicknessProperty, value);
        }

        public Rect Rectangle {
            get => (Rect) this.GetValue(RectangleProperty);
            set => this.SetValue(RectangleProperty, value);
        }

        private Pen pen;
        private bool hasCheckedBorderPen;

        public LineBorderOutlineAdorder(UIElement editor) : base(editor) {
            this.IsHitTestVisible = false;
        }

        private void InvalidatePen() {
            this.pen = null;
            this.hasCheckedBorderPen = false;
        }

        protected override void OnRender(DrawingContext dc) {
            TextBoxBase editor = (TextBoxBase) this.AdornedElement;
            Rect rect = this.Rectangle;
            if (rect.IsEmpty || !editor.IsEnabled || !this.IsEnabled) {
                return;
            }

            if (this.pen == null && !this.hasCheckedBorderPen) {
                Brush brush = this.OutlineBorderBrush;
                double thickness = this.OutlineBorderThickness;
                if (brush != null && thickness > 0d) {
                    this.pen = new Pen(brush, thickness);
                    if (brush.IsFrozen) {
                        this.pen.Freeze();
                    }
                }

                this.hasCheckedBorderPen = true;
            }

            // need to add 4 for some reason
            dc.PushClip(new RectangleGeometry(new Rect(0, 0, editor.ViewportWidth + 4, editor.ViewportHeight)));
            if (this.pen != null) {
                double thickA = this.pen.Thickness * 0.5;
                double thickB = this.pen.Thickness;
                // Rect rectangle = new Rect(new Point(thick, thick), new Point(this.RenderSize.Width - thick, this.RenderSize.Height - thick));
                if (!rect.IsEmpty) {
                    rect.X += thickA;
                    rect.Y += thickA;
                    rect.Width -= thickB;
                    rect.Height -= thickB;
                    dc.DrawRectangle(null, this.pen, rect);
                }
            }

            dc.DrawRectangle(this.OutlineBackground, null, rect);
            dc.Pop();
        }
    }
}