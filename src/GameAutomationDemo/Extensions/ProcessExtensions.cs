using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameAutomationDemo
{
    public static class ProcessExtensions
    {
        public static int GetMainWindowHwnd(this Process process, int timeout = 10)
        {
            var hwnd = process.MainWindowHandle.ToInt32();
            var t = timeout * 2;
            if (hwnd <= 0)
            {
                for (var i = 0; i < t; i++)
                {
                    if (hwnd > 0)
                        break;
                    else
                    {
                        Task.WaitAll(Task.Delay(TimeSpan.FromMilliseconds(500)));
                        hwnd = process.MainWindowHandle.ToInt32();
                    }
                }
            }
            if (hwnd <= 0)
                throw new Exception($"获取进程{process.ProcessName}主窗口句柄失败，{process.ProcessName}窗口初始化太缓慢，已超过{timeout}秒");

            return hwnd;
        }
    }
}
