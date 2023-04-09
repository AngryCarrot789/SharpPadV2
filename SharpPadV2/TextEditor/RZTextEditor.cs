using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using SharpPadV2.Core;
using SharpPadV2.Utils;

namespace SharpPadV2.TextEditor {
    public class RZTextEditor : TextBox {
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

        public int SafeCaretIndex {
            get {
                int caret = this.CaretIndex;
                return caret >= this.Text.Length ? (caret - 1) : caret;
            }
        }

        private ScrollViewer PART_ContentHost;

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

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);
            this.DrawRectangleAtCaret();
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.PART_ContentHost = this.GetTemplateChild("PART_ContentHost") as ScrollViewer;
            if (this.PART_ContentHost != null) {
                this.PART_ContentHost.ScrollChanged += (s,e) => this.DrawRectangleAtCaret();
            }
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
            double width = this.ViewportWidth;
            if (width > 0) {
                double y;
                double height = TextBlock.GetLineHeight(this);
                if (height <= 0) {
                    Rect rect = this.GetRectFromCharacterIndex(this.CaretIndex);
                    y = rect.Y;
                    height = rect.Height;
                }
                else {
                    int lineIndex = this.GetLineIndexFromCharacterIndex(this.CaretIndex);
                    y = lineIndex * height;
                }

                y -= this.VerticalOffset;

                // For some reason you have to add 4 for it to stretch the entire width... there's a 4px offset weirdly
                this.OutlineBorder.Rectangle = new Rect(0, y, width + 4, height);
            }
        }

        // public void DrawRectangleAtCaret() {
        //     double width = this.ExtentWidth;
        //     if (width < 1) {
        //         return;
        //     }
        //     TextPointer lineBegin = TextPointerUtils.GetLineBegin(this.CaretPosition);
        //     Rect a = lineBegin.GetCharacterRect(LogicalDirection.Forward);
        //     double offsetY = this.Document.LineHeight * 0.5d;
        //     this.OutlineBorder.Rectangle = new Rect(0, a.Y - offsetY, width, a.Height + offsetY);
        // }

        public void CutLineOrSelection() {
            if (this.SelectionLength > 0) {
                this.Cut();
            }
            else {
                this.SelectEntireCurrentLine();
                this.Cut();
            }
        }

        public void CopyLineOrSelection() {
            if (this.SelectionLength > 0) {
                this.Copy();
            }
            else {
                this.SelectEntireCurrentLine();
                string newLineText = this.SelectedText;
                this.CaretIndex = this.Text.LastIndexOf('\n', this.SafeCaretIndex) + 1;
                IoC.Clipboard.ReadableText = newLineText;
            }
        }

        public void SelectEntireCurrentLine() {
            // search backwards from caret to start of string
            int caret = this.CaretIndex;
            if (caret >= this.Text.Length) {
                caret--;
            }

            int startLineIndex = this.Text.LastIndexOf('\n', caret) + 1;
            int endLineIndex = this.Text.IndexOf('\n', caret + 1);
            if (endLineIndex == -1) {
                endLineIndex = this.Text.Length;
            }

            if (endLineIndex <= startLineIndex) {
                this.Select(endLineIndex, 0);
                return;
            }

            this.Select(startLineIndex, endLineIndex - startLineIndex);
        }
    }
}