using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GameAutomationDemo.Helpers
{
    public static class LogHelper
    {
        static ActionBlock<string> _action;

        static string _path;

        static LogHelper()
        {
            if (_action == null)
            {
                _path = AppHelper.MapPath($"/logs/{DateTime.Now.ToString("yyyy-MM-dd")}.txt");
                _action = new ActionBlock<string>((s) =>
                {
                    File.AppendAllText(_path,$"{Environment.NewLine}========================{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}==============================={Environment.NewLine}{s}");
                });
            }
        }

        public static void LogInfo(string msg)
        {

        }

        public static void LogError(Exception ex, string remark = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            sb.Append("======================================================================================");
            sb.Append(Environment.NewLine);
            if (!string.IsNullOrWhiteSpace(remark))
            {
                sb.Append($"异常备注：");
                sb.Append(remark);
                sb.Append(Environment.NewLine);
            }
            sb.Append("Message:");
            sb.Append(ex.Message);
            sb.Append(Environment.NewLine);
            sb.Append("Source:");
            sb.Append(ex.Source);
            sb.Append(Environment.NewLine);
            sb.Append("StackTrace:");
            sb.Append(ex.StackTrace);

            _action.Post(sb.ToString());
        }

        private static void Log(string msg)
        {
            Console.WriteLine(msg);
            _action.Post(msg);
        }
    }
}
