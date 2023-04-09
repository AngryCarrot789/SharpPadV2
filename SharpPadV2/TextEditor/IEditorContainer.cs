using System.Threading.Tasks;
using System.Windows.Input;
using SharpPadV2.Core;

namespace SharpPadV2.TextEditor {
    public interface IEditorContainer {
        Task<bool> CloseEditor(TextEditorViewModel editor);
    }
}