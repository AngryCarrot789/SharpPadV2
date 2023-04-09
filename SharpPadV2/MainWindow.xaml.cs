using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpPadV2.Core;
using SharpPadV2.TextEditor;
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

            this.DataContext = new MainViewModel();
            ((MainViewModel) this.DataContext).CurrentTextEditor = new TextEditorViewModel(this.MainTextEditor);
        }
    }
}
