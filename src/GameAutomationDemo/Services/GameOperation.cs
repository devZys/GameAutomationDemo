using Dm;
using GameAutomationDemo.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameAutomationDemo.Services
{
    public class GameOperation
    {
        private Process _process;
        private dmsoft _dm;
        private string _gameName;

        public GameOperation(Process process, string gameName = "qjzjz")
        {
            _process = process;
            _gameName = gameName;
            Init();
        }

        private void Init()
        {
            var x = -1;
            var y = -1;
            for (var i = 0; i < 100; i++)
            {
                var dm = new dmsoft();
                dm.BindWindowByCustom(_process);
                //var p = _dmMain.FindPic(0, 0, 1000,1000, AppHelper.MapPath("/resources/pics/playerstart.bmp"), "000000", 1.0, 0, out var intX, out var intY);
                var p = dm.FindPic(0, 0, 1000, 1000, AppHelper.MapPath($"/resources/pics/{_gameName}.bmp"), "000000", 0.9, 0, out var intX, out var intY);
                x = (int)intX;
                y = (int)intY;
                if (x > 0 && y > 0)
                {
                    _dm = dm;
                    break;
                }
                else
                    Task.WaitAll(Task.Delay(TimeSpan.FromSeconds(1)));
            }

            if (_dm != null)
            {
                _dm.MoveTo(x, y);
                _dm.LeftClick();

            }
            else
                throw new Exception($"错误：模拟器启动太漫长，已超过100秒");
        }
    }
}
