﻿using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TorrentMonitorUI
{
    /// <summary>
    /// ListView sublass that removes the dotted outline around focused items.
    /// Based on https://stackoverflow.com/a/15768802
    /// </summary>
    partial class NoFocusOutlineListView : ListView
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_CHANGEUISTATE = 0x127;
        private const int UIS_SET = 1;
        private const int UISF_HIDEFOCUS = 0x1;

        public NoFocusOutlineListView()
        {
            // Removes the ugly dotted line around focused item
            SendMessage(this.Handle, WM_CHANGEUISTATE, MakeLong(UIS_SET, UISF_HIDEFOCUS), 0);
        }

        private int MakeLong(int wLow, int wHigh)
        {
            int low = (int)IntLoWord(wLow);
            short high = IntLoWord(wHigh);
            int product = 0x10000 * (int)high;
            int mkLong = (int)(low | product);
            return mkLong;
        }

        private short IntLoWord(int word)
        {
            return (short)(word & short.MaxValue);
        }
    }
}
