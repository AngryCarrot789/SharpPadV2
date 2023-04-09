using SharpPadV2.Core;
using SharpPadV2.TextEditor;

namespace SharpPadV2 {
    public class MainViewModel : BaseViewModel {
        private TextEditorViewModel currentTextEditor;
        public TextEditorViewModel CurrentTextEditor {
            get => this.currentTextEditor;
            set => this.RaisePropertyChanged(ref this.currentTextEditor, value);
        }

        public MainViewModel() {

        }
    }
}