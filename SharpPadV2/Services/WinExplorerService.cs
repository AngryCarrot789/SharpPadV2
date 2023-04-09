using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using SharpPadV2.Core.Services;

namespace SharpPadV2.Services {
    public class WinExplorerService : IExplorerService {
        public void OpenFileInExplorer(string filePath) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && File.Exists(filePath)) {
                Process.Start("explorer.exe", $"/select, \"{filePath.Replace('/', '\\')}\"");
            }
        }
    }
}