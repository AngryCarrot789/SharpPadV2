using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Win32;
using SharpPadV2.Converters;
using SharpPadV2.Core;
using SharpPadV2.Core.Actions;
using SharpPadV2.Core.AdvancedContextService;
using SharpPadV2.Core.AdvancedContextService.Base;
using SharpPadV2.Core.Utils;
using SharpPadV2.Core.Views.Dialogs.FilePicking;
using SharpPadV2.Utils;

namespace SharpPadV2.TextEditor {
    public class TextEditorViewModel : BaseViewModel, IContextProvider {
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

        private string filePath;
        public string FilePath {
            get => this.filePath;
            set => this.RaisePropertyChanged(ref this.filePath, value);
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

        public ICommand OpenFileCommand { get; }
        public ICommand SaveFileCommand { get; }
        public ICommand SaveFileAsCommand { get; }
        public ICommand CutSelectionCommand { get; }
        public ICommand CopySelectionCommand { get; }
        public ICommand PasteClipboardCommand { get; }
        public ICommand DeleteSelectionCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        // Keep a raw handle for now
        public RZTextEditor Editor { get; }

        public Encoding FileEncoding { get; set; }

        public TextEditorViewModel(RZTextEditor editor) {
            this.Editor = editor;
            this.FileEncoding = Encoding.UTF8;
            this.currentEncoding = "UTF8";
            this.fontFamily = "Consolas";
            this.fontSize = 14;
            this.lineHeight = 5;

            this.OpenFileCommand = new AsyncRelayCommand(this.OpenFileActionAsync);
            this.SaveFileCommand = new AsyncRelayCommand(this.SaveFileActionAsync);
            this.SaveFileAsCommand = new AsyncRelayCommand(this.SaveFileAsActionAsync);
            this.CutSelectionCommand = new AsyncRelayCommand(this.CutSelectionActionAsync, this.CanExecuteSelectionUsageCommand);
            this.CopySelectionCommand = new AsyncRelayCommand(this.CopySelectionActionAsync, this.CanExecuteSelectionUsageCommand);
            this.PasteClipboardCommand = new AsyncRelayCommand(this.PasteClipboardActionAsync);
            this.DeleteSelectionCommand = new AsyncRelayCommand(this.DeleteSelectionActionAsync, this.CanExecuteSelectionUsageCommand);
            this.UndoCommand = new AsyncRelayCommand(this.UndoActionAsync, () => this.Editor.CanUndo);
            this.RedoCommand = new AsyncRelayCommand(this.RedoActionAsync, () => this.Editor.CanRedo);
        }

        /*
            This could be added to the text editor control instead of having to call the c# register methods below
            
            <t:RZTextEditor.InputBindings>
                <sc:ShortcutCommandBinding ShortcutAndUsageId="Application/TextEditor/OpenFileAction" Command="{Binding OpenFileCommand, Mode=OneTime}"/>
                <sc:ShortcutCommandBinding ShortcutAndUsageId="Application/TextEditor/SaveFileAction" Command="{Binding SaveFileCommand, Mode=OneTime}"/>
                <sc:ShortcutCommandBinding ShortcutAndUsageId="Application/TextEditor/SaveFileAsAction" Command="{Binding SaveFileAsCommand, Mode=OneTime}"/>
                <sc:ShortcutCommandBinding ShortcutAndUsageId="Application/TextEditor/UndoAction" Command="{Binding UndoCommand, Mode=OneTime}"/>
                <sc:ShortcutCommandBinding ShortcutAndUsageId="Application/TextEditor/RedoAction" Command="{Binding RedoCommand, Mode=OneTime}"/>
                <sc:ShortcutCommandBinding ShortcutAndUsageId="Application/TextEditor/RedoAction2nd" Command="{Binding RedoCommand, Mode=OneTime}"/>
                <sc:ShortcutCommandBinding ShortcutAndUsageId="Application/TextEditor/CutAction" Command="{Binding CutSelectionCommand, Mode=OneTime}"/>
                <sc:ShortcutCommandBinding ShortcutAndUsageId="Application/TextEditor/CopyAction" Command="{Binding CopySelectionCommand, Mode=OneTime}"/>
                <sc:ShortcutCommandBinding ShortcutAndUsageId="Application/TextEditor/PasteAction" Command="{Binding PasteClipboardCommand, Mode=OneTime}"/>
                <sc:ShortcutCommandBinding ShortcutAndUsageId="Application/TextEditor/DeleteSelectionAction" Command="{Binding DeleteSelectionCommand, Mode=OneTime}"/>
            </t:RZTextEditor.InputBindings>
         */

        static TextEditorViewModel() {
            ActionManager.Instance.Register("actions.editor.OpenFileAction",        AnAction.Lambda(SimpleAction(async (x) => await x.OpenFileActionAsync())));
            ActionManager.Instance.Register("actions.editor.SaveFileAction",        AnAction.Lambda(SimpleAction(async (x) => await x.SaveFileActionAsync())));
            ActionManager.Instance.Register("actions.editor.SaveFileAsAction",      AnAction.Lambda(SimpleAction(async (x) => await x.SaveFileAsActionAsync())));
            ActionManager.Instance.Register("actions.editor.UndoAction",            AnAction.Lambda(SimpleAction(async (x) => await x.UndoActionAsync())));
            ActionManager.Instance.Register("actions.editor.RedoAction",            AnAction.Lambda(SimpleAction(async (x) => await x.RedoActionAsync())));
            ActionManager.Instance.Register("actions.editor.RedoAction2nd",         AnAction.Lambda(SimpleAction(async (x) => await x.RedoActionAsync())));
            ActionManager.Instance.Register("actions.editor.CutAction",             AnAction.Lambda(SimpleAction(async (x) => await x.CutSelectionActionAsync())));
            ActionManager.Instance.Register("actions.editor.CopyAction",            AnAction.Lambda(SimpleAction(async (x) => await x.CopySelectionActionAsync())));
            ActionManager.Instance.Register("actions.editor.PasteAction",           AnAction.Lambda(SimpleAction(async (x) => await x.PasteClipboardActionAsync())));
            ActionManager.Instance.Register("actions.editor.DeleteSelectionAction", AnAction.Lambda(SimpleAction(async (x) => await x.DeleteSelectionActionAsync())));
        }

        private static Func<AnActionEvent, Task<bool>> SimpleAction(Func<TextEditorViewModel, Task> action) {
            return async (x) => {
                if (x.DataContext is TextEditorViewModel editor) {
                    await action(editor);
                    return true;
                }
                else {
                    return false;
                }
            };
        }

        private bool CanExecuteSelectionUsageCommand() {
            return !this.Editor.Selection.IsEmpty;
        }

        public async Task OpenFileActionAsync() {
            await Task.Delay(5000);

            OpenFileDialog ofd = new OpenFileDialog {
                Filter = Filter.Of().AddFilter("Text", "txt").AddAllFiles().ToString(),
                Title = "Select a file to open",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (ofd.ShowDialog() == true) {
                FileInfo info = new FileInfo(ofd.FileName);
                byte[] data = new byte[info.Length];
                using (FileStream stream = info.OpenRead()) {
                    await stream.ReadAsync(data, 0, data.Length);
                }

                Encoding encoding = FileUtils.ParseEncoding(data, out int offset, out string encodingName);
                if (encoding == null) {
                    encoding = Encoding.UTF8;
                    encodingName = "UTF8";
                }

                string text = this.SetEditorTextBytes(data, offset, encoding);
                this.FileEncoding = encoding;
                this.CurrentEncoding = encodingName;
                this.LineSeparatorType = TextUtils.DetectSeparatorType(text);
            }
        }

        public async Task SaveFileActionAsync() {
            if (File.Exists(this.filePath) || this.HasSavedOnce) {
                await this.SaveFileActionAsync(this.filePath);
            }
            else {
                await this.SaveFileAsActionAsync();
            }
        }

        public async Task SaveFileAsActionAsync() {
            SaveFileDialog sfd = new SaveFileDialog() {
                Filter = Filter.Of().AddFilter("Text", "txt").AddAllFiles().ToString(),
                Title = "Save this document to a file"
            };

            if (sfd.ShowDialog() == true) {
                await this.SaveFileActionAsync(this.FilePath = sfd.FileName);
            }
        }

        public async Task SaveFileActionAsync(string filePath) {
            try {
                await this.SaveFile(filePath);
                this.HasSavedOnce = true;
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
                this.Editor.Cut();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageAsync("Failed to cut", $"Could not cut content: {e.Message}");
            }
        }

        public async Task CopySelectionActionAsync() {
            try {
                this.Editor.Copy();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageAsync("Failed to cut", $"Could not cut content: {e.Message}");
            }

        }

        public async Task PasteClipboardActionAsync() {
            try {
                this.Editor.Paste();
                foreach (Block block in this.Editor.Document.Blocks) {
                    block.ClearValue(TextElement.BackgroundProperty);
                }
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageAsync("Failed to cut", $"Could not cut content: {e.Message}");
            }
        }

        public Task DeleteSelectionActionAsync() {
            this.Editor.Selection.Text = "";
            return Task.CompletedTask;
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
            FlowDocument document = this.Editor.Document;
            if (document == null) {
                return "";
            }

            TextRange range = new TextRange(document.ContentStart, document.ContentEnd);
            string text = range.Text ?? "";
            if (text.Length > 0) {
                text = text.Replace(Environment.NewLine, this.LineSeparator);
            }

            return text;
        }

        public byte[] GetEditorTextBytes() {
            string text = this.GetEditorText();
            return this.FileEncoding.GetBytes(text);
        }

        public string SetEditorTextBytes(byte[] bytes, int offset, Encoding encoding) {
            FlowDocument document = this.Editor.Document;
            if (document == null) {
                this.Editor.Document = document = new FlowDocument();
            }

            document.Blocks.Clear();
            TextRange range = new TextRange(document.ContentStart, document.ContentEnd);

            string text = encoding.GetString(bytes, offset, bytes.Length - offset);
            range.Text = text;
            return text;
        }

        public List<IContextEntry> GetContext(List<IContextEntry> list) {
            list.Add(new CommandContextEntry("Undo", this.UndoCommand, null, null, "Undo your last modification"));
            list.Add(new CommandContextEntry("Redo", this.RedoCommand, null, null, "Redo your last undo action"));
            list.Add(ContextEntrySeparator.Instance);
            CommandContextEntry paste = new CommandContextEntry("Paste", this.PasteClipboardCommand, null, ShortcutGestureConverter.PathToGesture("Application/TextEditor/PasteAction"), "Paste the clipboard text to your caret/cursor");
            if (this.CanExecuteSelectionUsageCommand()) {
                list.Add(new CommandContextEntry("Cut", this.CutSelectionCommand, null, ShortcutGestureConverter.PathToGesture("Application/TextEditor/CutAction"), "Cut the selected text (Delete and add to clipboard)"));
                list.Add(new CommandContextEntry("Copy", this.CopySelectionCommand, null, ShortcutGestureConverter.PathToGesture("Application/TextEditor/CopyAction"), "Copy the selected text to clipboard"));
                list.Add(paste);
                list.Add(new CommandContextEntry("Delete", this.DeleteSelectionCommand, null, ShortcutGestureConverter.PathToGesture("Application/TextEditor/DeleteSelectionAction"), "Deletes the selected text"));
            }
            else {
                list.Add(paste);
            }

            return list;
        }
    }
}