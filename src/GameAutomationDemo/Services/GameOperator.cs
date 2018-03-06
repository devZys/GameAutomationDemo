using Dm;
using GameAutomationDemo.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameAutomationDemo.Services
{
    public class GameOperator
    {
        //[DllImport("user32.dll")]
        //private static extern int GetWindowRect(IntPtr hwnd, out Rect lpRect);

        [DllImport("user32.dll", EntryPoint = "MoveWindow")]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        private Process _process;
        private dmsoft _dm;
        private int _playerIndex;

        public GameOperator(Process process, int playerIndex)
        {
            _process = process;
            _playerIndex = playerIndex;
            Init();
        }

        private void Init()
        {
            AppHelper.WaitMillSeconds(Convert.ToInt32(0.05 * (_playerIndex+1) * 1000));
            _dm = new dmsoft();
            _dm.MoveWindow(_process.GetMainWindowHwnd(), _playerIndex * 20, 0);

            var b = IsStared();
            if (!b)
            {
                for (var i = 0; i < 100; i++)
                {
                    Task.WaitAll(Task.Delay(TimeSpan.FromSeconds(1)));
                    b = IsStared();
                    if (!b)
                        continue;
                    var commond = $"{Path.Combine(Path.GetDirectoryName(AppSettings.Player.Path), "ldconsole.exe")} adb --index {_playerIndex} --command \"shell am start -n  com.longtugame.qjmu.longtu/com.guorangame.hiheros.MainActivity\"";
                    AppHelper.ExecuteWindowsCommond(commond);
                    _dm.BindWindowByCustom(_process);
                    break;
                }
            }

            if (!b)
                throw new Exception($"错误：模拟器启动太漫长，已超过100秒");

            StartOpera();
        }

        private bool IsStared()
        {
            var console = Path.Combine(Path.GetDirectoryName(AppSettings.Player.Path), "ldconsole.exe");
            var str = AppHelper.ExecuteWindowsCommond($"{console} list2");
            var arr = str.Split(Environment.NewLine.ToArray()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            return int.TryParse(arr[_playerIndex].Split(',')[4], out var i) && i > 0;
        }

        private void StartOpera()
        {
            var r = 0;
            object intX = null;
            object intY = null;
            while (r <= 0)
            {
                AppHelper.WaitSeconds(1);
                r = _dm.FindPic(0, 0, 800, 600, AppHelper.MapPath("/resources/pics/login.bmp"), "000000", 1.0, 0, out intX, out intY);
            }
            if (r > 0)
            {
                _dm.MoveTo((int)intX, (int)intY);
                _dm.LeftClick();
            }
        }
    }
}
