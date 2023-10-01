using System.Runtime.InteropServices;
using System.Text;

namespace OpenApi.Tools.Core
{
    /// <summary>
    /// Path helper
    /// </summary>
    public static class PathHelper
    {
        const int MAX_PATH = 255;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetShortPathNameW", SetLastError = true)]
        static extern int GetShortPathName(
            [MarshalAs(UnmanagedType.LPTStr)]
            string pathName,
            [MarshalAs(UnmanagedType.LPTStr)]
            StringBuilder shortName, int cbShortName);

        /// <summary>
        /// Gets short path
        /// </summary>
        /// <param name="path">Long path</param>
        /// <returns>Short path</returns>
        public static string GetShortPath(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var shortPath = new StringBuilder(MAX_PATH);
                GetShortPathName($"\\\\?\\{path}", shortPath, MAX_PATH);
                var shortenedPath = shortPath.ToString();
                return shortenedPath.Replace("\\\\?\\", string.Empty);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return path;
            }
            else
            {
                throw new NotSupportedException($"Calling {nameof(GetShortPath)} is not supported on the system.");
            }
        }
    }
}