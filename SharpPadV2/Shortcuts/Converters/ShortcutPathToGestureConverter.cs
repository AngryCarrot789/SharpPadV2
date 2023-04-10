using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Windows;
using System.Windows.Data;
using SharpPadV2.Converters;
using SharpPadV2.Core.Shortcuts.Managing;

namespace SharpPadV2.Shortcuts.Converters {
    public class GlobalUpdateShortcutGestureConverter : IValueConverter, INotifyPropertyChanged {
        public static GlobalUpdateShortcutGestureConverter Instance { get; } = new GlobalUpdateShortcutGestureConverter();

        public int Version { get; private set; }

        public static void BroadcastChange() {
            Instance.Version++;
            Instance.OnPropertyChanged(nameof(Version));
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(parameter is string path)) {
                path = (value as string) ?? throw new Exception("Value and Parameter are not a shortcut string: " + parameter);
            }

            return PathToGesture(path, null, out string gesture) ? gesture : DependencyProperty.UnsetValue;
        }

        public static bool PathToGesture(string path, string fallback, out string gesture) {
            GroupedShortcut shortcut = WPFShortcutManager.Instance.FindShortcutByPath(path);
            if (shortcut == null) {
                return (gesture = fallback) != null;
            }

            gesture = shortcut.Shortcut.ToString();
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}