using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RectifyUI
{
    public static class ControlExtensions
    {
        #region Redraw Suspend/Resume
        // From: http://stackoverflow.com/a/778133/779521

        [DllImport("user32.dll", EntryPoint = "SendMessageA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        private const int WM_SETREDRAW = 0xB;

        /// <summary>
        /// Disable window drawing via WM_SETREDRAW
        /// </summary>
        /// <param name="target"></param>
        public static void SuspendDrawing(this Control target)
        {
            SendMessage(target.Handle, WM_SETREDRAW, 0, 0);
        }

        /// <summary>
        /// Resume window drawing via WM_SETREDRAW
        /// </summary>
        public static void ResumeDrawing(this Control target) { ResumeDrawing(target, true); }

        /// <summary>
        /// Resume window drawing via WM_SETREDRAW
        /// </summary>
        public static void ResumeDrawing(this Control target, bool redraw)
        {
            SendMessage(target.Handle, WM_SETREDRAW, 1, 0);

            if (redraw)
            {
                target.Refresh();
            }
        }

        #endregion
    }
}