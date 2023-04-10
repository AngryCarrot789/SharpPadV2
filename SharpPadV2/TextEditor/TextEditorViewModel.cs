using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using SharpPadV2.Core;
using SharpPadV2.Core.Actions;
using SharpPadV2.Core.AdvancedContextService;
using SharpPadV2.Core.AdvancedContextService.Actions;
using SharpPadV2.Core.AdvancedContextService.Base;
using SharpPadV2.Core.Interactivity;
using SharpPadV2.Core.TextEditor;
using SharpPadV2.Core.Views.Dialogs.FilePicking;
using SharpPadV2.Core.Views.Dialogs.Message;
using SharpPadV2.Utils;

namespace SharpPadV2.TextEditor {
    public class TextEditorViewModel : BaseViewModel, IContextProvider, IFileDropNotifier {
        private string currentEncoding;
        public string CurrentEncoding {
            get => this.currentEncoding;
            set => this.RaisePropertyChanged(ref this.currentEncoding, value);
        }

        private double lineHeight;
        public double LineHeight {
            get => this.lineHeight;
            set => this.RaisePropertyChanged(ref this.lineHeight, value);
        }

        private string editorName;
        public string EditorName {
            get => this.editorName;
            set => this.RaisePropertyChanged(ref this.editorName, value);
        }

        private string filePath;
        public string FilePath {
            get => this.filePath;
            private set => this.RaisePropertyChanged(ref this.filePath, value);
        }

        private string text;
        public string Text {
            get => this.text;
            set {
                this.RaisePropertyChanged(ref this.text, value);
                this.IsDirty = true;
            }
        }

        private string fontFamily;
        public string FontFamily {
            get => this.fontFamily;
            set => this.RaisePropertyChanged(ref this.fontFamily, value);
        }

        private int fontSize;
        public int FontSize {
            get => this.fontSize;
            set => this.RaisePropertyChanged(ref this.fontSize, value);
        }

        private bool hasSavedOnce;
        public bool HasSavedOnce {
            get => this.hasSavedOnce;
            private set => this.RaisePropertyChanged(ref this.hasSavedOnce, value);
        }

        private bool isDirty;
        public bool IsDirty {
            get => this.isDirty;
            set => this.RaisePropertyChangedIfChanged(ref this.isDirty, value);
        }

        private int lineSeparatorType;
        public int LineSeparatorType {
            get => this.lineSeparatorType;
            set {
                this.RaisePropertyChanged(ref this.lineSeparatorType, value);
                this.RaisePropertyChanged(nameof(this.LineSeparator));
            }
        }

        public string LineSeparator {
            get {
                switch (this.lineSeparatorType) {
                    case 1:  return "\n";   // LF
                    case 2:  return "\r";   // CR
                    default: return "\r\n"; // CRLF
                }
            }
        }

        public ICommand OpenNewFileContentsCommand { get; }
        public ICommand SaveFileCommand { get; }
        public ICommand SaveFileAsCommand { get; }
        public ICommand CutSelectionCommand { get; }
        public ICommand CopySelectionCommand { get; }
        public ICommand PasteClipboardCommand { get; }
        public ICommand DeleteSelectionCommand { get; }
        public ICommand SelectCurrentLineCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand CloseSelfCommand { get; }

        public Encoding FileEncoding { get; set; }

        public IEditorContainer Container { get; set; }

        public ITextEditor Editor => this.Container.Editor;

        public TextEditorViewModel(IEditorContainer container) {
            this.Container = container;
            this.FileEncoding = Encoding.UTF8;
            this.currentEncoding = "UTF8";
            this.fontFamily = "Consolas";
            this.fontSize = 14;
            // this.lineHeight = 5; // for rich text boxes
            this.lineHeight = 21;

            this.OpenNewFileContentsCommand = new AsyncRelayCommand(this.OpenFileActionAsync);
            this.SaveFileCommand = new AsyncRelayCommand(this.SaveFileActionAsync);
            this.SaveFileAsCommand = new AsyncRelayCommand(this.SaveFileAsActionAsync);
            this.CutSelectionCommand = new AsyncRelayCommand(this.CutSelectionActionAsync, this.CanExecuteSelectionUsageCommand);
            this.CopySelectionCommand = new AsyncRelayCommand(this.CopyLineOrSelectionActionAsync, this.CanExecuteSelectionUsageCommand);
            this.PasteClipboardCommand = new AsyncRelayCommand(this.PasteClipboardActionAsync);
            this.DeleteSelectionCommand = new AsyncRelayCommand(this.DeleteSelectionActionAsync, this.CanExecuteSelectionUsageCommand);
            this.SelectCurrentLineCommand = new AsyncRelayCommand(this.SelectLineActionAsync);
            this.UndoCommand = new AsyncRelayCommand(this.UndoActionAsync, () => this.Editor.CanUndo);
            this.RedoCommand = new AsyncRelayCommand(this.RedoActionAsync, () => this.Editor.CanRedo);
            this.CloseSelfCommand = new AsyncRelayCommand(async () => {
                await this.Container.CloseEditor(this);
            });
        }

