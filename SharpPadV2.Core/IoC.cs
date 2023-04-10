using System;
using SharpPadV2.Core.Actions;
using SharpPadV2.Core.Services;
using SharpPadV2.Core.Shortcuts.Dialogs;
using SharpPadV2.Core.Shortcuts.Managing;
using SharpPadV2.Core.Views.Dialogs.FilePicking;
using SharpPadV2.Core.Views.Dialogs.Message;
using SharpPadV2.Core.Views.Dialogs.UserInputs;

namespace SharpPadV2.Core {
    public static class IoC {
        public static SimpleIoC Instance { get; } = new SimpleIoC();

        public static ActionManager ActionManager { get; } = new ActionManager();
        public static Action<string> OnShortcutManagedChanged { get; set; }
        public static Action<string> BroadcastShortcutActivity { get; set; }

        public static IDispatcher Dispatcher { get; set; }
        public static IClipboardService Clipboard { get; set; }
        public static IMessageDialogService MessageDialogs { get; set; }
        public static IFilePickDialogService FilePicker { get; set; }
        public static IUserInputDialogService UserInput { get; set; }
        public static IExplorerService ExplorerService { get; set; }

        public static IKeyboardDialogService KeyboardDialogs { get; set; }

        public static IMouseDialogService MouseDialogs { get; set; }

        public static ShortcutManager ShortcutManager { get; set; }
        public static IShortcutManagerDialogService ShortcutManagerDialog { get; set; }
    }
}