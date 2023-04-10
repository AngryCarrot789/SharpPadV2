using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SharpPadV2.Core;
using SharpPadV2.Core.Shortcuts;
using SharpPadV2.Core.Shortcuts.Inputs;
using SharpPadV2.Core.Shortcuts.Managing;
using SharpPadV2.Core.Shortcuts.Usage;

namespace SharpPadV2.Shortcuts.Bindings {
    /// <summary>
    /// A global shortcut command binding lets you execute a command when a specific shortcut is activated
    /// <para>
    /// Care must be taken with these, because they are registered globally, meaning there is a memory leak potential.
    /// Every time an instance of this class registers itself, a callback will be invoked when the shortcut is invoked
    /// (and that callback will intern execute the command). To unregister, you can set <see cref="ShortcutPath"/> to null
    /// </para>
    /// <para>
    /// You can use the "UsageID" part to further filter which command finally gets called but overall, you can't for
    /// example use this in a ListBoxItem or TabItem or some sort of control that is generally dynamically created/removed, because
    /// input bindings do not have an "added/removed from visual tree" event
    /// </para>
    /// </summary>
    public class ContextualShortcutCommandBinding : InputBinding {
        public static readonly DependencyProperty ShortcutPathProperty =
            DependencyProperty.Register(
                "ShortcutPath",
                typeof(string),
                typeof(ContextualShortcutCommandBinding),
                new PropertyMetadata(null, (d, e) => ((ContextualShortcutCommandBinding) d).OnShortcutPathPropertyChanged((string) e.OldValue, (string) e.NewValue)));

        /// <summary>
        /// <para>
        /// The target shortcut and the usage id as a single string (cannot be two properties due to the <see cref="InputBinding"/> limitations,
        /// such as the lack of an Loaded or AddedToVisualTree event/function).
        /// </para>
        /// <para>
        /// The ShortcutID and UsageID are separated by a colon ':' character. The UsageID is optional, so you can just set this as the ShortcutID.
        /// A UsageID is only nessesary if you plan on using the same shortcut to invoke code in multiple places, so that only the right callback gets fired
        /// </para>
        /// <para>
        /// Examples: "Path/To/My/Shortcut:UsageID", "My/Action"
        /// </para>
        /// </summary>
        public string ShortcutPath {
            get => (string) this.GetValue(ShortcutPathProperty);
            set => this.SetValue(ShortcutPathProperty, value);
        }

        public GroupedShortcut Shortcut => WPFShortcutManager.Instance.FindShortcutByPath(this.ShortcutPath);

        public IShortcutUsage Usage { get; set; }

        public ContextualShortcutCommandBinding() {

        }

        private void OnShortcutPathPropertyChanged(string oldValue, string newValue) {
            this.Gesture = null;
            if (!string.IsNullOrWhiteSpace(newValue)) {
                this.Gesture = new KeyAndMouseInputGesture(this);
            }
        }

        private async Task<bool> OnShortcutFired(ShortcutProcessor processor, GroupedShortcut shortcut) {
            ICommand cmd = this.Command;
            object param = this.CommandParameter;
            if (cmd != null && cmd.CanExecute(param)) {
                if (cmd is BaseAsyncRelayCommand arc) {
                    await arc.ExecuteAsync(param);
                    return true;
                }
                else {
                    cmd.Execute(param);
                    return true;
                }
            }
            else {
                return false;
            }
        }

        protected override Freezable CreateInstanceCore() => new ContextualShortcutCommandBinding();

        public bool MatchKeyInput(object target, KeyEventArgs e) {
            if (this.Usage != null || !(target is DependencyObject obj)) {
                return false;
            }

            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (ShortcutUtils.IsModifierKey(key) || key == Key.DeadCharProcessed) {
                return false;
            }

            GroupedShortcut shortcut = this.Shortcut;
            // if (shortcut != null && shortcut.Shortcut is IKeyboardShortcut ks) {
            //     if (UIFocusGroup.GetShortcutProcessor(obj) is WPFShortcutProcessor processor) {
            //         if (!WPFShortcutProcessor.CanProcessEvent(obj, e.RoutedEvent == UIElement.PreviewKeyUpEvent || e.RoutedEvent == UIElement.PreviewKeyDownEvent)) {
            //             return;
            //         }
            //         try {
            //             this.CurrentInputBindingUsageID = UIFocusGroup.GetUsageID(focused) ?? WPFShortcutManager.DEFAULT_USAGE_ID;
            //             this.SetupDataContext(sender, focused);
            //             KeyStroke stroke = new KeyStroke((int) key, (int) Keyboard.Modifiers, isRelease);
            //             if (await processor.OnKeyStroke(UIFocusGroup.FocusedGroupPath, stroke)) {
            //                 e.Handled = true;
            //             }
            //         }
            //         finally {
            //             this.CurrentDataContext = null;
            //             this.CurrentInputBindingUsageID = WPFShortcutManager.DEFAULT_USAGE_ID;
            //         }
            //     }
            // }

            return false;
        }

        public bool MatchMouseInput(object target, MouseEventArgs e) {
            throw new System.NotImplementedException();
        }
    }

    public class KeyAndMouseInputGesture : InputGesture {
        public ContextualShortcutCommandBinding Shortcut { get; }

        public KeyAndMouseInputGesture(ContextualShortcutCommandBinding shortcut) {
            this.Shortcut = shortcut;
        }

        public override bool Matches(object targetElement, InputEventArgs args) {
            if (this.Shortcut.Usage != null && this.Shortcut.Usage.IsCompleted) {
                return false;
            }

            if (args is KeyEventArgs kea) {
                return this.Shortcut.MatchKeyInput(targetElement, kea);
            }
            else if (args is MouseEventArgs mea) {
                return this.Shortcut.MatchMouseInput(targetElement, mea);
            }
            else {
                return false;
            }
        }
    }
}