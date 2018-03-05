using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameAutomationDemo.Helpers
{
    public static class AppHelper
    {
        private static INIClass _ini;

        static AppHelper()
        {
            if(_ini==null)
                _ini = new INIClass(MapPath("/appsettings.ini"));
        }

        public static string MapPath(string uri)
        {
            var path = Directory.GetCurrentDirectory();
#if DEBUG
            path = path.Replace(Path.Combine("bin", "Debug"), "");
#endif
            var arr = uri.Split('/').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            foreach (var s in arr)
            {
                path = Path.Combine(path, s);
            }

            return path;
        }

        public static string GetAppSetting(string section, string key)
        {            
            return _ini.IniReadValue(section, key);
        }

        public static void SetAppSetting(string section, string key, string val)
        {
            _ini.IniWriteValue(section, key, val);
        }

        public static int GetProcessHwnd(Process process)
        {
            var hw = process.MainWindowHandle.ToInt32();
            if (hw <= 0)
            {
                for (int i = 0; i < 20; i++)
                {
                    hw = process.MainWindowHandle.ToInt32();
                    if (hw <= 0)
                        Task.WaitAll(Task.Delay(TimeSpan.FromMilliseconds(500)));
                    else
                        break;
                }
            }
            if (hw <= 0)
                throw new Exception($"错误：绑定{process.ProcessName}窗口失败，{process.ProcessName}启动太过缓慢。");

            return hw;
        }
    }
}
