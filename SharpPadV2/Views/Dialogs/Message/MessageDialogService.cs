using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using SharpPadV2.Core.Views.Dialogs.Message;
using SharpPadV2.Views.Dialogs.FilePicking;

namespace SharpPadV2.Views.Dialogs.Message {
    public class MessageDialogService : IMessageDialogService {
        public async Task ShowMessageAsync(string caption, string message) {
            void Action() {
                MessageBox.Show(FolderPicker.GetCurrentActiveWindow(), message, caption);
            }

            await InvokeAsync(Action);
        }

        public async Task ShowMessageAsync(string message) {
            await this.ShowMessageAsync("Information", message);
        }

        public async Task<MsgDialogResult> ShowDialogAsync(string caption, string message, MsgDialogType type, MsgDialogResult defaultResult) {
            MsgDialogResult Action() {
                switch (MessageBox.Show(FolderPicker.GetCurrentActiveWindow(), message, caption, (MessageBoxButton) type, MessageBoxImage.Information, (MessageBoxResult) defaultResult)) {
                    case MessageBoxResult.OK: return MsgDialogResult.OK;
                    case MessageBoxResult.Cancel: return MsgDialogResult.Cancel;
                    case MessageBoxResult.Yes: return MsgDialogResult.Yes;
                    case MessageBoxResult.No: return MsgDialogResult.No;
                    default: return MsgDialogResult.Cancel;
                }
            }

            return await InvokeAsync(Action);
        }

        public async Task<bool> ShowYesNoDialogAsync(string caption, string message, bool defaultResult = true) {
            bool Action() {
                switch (MessageBox.Show(FolderPicker.GetCurrentActiveWindow(), message, caption, MessageBoxButton.YesNo, MessageBoxImage.Information, defaultResult ? MessageBoxResult.Yes : MessageBoxResult.No)) {
                    case MessageBoxResult.Yes: return true;
                    default: return false;
                }
            }

            return await InvokeAsync(Action);
        }

        public static async Task InvokeAsync(Action func) {
            Application app = Application.Current;
            Dispatcher dispatcher;
            if (app != null && (dispatcher = app.Dispatcher) != null) {
                if (dispatcher.CheckAccess()) {
                    func();
                }
                else {
                    await dispatcher.InvokeAsync(func);
                }
            }
        }

        public static async Task<TResult> InvokeAsync<TResult>(Func<TResult> func) {
            Application app = Application.Current;
            Dispatcher dispatcher;
            if (app != null && (dispatcher = app.Dispatcher) != null) {
                return dispatcher.CheckAccess() ? func() : await dispatcher.InvokeAsync(func);
            }

            return default;
        }
    }
}