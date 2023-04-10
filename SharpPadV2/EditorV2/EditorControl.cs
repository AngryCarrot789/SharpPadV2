using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SharpPadV2.EditorV2 {
    public class EditorControl : FrameworkElement {
        public EditorControl() {
            this.Focusable = true;
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (e.Handled) {
                return;
            }

            int codePoint = (int) e.Key;
            if (codePoint >= 44 && codePoint <= 69) {
                if (IsModifierPressed(ModifierKeys.Shift)) {

                }

                if (ToChar(e.Key, out char ch)) {
                    this.OnCharTyped(ch);
                }
            }
        }

        public static bool ToChar(Key key, out char ch) {
            if (key >= Key.D0 && key <= Key.D9) {
                ch = (char) (key - 34 + 48);
            }
            else if (key >= Key.A && key <= Key.Z) {
                ch = (char) (key - 44 + 65);
            }
            else {
                ch = '\0';
                return false;
            }

            return true;
        }

        public void OnCharTyped(char ch) {

        }

        public static bool IsModifierPressed(ModifierKeys keys) {
            return (Keyboard.Modifiers & keys) == keys;
        }

        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);
        }
    }
}