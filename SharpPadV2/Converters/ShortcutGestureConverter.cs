using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using SharpPadV2.Core.Shortcuts.Managing;
using SharpPadV2.Shortcuts;

namespace SharpPadV2.Converters {
    public class ShortcutGestureConverter : SpecialValueConverter<ShortcutGestureConverter>, IValueConverter, INotifyPropertyChanged {
        public string NoSuchShortcutText { get; set; } = null;

        public int Version { get; private set; }

        public static void BroadcastChange() {
            Instance.Version++;
            Instance.OnPropertyChanged(nameof(Version));
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(parameter is string path)) {
                throw new Exception("Parameter is not a shortcut string: " + parameter);
            }

            return PathToGesture(path, this.NoSuchShortcutText) ?? DependencyProperty.UnsetValue;
        }

        public static string PathToGesture(string path, string fallback = null) {
            GroupedShortcut shortcut = AppShortcutManager.Instance.FindShortcutByPath(path);
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