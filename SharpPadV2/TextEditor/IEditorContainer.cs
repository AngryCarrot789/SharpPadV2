using System.Threading.Tasks;
using SharpPadV2.Core.TextEditor;

namespace SharpPadV2.TextEditor {
    public interface IEditorContainer {
        ITextEditor Editor { get; }

        Task<bool> CloseEditor(TextEditorViewModel editor);
    }
}