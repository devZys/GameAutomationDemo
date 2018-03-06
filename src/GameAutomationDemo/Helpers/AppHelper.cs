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
            if (_ini == null)
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

        public static string ExecuteWindowsCommond(string commond)
        {
            var result = string.Empty;
            Process myProcess = new Process();
            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo("cmd.exe");
            myProcessStartInfo.UseShellExecute = false;
            myProcessStartInfo.CreateNoWindow = true;
            myProcessStartInfo.RedirectStandardError = true;
            myProcessStartInfo.RedirectStandardInput = true;
            myProcessStartInfo.RedirectStandardOutput = true;
            myProcess.StartInfo = myProcessStartInfo;
            myProcessStartInfo.Arguments = $"/c {commond}";
            myProcess.Start();

            result = myProcess.StandardOutput.ReadToEnd();

            myProcess.StandardInput.WriteLine("exit");
            myProcess.WaitForExit();
            myProcess.Close();

            return result;
        }

        public static void WaitSeconds(int seconds)
        {
            Task.WaitAll(Task.Delay(TimeSpan.FromSeconds(seconds)));
        }
        
        public static void WaitMillSeconds(int total)
        {
            Task.WaitAll(Task.Delay(TimeSpan.FromMilliseconds(total)));
        }
    }
}
