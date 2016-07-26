using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace OPTI_Experiment
{
    public static class WindowStyleHelper
    {
        [DllImport("user32.dll")]
        static extern uint GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        // Changes an attribute of the specified window. 
        static extern int SetWindowLong(IntPtr hwnd, int index, uint newStyle);

        [DllImport("user32.dll")]
        // Changes the size, position, and Z order of a child, pop-up, or top-level window.
        static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x,
            int y, int width, int height, uint flags);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam,
            IntPtr lParam);

        // Sets a new extended window style.
        const int GWL_EXSTYLE = -20;

        // The window has a double border; the window can, optionally, be created with a title bar by specifying the WS_CAPTION style in the dwStyle parameter.
        const int WS_EX_DLGMODALFRAME = 0x0001;

        // The title bar of the window includes a question mark.
        // WS_EX_CONTEXTHELP cannot be used with the WS_MAXIMIZEBOX or WS_MINIMIZEBOX styles.
        const uint WS_EX_CONTEXTHELP = 0x00000400;

        // The window has a minimize button.
        const uint WS_MINIMIZEBOX = 0x00020000;

        // The window has a maximize button.
        const uint WS_MAXIMIZEBOX = 0x00010000;

        // 크기는 변경하지 않고 위치만 이동한다. cx, cy 인수가 무시된다.
        const int SWP_NOSIZE = 0x0001;

        // 위치는 이동하지 않고 크기만 변경한다. X,Y인수가 무시된다.
        const int SWP_NOMOVE = 0x0002;

        // 현재의 Z순서를 그대로 유지한다. hWndInsertAfter 인수를 무시한다.
        const int SWP_NOZORDER = 0x0004;

        // SetWindowLong으로 경계선 스타일을 변경했을 경우 새 스타일을 적용한다. 이 플래그가 지정되면 크기가 변경되지 않아도 WM_NCCALCSIZE 메시지가 전달된다.
        const int SWP_FRAMECHANGED = 0x0020;

        const uint WM_SETICON = 0x0080;

        // Sets a new window style.
        const int GWL_STYLE = -16;

        // The window has a window menu on its title bar.
        const uint WS_SYSMENU = 0x80000;

        const int WM_SYSCOMMAND = 0x0112;

        const int SC_CONTEXTHELP = 0xF180;

        const int WM_MOUSEHOVER = 0x02A1;

        public static void RemoveIcon(Window window)
        {
            // Get this window's handle
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            // Change the extended window style to not show a window icon
            uint extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_DLGMODALFRAME);
            // Update the window's non-client area to reflect the changes
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE |
                SWP_NOZORDER | SWP_FRAMECHANGED);
        }

        public static void RemoveIconAndSysMenu(Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }
    }
}