        /*
            This could be added to the text editor control instead of having to call the c# register methods below
            
            <t:RZTextEditor.InputBindings>
                <sc:ShortcutCommandBinding ShortcutAndUsageId="Application/TextEditor/OpenFileShortcut" Command="{Binding OpenFileCommand, Mode=OneTime}"/>
                <sc:ShortcutCommandBinding ShortcutAndUsageId="Application/TextEditor/SaveFileShortcut" Command="{Binding SaveFileCommand, Mode=OneTime}"/>
                <sc:ShortcutCommandBinding ShortcutAndUsageId="Application/TextEditor/SaveFileAsShortcut" Command="{Binding SaveFileAsCommand, Mode=OneTime}"/>
                <sc:ShortcutCommandBinding ShortcutAndUsageId="Application/TextEditor/UndoShortcut" Command="{Binding UndoCommand, Mode=OneTime}"/>
                <sc:ShortcutCommandBinding ShortcutAndUsageId="Application/TextEditor/RedoShortcut" Command="{Binding RedoCommand, Mode=OneTime}"/>
                <sc:ShortcutCommandBinding ShortcutAndUsageId="Application/TextEditor/RedoShortcut2nd" Command="{Binding RedoCommand, Mode=OneTime}"/>
                <sc:ShortcutCommandBinding ShortcutAndUsageId="Application/TextEditor/CutShortcut" Command="{Binding CutSelectionCommand, Mode=OneTime}"/>
                <sc:ShortcutCommandBinding ShortcutAndUsageId="Application/TextEditor/CopyShortcut" Command="{Binding CopySelectionCommand, Mode=OneTime}"/>
                <sc:ShortcutCommandBinding ShortcutAndUsageId="Application/TextEditor/PasteShortcut" Command="{Binding PasteClipboardCommand, Mode=OneTime}"/>
                <sc:ShortcutCommandBinding ShortcutAndUsageId="Application/TextEditor/DeleteSelectionShortcut" Command="{Binding DeleteSelectionCommand, Mode=OneTime}"/>
            </t:RZTextEditor.InputBindings>
         */

        static TextEditorViewModel() {
            ActionManager.Instance.Register("actions.tabs.CloseTabAction",             AnAction.Lambda(EditorAction<TextEditorViewModel>(async x => await x.Container.CloseEditor(x)),         "Close Document",   "Closes/removes this document"));
            ActionManager.Instance.Register("actions.editor.OpenFileAction",           AnAction.Lambda(EditorAction<TextEditorViewModel>(async x => await x.OpenFileActionAsync()),            "Open File",        "Open a file into this editor"));
            ActionManager.Instance.Register("actions.editor.SaveFileAction",           AnAction.Lambda(EditorAction<TextEditorViewModel>(async x => await x.SaveFileActionAsync()),            "Save",             "Save this editor's contents to the current file or a new file"));
            ActionManager.Instance.Register("actions.editor.SaveFileAsAction",         AnAction.Lambda(EditorAction<TextEditorViewModel>(async x => await x.SaveFileAsActionAsync()),          "Save As...",       "Save this editor's contents to a new file"));
            ActionManager.Instance.Register("actions.editor.UndoAction",               AnAction.Lambda(EditorAction<TextEditorViewModel>(async x => await x.UndoActionAsync()),                "Undo",             "Undo your last edit"));
            ActionManager.Instance.Register("actions.editor.RedoAction",               AnAction.Lambda(EditorAction<TextEditorViewModel>(async x => await x.RedoActionAsync()),                "Redo",             "Redo your last edit"));
            ActionManager.Instance.Register("actions.editor.CutAction",                AnAction.Lambda(EditorAction<TextEditorViewModel>(async x => await x.CutSelectionActionAsync()),        "Cut",              "Cut your selection or current line"));
            ActionManager.Instance.Register("actions.editor.CopyAction",               AnAction.Lambda(EditorAction<TextEditorViewModel>(async x => await x.CopyLineOrSelectionActionAsync()), "Copy",             "Copy your selection or current line"));
            ActionManager.Instance.Register("actions.editor.DuplicateAction",          AnAction.Lambda(EditorAction<TextEditorViewModel>(async x => await x.DuplicateSelectionActionAsync()),  "Duplicate",        "Duplicate your selection or current line"));
            ActionManager.Instance.Register("actions.editor.PasteAction",              AnAction.Lambda(EditorAction<TextEditorViewModel>(async x => await x.PasteClipboardActionAsync()),      "Paste",            "Paste the clipboard into the editor"));
            ActionManager.Instance.Register("actions.editor.DeleteSelectionAction",    AnAction.Lambda(EditorAction<TextEditorViewModel>(async x => await x.DeleteSelectionActionAsync()),     "Delete Selection", "Delete the selection you made"));
            ActionManager.Instance.Register("actions.editor.SelectLineAction",         AnAction.Lambda(EditorAction<TextEditorViewModel>(async x => await x.SelectLineActionAsync()),          "Select Line",      "Selects an entire line"));
            ActionManager.Instance.Register("actions.editor.AddOrRemoveCaretAtCursor", AnAction.Lambda(EditorAction<TextEditorViewModel>(async x => await x.AddOrRemoveCaretActionAsync())));
        }

