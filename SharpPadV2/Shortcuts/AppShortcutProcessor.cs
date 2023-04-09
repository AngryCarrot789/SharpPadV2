using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SharpPadV2.Core;
using SharpPadV2.Core.Actions;
using SharpPadV2.Core.Shortcuts.Inputs;
using SharpPadV2.Core.Shortcuts.Managing;
using SharpPadV2.Core.Shortcuts.Usage;
using SharpPadV2.Core.Utils;

namespace SharpPadV2.Shortcuts {
    public class AppShortcutProcessor : ShortcutProcessor {
        public new AppShortcutManager Manager => (AppShortcutManager) base.Manager;

        public string CurrentInputBindingUsageID { get; set; } = AppShortcutManager.DEFAULT_USAGE_ID;

        public AppShortcutProcessor(ShortcutManager manager) : base(manager) {

        }

        private static AppShortcutProcessor GetWindowProcessor(object sender) {
            return sender is Window window ? UIFocusGroup.GetShortcutProcessor(window) : null;
        }

        public static bool CanProcessEvent(DependencyObject obj, bool isPreviewEvent) {
            return UIFocusGroup.GetUsePreviewEvents(obj) == isPreviewEvent;
        }

        // Using async void here could possibly be dangerous if the awaited processor method (e.g. OnMouseStroke) halts
        // for a while due to a dialog for example. However... the methods should only really be callable when the window
        // is actually focused. But if the "root" event source is not a window then it could possibly be a problem
        // Could maybe implement a bool flag to state if it's current being processed or not?

        public async void OnWindowMouseDown(object sender, MouseButtonEventArgs e, bool isPreviewEvent) {
            if (e.OriginalSource is DependencyObject focused && CanProcessEvent(focused, isPreviewEvent)) {
                UIFocusGroup.ProcessFocusGroupChange(focused);

                try {
                    this.CurrentInputBindingUsageID = UIFocusGroup.GetUsageID(focused) ?? AppShortcutManager.DEFAULT_USAGE_ID;
                    this.SetupDataContext(focused);
                    MouseStroke stroke = new MouseStroke((int) e.ChangedButton, (int) Keyboard.Modifiers, e.ClickCount);
                    if (await this.OnMouseStroke(UIFocusGroup.FocusedGroupPath, stroke)) {
                        e.Handled = true;
                    }
                }
                finally {
                    this.CurrentDataContext = null;
                    this.CurrentInputBindingUsageID = AppShortcutManager.DEFAULT_USAGE_ID;
                }
            }
        }

        public async void OnWindowMouseWheel(object sender, MouseWheelEventArgs e, bool isPreviewEvent) {
            if (e.OriginalSource is DependencyObject focused && CanProcessEvent(focused, isPreviewEvent)) {
                int button;
                if (e.Delta < 0) {
                    button = AppShortcutManager.BUTTON_WHEEL_DOWN;
                }
                else if (e.Delta > 0) {
                    button = AppShortcutManager.BUTTON_WHEEL_UP;
                }
                else {
                    return;
                }

                try {
                    this.CurrentInputBindingUsageID = UIFocusGroup.GetUsageID(focused) ?? AppShortcutManager.DEFAULT_USAGE_ID;
                    this.SetupDataContext(focused);
                    MouseStroke stroke = new MouseStroke(button, (int) Keyboard.Modifiers, 0, e.Delta);
                    if (await this.OnMouseStroke(UIFocusGroup.FocusedGroupPath, stroke)) {
                        e.Handled = true;
                    }
                }
                finally {
                    this.CurrentDataContext = null;
                    this.CurrentInputBindingUsageID = AppShortcutManager.DEFAULT_USAGE_ID;
                }
            }
        }

        public async void OnKeyEvent(object sender, DependencyObject focused, KeyEventArgs e, bool isRelease, bool isPreviewEvent) {
            if (e.Handled) {
                return;
            }

            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (ShortcutUtils.IsModifierKey(key) || key == Key.DeadCharProcessed) {
                return;
            }

            if (!CanProcessEvent(focused, isPreviewEvent)) {
                return;
            }

            AppShortcutProcessor processor = GetWindowProcessor(sender);
            if (processor == null) {
                return;
            }

            try {
                this.CurrentInputBindingUsageID = UIFocusGroup.GetUsageID(focused) ?? AppShortcutManager.DEFAULT_USAGE_ID;
                this.SetupDataContext(focused);
                KeyStroke stroke = new KeyStroke((int) key, (int) Keyboard.Modifiers, isRelease);
                if (await processor.OnKeyStroke(UIFocusGroup.FocusedGroupPath, stroke)) {
                    e.Handled = true;
                }
            }
            finally {
                this.CurrentDataContext = null;
                this.CurrentInputBindingUsageID = AppShortcutManager.DEFAULT_USAGE_ID;
            }
        }

