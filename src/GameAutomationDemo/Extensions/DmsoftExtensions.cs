using Dm;
using GameAutomationDemo.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameAutomationDemo
{
    public static class DmsoftExtensions
    {
        public static int BindWindowByCustom(this dmsoft dm, int hwnd)
        {
            return dm.BindWindow(hwnd, "dx2", "windows", "windows", 1);
        }

        public static int BindWindowByCustom(this dmsoft dm, Process process)
        {
            return dm.BindWindowByCustom(process.GetMainWindowHwnd());
        }
    }
}