        public static async Task<TextEditorViewModel> OpenFileOrShowError(IEditorContainer container, string filePath) {
            TextEditorViewModel viewModel = new TextEditorViewModel(container);
            try {
                await viewModel.OpenFileActionAsync(filePath);
                return viewModel;
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageAsync("Failed to open file", $"Failed to open {filePath}:\n{e.Message}");
                return null;
            }
        }

        private static Func<AnActionEventArgs, Task<bool>> EditorAction<T>(Func<T, Task> action) {
            return async (x) => {
                if (x.DataContext.TryGetContext(out T editor)) {
                    await action(editor);
                    return true;
                }
                else {
                    return false;
                }
            };
        }

        private bool CanExecuteSelectionUsageCommand() {
            return this.Editor.SelectionLength > 0;
        }

        public async Task OpenFileActionAsync(string path, bool setFilePath = true) {
            FileInfo info = new FileInfo(path);
            string fileName = setFilePath ? Path.GetFileName(path) : null;
            byte[] data = new byte[info.Length];
            using (FileStream stream = info.OpenRead()) {
                await stream.ReadAsync(data, 0, data.Length);
            }

            Encoding encoding = FileUtils.ParseEncoding(data, out int offset, out string encodingName);
            if (encoding == null) {
                encoding = Encoding.UTF8;
                encodingName = "UTF8";
            }

            this.SetEditorTextBytes(data, offset, encoding);
            this.FileEncoding = encoding;
            this.CurrentEncoding = encodingName;

            if (setFilePath) {
                this.FilePath = path;
                this.EditorName = fileName;
            }
        }

