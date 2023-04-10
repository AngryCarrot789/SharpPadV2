using System.ComponentModel;
using System.Linq;
using SharpPadV2.Core;
using SharpPadV2.Core.Views.Dialogs.Message;
using SharpPadV2.TextEditor;
using SharpPadV2.Views;

namespace SharpPadV2 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowEx {
        public MainWindow() {
            this.InitializeComponent();
            IoC.BroadcastShortcutActivity = (x) => {
                // this.ShortcutIndicatorBlock.Text = x ?? "";
            };

            this.DataContext = new MainViewModel(this.MainTextEditor.Editor);
        }

        protected override async void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);
            if (this.DataContext is MainViewModel view) {
                int count = view.Editors.Count(x => x.IsDirty);
                if (count < 1)
                    return;

                MsgDialogResult result = await IoC.MessageDialogs.ShowDialogAsync("Unsaved work", $"You have {count} unsaved document{(count == 1 ? "" : "s")}. Do you want to save them?", MsgDialogType.YesNoCancel, MsgDialogResult.Yes);
                if (result == MsgDialogResult.Cancel) {
                    e.Cancel = true;
                    return;
                }

                if (result == MsgDialogResult.No) {
                    return;
                }

                foreach (TextEditorViewModel editor in view.Editors) {
                    if (!await editor.SaveFileActionAsync()) {
                        e.Cancel = true;
                        return;
                    }
                }
            }
        }
    }
}
