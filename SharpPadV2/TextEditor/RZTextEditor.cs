using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace SharpPadV2.TextEditor {
    public class RZTextEditor : RichTextBox {
        public LineBorderOutlineAdorder OutlineBorder { get; }

        public static readonly DependencyProperty OutlineBackgroundProperty =
            LineBorderOutlineAdorder.OutlineBackgroundProperty.AddOwner(
                typeof(RZTextEditor),
                new FrameworkPropertyMetadata(null, (d, e) => ((RZTextEditor) d).OutlineBorder.OutlineBackground = (Brush) e.NewValue));

        public static readonly DependencyProperty OutlineBorderBrushProperty =
            LineBorderOutlineAdorder.OutlineBorderBrushProperty.AddOwner(
                typeof(RZTextEditor),
                new FrameworkPropertyMetadata(null, (d, e) => ((RZTextEditor) d).OutlineBorder.OutlineBorderBrush = (Brush) e.NewValue));

        public static readonly DependencyProperty OutlineBorderThicknessProperty =
            LineBorderOutlineAdorder.OutlineBorderThicknessProperty.AddOwner(
                typeof(RZTextEditor),
                new FrameworkPropertyMetadata(2d, (d, e) => ((RZTextEditor) d).OutlineBorder.OutlineBorderThickness = (double) e.NewValue));

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

        public RZTextEditor() {
            this.AutoWordSelection = false;
            this.IsInactiveSelectionHighlightEnabled = true;
            this.AcceptsTab = true;
            this.AcceptsReturn = true;
            this.SpellCheck.SpellingReform = SpellingReform.Postreform;
            this.Language = XmlLanguage.GetLanguage("en");
            this.UndoLimit = 1000;
            this.OutlineBorder = new LineBorderOutlineAdorder(this);
            this.Loaded += this.OnLoaded;
            this.TextChanged += (sender, args) => this.DrawRectangleAtCaret();
            this.SelectionChanged += (sender, args) => this.DrawRectangleAtCaret();
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
            if (layer != null) {
                layer.Add(this.OutlineBorder);
            }

            this.DrawRectangleAtCaret();
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e) {
            if (!e.Handled && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
                if (e.Delta < 0) {
                    this.LineRight();
                    this.LineRight();
                    this.LineRight();
                    e.Handled = true;
                }
                else if (e.Delta > 0) {
                    this.LineLeft();
                    this.LineLeft();
                    this.LineLeft();
                    e.Handled = true;
                }
            }

            base.OnPreviewMouseWheel(e);
        }

        public void DrawRectangleAtCaret() {
            double width = this.ExtentWidth;
            if (width < 1) {
                return;
            }

            TextPointer caret = this.CaretPosition;
            TextPointer lineBegin = caret.GetLineStartPosition(0) ?? caret.DocumentStart;
            TextPointer nextLine = caret.GetLineStartPosition(1);
            TextPointer lineEnd = nextLine != null ? (nextLine.GetNextContextPosition(LogicalDirection.Backward) ?? caret.DocumentEnd) : caret.DocumentEnd;
            Rect a = lineBegin.GetCharacterRect(LogicalDirection.Forward);
            Rect b = lineEnd.GetCharacterRect(LogicalDirection.Backward);
            Rect line = new Rect(a.TopLeft, b.BottomRight);
            line.X = 0;
            line.Width = width;
            line.Height += this.Document.LineHeight;
            this.OutlineBorder.Rectangle = line;
        }
    }
}