        public async Task<bool> OpenFileActionAsync() {
            OpenFileDialog ofd = new OpenFileDialog {
                Filter = Filter.Of().AddFilter("Text", "txt").AddAllFiles().ToString(),
                Title = "Select a file to open",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (ofd.ShowDialog() == true) {
                await this.OpenFileActionAsync(ofd.FileName);
                return true;
            }

            return false;
        }

        public async Task<bool> SaveFileActionAsync() {
            if (File.Exists(this.filePath) || this.HasSavedOnce) {
                await this.SaveFileActionAsync(this.filePath);
                return true;
            }
            else {
                return await this.SaveFileAsActionAsync();
            }
        }

        public async Task<bool> SaveFileAsActionAsync() {
            SaveFileDialog sfd = new SaveFileDialog() {
                Filter = Filter.Of().AddFilter("Text", "txt").AddAllFiles().ToString(),
                Title = "Save this document to a file",
                FileName = File.Exists(this.FilePath) ? this.FilePath : (this.editorName ?? "FileNameHere.txt")
            };

            if (sfd.ShowDialog() == true) {
                this.FilePath = sfd.FileName;
                this.EditorName = Path.GetFileName(this.FilePath);
                await this.SaveFileActionAsync(this.FilePath);
                return true;
            }
            else {
                return false;
            }
        }

        public async Task SaveFileActionAsync(string filePath) {
            try {
                await this.SaveFile(filePath);
                this.HasSavedOnce = true;
                this.IsDirty = false;
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageAsync("Failed to save", $"Failed to save document to {this.filePath}:\n{e.Message}");
            }
        }

        public async Task SaveFile(string filePath) {
            using (FileStream stream = File.OpenWrite(filePath)) {
                byte[] bytes = this.GetEditorTextBytes();
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        public async Task CutSelectionActionAsync() {
            try {
                this.Editor.CutLineOrSelection();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageAsync("Failed to perform action", $"Could not cut content: {e.Message}");
            }
        }

        public async Task CopyLineOrSelectionActionAsync() {
            try {
                this.Editor.CopyLineOrSelection();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageAsync("Failed to perform action", $"Could not copy content: {e.Message}");
            }
        }

        public async Task DuplicateSelectionActionAsync() {
            try {
                this.Editor.DuplicateLineOrSelection();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageAsync("Failed to perform action", $"Could not duplicate line or selection: {e.Message}");
            }
        }

        public async Task PasteClipboardActionAsync() {
            try {
                this.Editor.PasteClipboard();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageAsync("Failed to perform action", $"Could not paste content: {e.Message}");
            }
        }

        public Task DeleteSelectionActionAsync() {
            this.Editor.SetSelectedText("");
            return Task.CompletedTask;
        }

        public async Task SelectLineActionAsync() {
            try {
                this.Editor.SelectEntireCurrentLine();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageAsync("Failed to perform action", $"Could not select entire line: {e.Message}");
            }
        }

        public async Task AddOrRemoveCaretActionAsync() {
            // try {
            //     this.Editor.CaretManager.AddOrRemoveNewCaret();
            // }
            // catch (Exception e) {
            //     await IoC.MessageDialogs.ShowMessageAsync("Failed to perform action", $"Could not add or remove caret: {e.Message}");
            // }
        }

        public Task UndoActionAsync() {
            this.Editor.Undo();
            return Task.CompletedTask;
        }

        public Task RedoActionAsync() {
            this.Editor.Redo();
            return Task.CompletedTask;
        }

        public string GetEditorText() {
            string editorText = this.Text;
            if (editorText.Length > 0) {
                editorText = editorText.Replace(Environment.NewLine, this.LineSeparator);
            }

            return editorText;
        }

        public byte[] GetEditorTextBytes() {
            string editorText = this.GetEditorText();
            return this.FileEncoding.GetBytes(editorText);
        }

        public string SetEditorTextBytes(byte[] bytes, int offset, Encoding encoding) {
            this.Text = "";
            string editorText = encoding.GetString(bytes, offset, bytes.Length - offset);
            this.LineSeparatorType = TextUtils.DetectSeparatorType(editorText);
            if (editorText.Length > 0) {
                editorText = editorText.Replace(this.LineSeparator, Environment.NewLine);
            }

            this.Text = editorText;
            this.IsDirty = false;
            return editorText;
        }

        public List<IContextEntry> GetContext(List<IContextEntry> list) {
            list.Add(new ActionContextEntry(this, "actions.editor.UndoAction"));
            list.Add(new ActionContextEntry(this, "actions.editor.RedoAction"));
            list.Add(ContextEntrySeparator.Instance);
            if (this.CanExecuteSelectionUsageCommand()) {
                list.Add(new ActionContextEntry(this, "actions.editor.CutAction"));
                list.Add(new ActionContextEntry(this, "actions.editor.CopyAction"));
                list.Add(new ActionContextEntry(this, "actions.editor.PasteAction"));
                list.Add(new ActionContextEntry(this, "actions.editor.DeleteSelectionAction"));
            }
            else {
                list.Add(new ActionContextEntry(this, "actions.editor.PasteAction"));
            }

            list.Add(new ActionContextEntry(this, "actions.editor.SelectLineAction"));
            return list;
        }

        public Task<bool> CanDrop(string[] paths, FileDropType type) {
            return Task.FromResult(paths.Length == 1 && File.Exists(paths[0]));
        }

        public async Task<FileDropType> OnFilesDropped(string[] paths, FileDropType type) {
            if (paths.Length != 1) {
                return type;
            }

            try {
                await this.OpenFileActionAsync(paths[0]);
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageAsync("Failed to open file", $"Failed to open {paths[0]}:\n{e.Message}");
            }

            return type;
        }

        public async Task<bool> OnUserTryRemoveActionAsync() {
            if (this.IsDirty) {
                MsgDialogResult x = await IoC.MessageDialogs.ShowDialogAsync("Unsaved work", "You have unsaved work. Do you want to save the file?", MsgDialogType.YesNoCancel, MsgDialogResult.Yes);
                if (x == MsgDialogResult.Cancel) {
                    return false;
                }

                if (x == MsgDialogResult.Yes) {
                    await this.SaveFileActionAsync();
                }

                this.text = "";
                this.IsDirty = false;
            }

            return true;
        }
    }
}