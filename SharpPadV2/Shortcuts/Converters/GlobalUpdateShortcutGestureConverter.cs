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
    public class ShortcutGestureConverter : SpecialValueConverter<ShortcutGestureConverter>, IValueConverter, INotifyPropertyChanged {
        public static ShortcutGestureConverter Instance { get; } = new ShortcutGestureConverter();

        public string NoSuchShortcutText { get; set; } = null;

        public int Version { get; private set; }

        public static void BroadcastChange() {
            Instance.Version++;
            Instance.OnPropertyChanged(nameof(Version));
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(parameter is string path)) {
                path = (value as string) ?? throw new Exception("Value and Parameter are not a shortcut string: " + parameter);
            }

            return PathToGesture(path, this.NoSuchShortcutText, out string gesture) ? gesture : DependencyProperty.UnsetValue;
        }

        public static bool PathToGesture(string path, string fallback, out string gesture) {
            GroupedShortcut shortcut = WPFShortcutManager.Instance.FindShortcutByPath(path);
            if (shortcut == null) {
                return (gesture = fallback) != null;
            }

            gesture = shortcut.Shortcut.ToString();
            return true;
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