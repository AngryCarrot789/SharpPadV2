namespace SharpPadV2.TextEditor {
    public class Caret {
        public int Index { get; set; }

        public int SelectionLength { get; set; }

        public CaretManager Manager { get; }

        public Caret(CaretManager manager) {
            this.Manager = manager;
        }
    }
}