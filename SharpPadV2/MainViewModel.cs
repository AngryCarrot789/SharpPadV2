using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using SharpPadV2.Core;
using SharpPadV2.Core.Interactivity;
using SharpPadV2.Core.TextEditor;
using SharpPadV2.Core.Views.Dialogs.FilePicking;
using SharpPadV2.TextEditor;

namespace SharpPadV2 {
    public class MainViewModel : BaseViewModel, IFileDropNotifier, IEditorContainer {
        private readonly ObservableCollection<TextEditorViewModel> editors;
        public ReadOnlyObservableCollection<TextEditorViewModel> Editors { get; }

        private TextEditorViewModel activeTextEditor;
        public TextEditorViewModel ActiveTextEditor {
            get => this.activeTextEditor;
            set {
                this.RaisePropertyChanged(ref this.activeTextEditor, value);
                this.UpdateVisibility();
            }
        }

        private bool isEditorVisible;
        public bool IsEditorVisible {
            get => this.isEditorVisible;
            set => this.RaisePropertyChanged(ref this.isEditorVisible, value);
        }

        public ICommand NewEmptyFileCommand { get; }
        public ICommand OpenFileCommand { get; }
        public AsyncRelayCommand<TextEditorViewModel> CloseEditorCommand { get; }

        public ITextEditor Editor { get; }

        public MainViewModel(ITextEditor editor) {
            this.NewEmptyFileCommand = new RelayCommand(() => {
                this.AddEditor(new TextEditorViewModel(this) {
                    EditorName = $"New Document {this.editors.Count + 1}"
                });

                this.ActiveTextEditor = this.editors[this.editors.Count - 1];
            });

            this.OpenFileCommand = new AsyncRelayCommand(this.OpenFileActionAsync);
            this.CloseEditorCommand = new AsyncRelayCommand<TextEditorViewModel>(async (x) => {
                if (await x.OnUserTryRemoveActionAsync()) {
                    this.RemoveEditor(x);
                }
            });

            this.Editor = editor;
            this.editors = new ObservableCollection<TextEditorViewModel>();
            this.Editors = new ReadOnlyObservableCollection<TextEditorViewModel>(this.editors);

            TextEditorViewModel initial = new TextEditorViewModel(this) {
                EditorName = "New Document 1"
            };

            this.AddEditor(initial);
            this.ActiveTextEditor = initial;
        }

        public async Task<bool> CloseEditor(TextEditorViewModel editor) {
            if (await editor.OnUserTryRemoveActionAsync()) {
                this.RemoveEditor(editor);
                return true;
            }

            return false;
        }

        public async Task OpenFileActionAsync() {
            OpenFileDialog ofd = new OpenFileDialog {
                Filter = Filter.Of().AddFilter("Text", "txt").AddAllFiles().ToString(),
                Title = "Select a file to open",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (ofd.ShowDialog() == true) {
                TextEditorViewModel editor = await TextEditorViewModel.OpenFileOrShowError(this, ofd.FileName);
                if (editor != null) {
                    this.AddEditor(editor);
                }
            }
        }

        public void AddEditor(TextEditorViewModel editor) {
            this.editors.Add(editor);
            this.UpdateVisibility();
        }

        public void RemoveEditor(TextEditorViewModel editor) {
            this.editors.Remove(editor);
            this.UpdateVisibility();
        }

        private void UpdateVisibility() {
            this.IsEditorVisible = this.editors.Count > 0 && this.activeTextEditor != null;
        }

        public async Task<bool> CanDrop(string[] paths, FileDropType type) {
            if (paths.Any(File.Exists)) {
                return true;
            }

            if (paths.Length > 0) {
                await IoC.MessageDialogs.ShowMessageAsync("Invalid drop", paths.Length > 1 ? $"These item cannot be dropped" : $"This item canont be dropped: {paths[0]}");
            }

            return false;
        }

        public async Task<FileDropType> OnFilesDropped(string[] paths, FileDropType type) {
            bool hasAny = false;
            foreach (string file in paths) {
                TextEditorViewModel editor = await TextEditorViewModel.OpenFileOrShowError(this, file);
                if (editor != null) {
                    this.AddEditor(editor);
                    hasAny = true;
                }
            }

            if (hasAny && this.editors.Count > 0) {
                this.ActiveTextEditor = this.editors[this.editors.Count - 1];
            }

            return type;
        }
    }
}