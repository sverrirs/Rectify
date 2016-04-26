using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;


namespace RectifyUI
{
    public static class Win32
    {
        /* [DllImport("shell32.dll", SetLastError = true)]
         private static extern int SHMultiFileProperties(System.Windows.Forms.IDataObject pdtobj, int flags);

         public static void OpenFileProperties2(string[] filePaths)
         {
             //http://stackoverflow.com/a/1281485/779521

             if (filePaths == null || filePaths.Length < 0)
                 return;

             var pdtobj = new DataObject();

             var coll = new StringCollection();
             coll.AddRange(filePaths);

             pdtobj.SetFileDropList(coll);
             if (SHMultiFileProperties(pdtobj, 0) != 0)
                 throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not open system file property window due to an error.");
         }*/

        private enum ShellVerbs
        {
            /// <summary>
            /// Displays the file or folder's properties.
            /// </summary>
            properties,
            /// <summary>
            /// Launches an editor and opens the document for editing. 
            /// If lpFile is not a document file, the function will fail.
            /// </summary>
            edit,
            /// <summary>
            /// Explores the folder specified by lpFile.
            /// </summary>
            explore,
            /// <summary>
            /// Initiates a search starting from the specified directory.
            /// </summary>
            find,
            /// <summary>
            /// Opens the file specified by the lpFile parameter. 
            /// The file can be an executable file, a document file, or a folder.
            /// </summary>
            open,
            /// <summary>
            /// Prints the document file specified by lpFile. 
            /// If lpFile is not a document file, the function will fail.
            /// </summary>
            print
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        private const int SW_SHOW = 5;
        private const uint SEE_MASK_INVOKEIDLIST = 12;

        private static void ExecuteShellVerb(string filePath, ShellVerbs verb)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
            info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(info);
            info.lpVerb = verb.ToString().ToLower(); //"properties";
            info.lpFile = filePath;
            info.nShow = SW_SHOW;
            info.fMask = SEE_MASK_INVOKEIDLIST;
            ShellExecuteEx(ref info);
        }

        public static void OpenFileProperties(string filePath)
        {
            ExecuteShellVerb(filePath, ShellVerbs.properties);
        }

        public static void OpenFileLocation(string filePath)
        {
            ExecuteShellVerb(filePath, ShellVerbs.explore);
        }
    }
}
