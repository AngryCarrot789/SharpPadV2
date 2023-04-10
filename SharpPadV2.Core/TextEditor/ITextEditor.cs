namespace SharpPadV2.Core.TextEditor {
    public interface ITextEditor {
        int CaretIndex { get; set; }
        int SelectionLength { get; set; }
        bool CanUndo { get; }
        bool CanRedo { get; }

        void SelectRange(int index, int length);
        void SetSelectedText(string text);
        void CutLineOrSelection();
        void CopyLineOrSelection();
        void DuplicateLineOrSelection();
        void PasteClipboard();
        void SelectEntireCurrentLine();
        void Undo();
        void Redo();
    }
}