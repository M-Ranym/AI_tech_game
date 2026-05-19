using System.Diagnostics;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
namespace com.horizon.Utilities
{
    public class FileOpener
    {
        public static void OpenFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                UnityEngine.Debug.LogError("File path is null or empty");
                return;
            }

            string absolutePath = Path.IsPathRooted(filePath) ? filePath : Path.Combine(Application.dataPath, filePath);

            if (!File.Exists(absolutePath))
            {
                UnityEngine.Debug.LogError($"File not found: {absolutePath}");
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = absolutePath,
                    UseShellExecute = true
                });
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Error opening file: {e.Message}");
            }
        }
    }
}
#endif