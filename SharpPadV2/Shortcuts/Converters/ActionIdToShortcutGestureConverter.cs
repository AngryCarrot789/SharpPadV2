using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using SharpPadV2.Converters;
using SharpPadV2.Core.Actions;
using SharpPadV2.Core.Shortcuts.Managing;

namespace SharpPadV2.Shortcuts.Converters {
    public class ActionToShortcutGestureConverter : SpecialValueConverter<ActionToShortcutGestureConverter>, IValueConverter, INotifyPropertyChanged {
        public string NoSuchActionText { get; set; } = null;

        public int Version { get; private set; }

        public static void BroadcastChange() {
            Instance.Version++;
            Instance.OnPropertyChanged(nameof(Version));
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(value is string id)) {
                throw new Exception("Value is not a shortcut string");
            }

            return ActionIdToGesture(id, this.NoSuchActionText) ?? DependencyProperty.UnsetValue;
        }

        public static string ActionIdToGesture(string id, string fallback = null) {
            AnAction action = ActionManager.Instance.GetAction(id);
            if (action == null) {
                return fallback;
            }

            GroupedShortcut shortcut = WPFShortcutManager.Instance.FindFirstShortcutByAction(id);
            return shortcut == null ? fallback : shortcut.Shortcut.ToString();
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}