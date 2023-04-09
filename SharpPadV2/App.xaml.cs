using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using SharpPadV2.Converters;
using SharpPadV2.Core;
using SharpPadV2.Core.Services;
using SharpPadV2.Core.Shortcuts.Managing;
using SharpPadV2.Core.Shortcuts.ViewModels;
using SharpPadV2.Services;
using SharpPadV2.Shortcuts;
using SharpPadV2.Shortcuts.Dialogs;
using SharpPadV2.Shortcuts.Views;
using SharpPadV2.Views.Dialogs.FilePicking;
using SharpPadV2.Views.Dialogs.Message;
using SharpPadV2.Views.Dialogs.UserInputs;

namespace SharpPadV2 {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private void Application_Startup(object sender, StartupEventArgs e) {
            IoC.MessageDialogs = new MessageDialogService();
            IoC.Dispatcher = new DispatcherDelegate(this);
            IoC.Clipboard = new ClipboardService();
            IoC.FilePicker = new FilePickDialogService();
            IoC.UserInput = new UserInputDialogService();
            IoC.ExplorerService = new WinExplorerService();
            InputStrokeViewModel.KeyToReadableString = KeyStrokeRepresentationConverter.ToStringFunction;
            InputStrokeViewModel.MouseToReadableString = MouseStrokeRepresentationConverter.ToStringFunction;
            IoC.KeyboardDialogs = new KeyboardDialogService();
            IoC.MouseDialogs = new MouseDialogService();
            IoC.ShortcutManager = AppShortcutManager.Instance;
            IoC.ShortcutManagerDialog = new ShortcutManagerDialogService();
            IoC.OnShortcutManagedChanged = (x) => {
                if (!string.IsNullOrWhiteSpace(x)) {
                    ShortcutGestureConverter.BroadcastChange();
                    // UpdatePath(this.Resources, x);
                }
            };

            string filePath = @"F:\VSProjsV2\SharpPadV2\SharpPadV2\Keymap.xml";
            if (File.Exists(filePath)) {
                AppShortcutManager.Instance.Root = null;
                using (FileStream stream = File.OpenRead(filePath)) {
                    ShortcutGroup group = WPFKeyMapDeserialiser.Instance.Deserialise(stream);
                    AppShortcutManager.Instance.Root = group;
                }
            }
            else {
                MessageBox.Show("Keymap file does not exist: " + filePath);
            }

            MainWindow window = new MainWindow();
            this.MainWindow = window;

            window.Show();
        }

        private class DispatcherDelegate : IDispatcher {
            private readonly App app;

            public DispatcherDelegate(App app) {
                this.app = app;
            }

            public void InvokeLater(Action action) {
                this.app.Dispatcher.Invoke(action, DispatcherPriority.Normal);
            }

            public void Invoke(Action action) {
                this.app.Dispatcher.Invoke(action);
            }

            public T Invoke<T>(Func<T> function) {
                return this.app.Dispatcher.Invoke(function);
            }

            public async Task InvokeAsync(Action action) {
                await this.app.Dispatcher.InvokeAsync(action);
            }

            public async Task<T> InvokeAsync<T>(Func<T> function) {
                return await this.app.Dispatcher.InvokeAsync(function);
            }
        }
    }
}
