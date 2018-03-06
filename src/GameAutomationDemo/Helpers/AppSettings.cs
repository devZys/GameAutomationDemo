using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameAutomationDemo.Helpers
{
    public static class AppSettings
    {
        public struct Player
        {
            public static string Path
            {
                get
                {
                    return AppHelper.GetAppSetting("player", "path");
                }
                set
                {
                    AppHelper.SetAppSetting("player", "path", value);
                }
            }

            public static int StartInterval
            {
                get
                {
                    int.TryParse(AppHelper.GetAppSetting("player", "startinterval"),out var x);
                    x = x <= 0 ? 1 : x;                        
                    return x;
                }
                set
                {
                    AppHelper.SetAppSetting("player", "path", value.ToString());
                }
            }
        }
    }
}
