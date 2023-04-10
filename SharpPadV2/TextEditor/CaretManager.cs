using System.Collections.Generic;
using System.Linq;
using SharpPadV2.Core.Utils;

namespace SharpPadV2.TextEditor {
    public class CaretManager {
        public RZTextEditor Editor { get; }

        private readonly Dictionary<int, Caret> carets;

        public int CaretCount => this.carets.Count;

        public int DocumentLength => this.Editor.Text.Length;

        public CaretManager(RZTextEditor editor) {
            this.Editor = editor;
            this.carets = new Dictionary<int, Caret>();
        }

        public void MoveAllHorizontally(int offset) {
            int length = this.DocumentLength;
            foreach (Caret caret in this.carets.Values.ToList()) {
                int newIndex = Maths.Clamp(caret.Index + offset, 0, length);
                if (this.carets.ContainsKey(newIndex)) {
                    this.carets.Remove(caret.Index);
                }
                else {
                    caret.Index = newIndex;
                }
            }
        }

        public void AddOrRemoveNewCaret() {
            int index = this.Editor.CaretIndex;
            if (this.carets.TryGetValue(index, out Caret caret)) {
                this.carets.Remove(index);
                this.OnCaretRemoved(index, caret);
            }
            else {
                this.carets[index] = caret = new Caret(this);
                this.OnCaretCreated(index, caret);
            }
        }

        public Caret AddOrRemoveCaret(int index, out bool removed) {
            if (this.carets.TryGetValue(index, out Caret caret)) {
                this.carets.Remove(index);
                this.OnCaretRemoved(index, caret);
                removed = true;
            }
            else {
                this.carets[index] = caret = new Caret(this);
                this.OnCaretCreated(index, caret);
                removed = false;
            }

            return caret;
        }

        public void MoveCaretTo(Caret caret, int targetIndex, out Caret merged) {
            if (this.carets.TryGetValue(targetIndex, out Caret other)) {
                if (ReferenceEquals(caret, other)) {
                    merged = null;
                    caret.Index = targetIndex;
                }
                else {
                    merged = other;
                    this.carets[targetIndex] = caret;
                }
            }
            else {
                this.carets.Remove(caret.Index);
                this.carets[targetIndex] = caret;
                caret.Index = targetIndex;
                merged = null;
            }
        }

        private void OnCaretCreated(int index, Caret caret) {

        }

        private void OnCaretRemoved(int index, Caret caret) {

        }
    }
}