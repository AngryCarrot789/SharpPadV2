using SharpPadV2.Core;
using SharpPadV2.Views;

namespace SharpPadV2 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowEx {
        public MainWindow() {
            this.InitializeComponent();
            IoC.BroadcastShortcutActivity = (x) => {
                // this.ShortcutIndicatorBlock.Text = x ?? "";
            };

            this.DataContext = new MainViewModel(this.MainTextEditor);
        }
    }
}
