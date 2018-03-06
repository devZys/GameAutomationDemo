using Dm;
using GameAutomationDemo.Helpers;
using GameAutomationDemo.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameAutomationDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            AutoRegCom();
        }

        const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        const int PROCESS_VM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;

        #region dllimport

        [DllImport("kernel32.dll")]
        public static extern int VirtualAllocEx(IntPtr hwnd, int lpaddress, int size, int type, int tect);
        [DllImport("kernel32.dll")]
        public static extern int WriteProcessMemory(IntPtr hwnd, int baseaddress, string buffer, int nsize, int filewriten);
        [DllImport("kernel32.dll")]
        public static extern int GetProcAddress(int hwnd, string lpname);
        [DllImport("kernel32.dll")]
        public static extern int GetModuleHandleA(string name);
        [DllImport("kernel32.dll")]
        public static extern int CreateRemoteThread(IntPtr hwnd, int attrib, int size, int address, int par, int flags, int threadid);
        [DllImport("kernel32.dll")]
        public static extern int OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("User32.dll", EntryPoint = "FindWindowEx")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);

        [DllImport("KERNEL32.DLL ")]
        public static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processid);
        [DllImport("KERNEL32.DLL ")]
        public static extern int CloseHandle(IntPtr handle);
        //[DllImport("KERNEL32.DLL ")]
        //public static extern int Process32First(IntPtr handle, ref ProcessEntry32 pe);
        //[DllImport("KERNEL32.DLL ")]
        //public static extern int Process32Next(IntPtr handle, ref ProcessEntry32 pe);

        //[DllImport("DmReg.dll")]
        //public static extern long SetDllPathW(string path, long mode);

        //[DllImport("DmReg.dll")]
        //public static extern long SetDllPathA(string path, long mode);

        #endregion

        //[DllImport("user32.dll")]

        //public static extern IntPtr FindWindow(string lpClassName,string lpWindowName);

        private static dmsoft _dmMain;

        private static List<Thread> _threads = new List<Thread>();

        private void startbtn_Click(object sender, EventArgs e)
        {
            try
            {
                ExitAllPlayer();

                Task.WaitAll(Task.Delay(TimeSpan.FromSeconds(1)));
                var processes = StartPlayersByConsole();
                var l = processes.Length;
                for (var i = 0; i < l; i++)
                {
                    var process = processes[i];
                    var index = i;
                    var thread = new Thread(() =>
                    {
                        var opera = new GameOperator(process, index);
                    });

                    thread.Start();
                    _threads.Add(thread);
                }


            }
            catch (Exception ex)
            {
                ExitAllPlayer();
                MessageBox.Show(ex.Message);
                LogHelper.LogError(ex);
            }
        }

        private Process[] StartPlayersByConsole()
        {
            var path = GetDnmultiplayerPath();
            var console = $"{Path.Combine(Directory.GetParent(AppSettings.Player.Path).FullName, $"ldconsole.exe")}";
            var strOutput = AppHelper.ExecuteWindowsCommond($"{console} list{Environment.NewLine}");
            var arr = strOutput.Split(Environment.NewLine.ToArray()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            for (var i = 0; i < arr.Count; i++)
            {
                AppHelper.ExecuteWindowsCommond($"{console} modify --index {i} --resolution 800,600,160");
                AppHelper.ExecuteWindowsCommond($"{console} launch --index {i}");
                Task.WaitAll(Task.Delay(TimeSpan.FromSeconds(AppSettings.Player.StartInterval)));
            }

            var processes = Process.GetProcesses().Where(x => x.ProcessName == "dnplayer").ToArray();
            for (var i = 0; i < 30; i++)
            {
                if (processes.Length == arr.Count)
                    break;
                else
                {
                    AppHelper.WaitSeconds(1);
                    processes = Process.GetProcesses().Where(x => x.ProcessName == "dnplayer").ToArray();
                }
            }

            if (processes.Length < arr.Count)
                throw new Exception($"模拟器全部启动太过缓慢，已等待超过30秒");

            return processes;
        }

        private Process[] StartPlayers()
        {
            var list = new List<Process>();
            _dmMain = new dmsoft();
            //var x = dmMain.Reg("111", "3.1254");
            var mailProcess = StartDnmultiplayerProcess();
            //var hProcess = (IntPtr)OpenProcess(PROCESS_ALL_ACCESS, false, playerProcess.Id);
            var x111 = mailProcess.MainWindowHandle.ToInt32();
            var bind = _dmMain.BindWindowByCustom(mailProcess);
            if (bind == 0)
                throw new Exception("错误：大漠绑定雷电多开器窗口失败");

            const int topX = 0;
            var topY = 0;
            const int maxX = 100000;
            const int maxY = 100000;
            int index = 0;
            while (true)
            {
                var p = _dmMain.FindPic(topX, topY, maxX, maxY, AppHelper.MapPath("/resources/pics/playerstart.bmp"), "000000", 1.0, 0, out var intX, out var intY);
                var x = (int)intX;
                var y = (int)intY;
                if (x > 0 && y > 0)
                {
                    _dmMain.MoveTo(x + 10, y + 10);
                    _dmMain.LeftClick();
                    //await Task.Delay(TimeSpan.FromSeconds(AppSettings.PlayerStartDelay));
                    var process = GetDnplayer(index);
                    if (process == null)
                        throw new Exception("错误：未成功启动雷电模拟器");
                    list.Add(process);

                    topY = y + 2;
                    Task.WaitAll(Task.Delay(TimeSpan.FromSeconds(AppSettings.Player.StartInterval)));
                }
                else
                    break;
            }

            return list.ToArray();
        }

        private Process GetDnplayer(int index)
        {
            Process result = null;
            for (int i = 0; i < 3; i++)
            {
                var processes = Process.GetProcesses().Where(x => x.ProcessName.ToLower() == "dnplayer").ToArray(); ;
                if (processes == null || processes.Length == 0)
                    Task.WaitAll(Task.Delay(10));
                else if (processes.Length > index)
                {
                    result = processes[index];
                    break;
                }
            }

            return result;
        }

        private Process StartDnmultiplayerProcess()
        {
            Process[] pnames = Process.GetProcesses(); //取得所有进程
            var process = pnames.SingleOrDefault(x => x.ProcessName.ToLower() == "dnmultiplayer");
            if (process == null)
            {
                var path = GetDnmultiplayerPath();
                if (string.IsNullOrWhiteSpace(path))
                    MessageBox.Show("请先安装雷电多开器，并发送到桌面快捷方式");

                process = Process.Start(path);
            }

            return process;
        }

        /// <summary>
        /// 获取多开器路径
        /// </summary>
        /// <returns></returns>
        private string GetDnmultiplayerPath()
        {
            var playerPath = AppSettings.Player.Path;
            if (!File.Exists(playerPath))
            {
                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string[] files = Directory.GetFiles(desktop, "*雷电多开器*", SearchOption.TopDirectoryOnly);
                files = files.Length == 0 ? Directory.GetFiles(desktop, "*dnmultiplayer*", SearchOption.TopDirectoryOnly) : files;
                if (files.Length > 0)
                {
                    playerPath = files[0];
                    if (playerPath.ToLower().EndsWith(".lnk"))
                    {
                        IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                        IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(playerPath);
                        playerPath = shortcut.TargetPath;
                    }
                    AppSettings.Player.Path = playerPath;
                }
            }

            return File.Exists(playerPath) ? playerPath : string.Empty;
        }

        private bool AutoRegCom()
        {
            string strCmd = $"regsvr32 {AppHelper.MapPath("/refs/dm.dll")} /s";
            string rInfo;
            try
            {
                Process myProcess = new Process();
                ProcessStartInfo myProcessStartInfo = new ProcessStartInfo("cmd.exe");
                myProcessStartInfo.UseShellExecute = false;
                myProcessStartInfo.CreateNoWindow = true;
                myProcessStartInfo.RedirectStandardOutput = true;
                myProcess.StartInfo = myProcessStartInfo;
                myProcessStartInfo.Arguments = "/c " + strCmd;
                myProcess.Start();
                StreamReader myStreamReader = myProcess.StandardOutput;
                rInfo = myStreamReader.ReadToEnd();
                myProcess.Close();

                AppHelper.ExecuteWindowsCommond($"regsvr32 {AppHelper.MapPath("/refs/DmReg.dll.dll")} /s");
                //SetDllPathW(AppHelper.MapPath("/refs/dm.dll"), 0);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("大漠插件调用出问题:" + ex.Message);
                //return ex.Message;  
                return false;
            }
        }

        private void TestHook(Process process)
        {
            int ok1;
            //int ok2;
            //int hwnd;
            int baseaddress;
            int temp = 0;
            int hack;
            int yan;
            string dllname;
            dllname = "D://C#_TEST//hookapi//Debug//native.dll";//dll路径
            int dlllength;
            dlllength = dllname.Length + 1;
            baseaddress = VirtualAllocEx(process.Handle, 0, dlllength, 4096, 4);
            if (baseaddress == 0) //返回0则操作失败，下面都是
                throw new Exception("\n申请内存空间失败！");

            ok1 = WriteProcessMemory(process.Handle, baseaddress, dllname, dlllength, temp);
            if (ok1 == 0)
                throw new Exception("\n写入内存空间失败！");

            hack = GetProcAddress(GetModuleHandleA("Kernel32"), "LoadLibraryA"); //取得loadlibarary在kernek32.dll地址
            if (hack == 0)
                throw new Exception("\n无法取得函数的入口点！");
            yan = CreateRemoteThread(process.Handle, 0, 0, hack, baseaddress, 0, temp); //创建远程线程。
            if (yan == 0)
                throw new Exception("\n创建远程线程失败！");
        }

        private void ExitAllPlayer()
        {
            var tl = _threads.Count;
            for (var i = 0; i < tl; i++)
            {
                _threads[0].Abort();
                _threads.RemoveAt(0);
            }

            var pros = Process.GetProcesses().Where(x => x.ProcessName == "dnplayer" || x.ProcessName == "dnmultiplayer").ToList();
            foreach (var pro in pros)
            {
                pro.Kill();
            }
        }
    }
}