        public void SetupDataContext(DependencyObject obj) {
            if (obj is FrameworkElement element) {
                this.CurrentDataContext = element.DataContext;
                if (this.CurrentDataContext is IHasDataContext iHas) {
                    this.CurrentDataContext = iHas.DataContext;
                }
            }
            else if (obj is IHasDataContext iHas) {
                this.CurrentDataContext = iHas.DataContext;
            }
        }

        public override async Task<bool> OnShortcutActivated(GroupedShortcut shortcut) {
            // ShortcutInputGesture input = ShortcutInputGesture.CurrentInputGesture;
            // if (input?.ShortcutKeyBinding != null && shortcut.Path == input.ShortcutKeyBinding.ShortcutID) {
            //     input.IsCompleted = true;
            // }

            bool finalResult = false;
            if (AppShortcutManager.InputBindingCallbackMap.TryGetValue(shortcut.Path, out Dictionary<string, List<ShortcutActivateHandler>> usageMap)) {
                if ((shortcut.IsGlobal || shortcut.Group.IsGlobal) && usageMap.TryGetValue(AppShortcutManager.DEFAULT_USAGE_ID, out List<ShortcutActivateHandler> callbacks2) && callbacks2.Count > 0) {
                    IoC.BroadcastShortcutActivity($"Activated global shortcut: {shortcut}. Calling {callbacks2.Count} callbacks...");
                    foreach (ShortcutActivateHandler callback in callbacks2) {
                        finalResult |= await callback(this, shortcut);
                    }
                    IoC.BroadcastShortcutActivity($"Activated global shortcut: {shortcut}. Calling {callbacks2.Count} callbacks... Complete!");
                }
                else if (usageMap.TryGetValue(this.CurrentInputBindingUsageID, out List<ShortcutActivateHandler> callbacks1) && callbacks1.Count > 0) {
                    IoC.BroadcastShortcutActivity($"Activated shortcut: {shortcut}. Calling {callbacks1.Count} callbacks...");
                    foreach (ShortcutActivateHandler callback in callbacks1) {
                        finalResult |= await callback(this, shortcut);
                    }
                    IoC.BroadcastShortcutActivity($"Activated shortcut: {shortcut}. Calling {callbacks1.Count} callbacks... Complete!");
                }
            }

            if (!finalResult && !string.IsNullOrWhiteSpace(shortcut.ActionID)) {
                finalResult = await ActionManager.Instance.Execute(shortcut.ActionID, this.CurrentDataContext);
            }

            return finalResult;
        }

        public override bool OnNoSuchShortcutForKeyStroke(string @group, in KeyStroke stroke) {
            if (stroke.IsKeyDown) {
                IoC.BroadcastShortcutActivity("No such shortcut for key stroke: " + stroke + " in group: " + group);
            }

            return base.OnNoSuchShortcutForKeyStroke(@group, in stroke);
        }

        public override bool OnNoSuchShortcutForMouseStroke(string @group, in MouseStroke stroke) {
            IoC.BroadcastShortcutActivity("No such shortcut for mouse stroke: " + stroke + " in group: " + group);
            return base.OnNoSuchShortcutForMouseStroke(@group, in stroke);
        }

        public override bool OnCancelUsageForNoSuchNextKeyStroke(IShortcutUsage usage, GroupedShortcut shortcut, in KeyStroke stroke) {
            IoC.BroadcastShortcutActivity("No such shortcut for next key stroke: " + stroke);
            return base.OnCancelUsageForNoSuchNextKeyStroke(usage, shortcut, in stroke);
        }

        public override bool OnCancelUsageForNoSuchNextMouseStroke(IShortcutUsage usage, GroupedShortcut shortcut, in MouseStroke stroke) {
            IoC.BroadcastShortcutActivity("No such shortcut for next mouse stroke: " + stroke);
            return base.OnCancelUsageForNoSuchNextMouseStroke(usage, shortcut, in stroke);
        }

        public override bool OnShortcutUsagesCreated() {
            StringJoiner joiner = new StringJoiner(new StringBuilder(), ", ");
            foreach (KeyValuePair<IShortcutUsage, GroupedShortcut> pair in this.ActiveUsages) {
                joiner.Append(pair.Key.CurrentStroke.ToString());
            }

            IoC.BroadcastShortcutActivity("Waiting for next input: " + joiner);
            return base.OnShortcutUsagesCreated();
        }

        public override bool OnSecondShortcutUsagesProgressed() {
            StringJoiner joiner = new StringJoiner(new StringBuilder(), ", ");
            foreach (KeyValuePair<IShortcutUsage, GroupedShortcut> pair in this.ActiveUsages) {
                joiner.Append(pair.Key.CurrentStroke.ToString());
            }

            IoC.BroadcastShortcutActivity("Waiting for next input: " + joiner);
            return base.OnSecondShortcutUsagesProgressed();
        }
    }
}