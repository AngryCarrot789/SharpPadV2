using System.Threading.Tasks;

namespace SharpPadV2.Core.Views.Windows {
    public interface IWindow : IViewBase {
        void CloseWindow();

        Task CloseWindowAsync();
    }
}