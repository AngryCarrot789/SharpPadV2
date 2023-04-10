using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using SharpPadV2.Core.Actions;

namespace SharpPadV2.Views {
    /// <summary>
    /// An extended window which adds support for a few of the things in the dark theme I made (e.g. Titlebar brush)
    /// </summary>
    public class WindowEx : Window {
        public static readonly DependencyProperty TitlebarBrushProperty = DependencyProperty.Register("TitlebarBrush", typeof(Brush), typeof(WindowEx), new PropertyMetadata());
        public static readonly DependencyProperty CanCloseWithActionProperty = DependencyProperty.Register("CanCloseWithAction", typeof(bool), typeof(WindowEx), new PropertyMetadata(true));

        [Category("Brush")]
        public Brush TitlebarBrush {
            get => (Brush) this.GetValue(TitlebarBrushProperty);
            set => this.SetValue(TitlebarBrushProperty, value);
        }

        public bool CanCloseWithAction {
            get => (bool) this.GetValue(CanCloseWithActionProperty);
            set => this.SetValue(CanCloseWithActionProperty, value);
        }

        public WindowEx() {

        }

        static WindowEx() {
            ActionManager.Instance.Register("actions.views.CloseViewAction", new CloseViewAction());
        }

        private class CloseViewAction : AnAction {
            public CloseViewAction() : base(null, null) {

            }

            public override Task<bool> Execute(AnActionEventArgs e) {
                if (e.DataContext.GetContext<WindowEx>() is WindowEx w && w.CanCloseWithAction) {
                    w.Close();
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
        }
    }
}