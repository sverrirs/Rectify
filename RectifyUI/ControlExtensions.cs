using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RectifyUI
{
    public static class ControlExtensions
    {
        #region Redraw Suspend/Resume
        // From: http://stackoverflow.com/a/778133/779521

        [DllImport("user32.dll", EntryPoint = "SendMessageW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hwnd, uint wMsg, IntPtr wParam, IntPtr lParam);
        private const uint WM_SETREDRAW = 0xB;

        /// <summary>
        /// Disable window drawing via WM_SETREDRAW
        /// </summary>
        /// <param name="target"></param>
        public static void SuspendDrawing(this Control target)
        {
            SendMessage(target.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
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
            SendMessage(target.Handle, WM_SETREDRAW, IntPtr.Zero + 1, IntPtr.Zero);

            if (redraw)
            {
                target.Refresh();
            }
        }

        #endregion
    }
}