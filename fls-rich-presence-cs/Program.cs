using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using DiscordRPC;
using DiscordRPC.Logging;
using DiscordRPC.Message;

namespace fls_rich_presence_cs
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetProcessIdOfThread(IntPtr handle);

        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private static Tray tray;
        private static DiscordRpcClient client;
        private static FileStream logFile;
        private static StreamWriter sw;
        private static RichPresence presence = new RichPresence()
        {
            Details = "Editing:",
            State = "",
            Timestamps = new Timestamps(),
            Assets = new Assets()
            {
                LargeImageKey = "fl_icon",
                LargeImageText = "FL Studio",
            }
		};

        static void Init()
        {
            logFile = new FileStream(Directory.GetCurrentDirectory() + "\\log.txt", FileMode.Create);
            sw = new StreamWriter(logFile);
            sw.AutoFlush = true;
            sw.WriteLine("Log: " + DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'"));
            Console.SetOut(sw);
            Console.SetError(sw);

            client = new DiscordRpcClient("398479706095091733", true, -1)
            {
                Logger = new DiscordRPC.Logging.ConsoleLogger() { Level = LogLevel.Info }
            };

            client.OnReady += OnReady;

            presence.Timestamps.Start = DateTime.UtcNow;
            client.Initialize();
        }

        static void Main(string[] args)
        {
            Init();

            Thread trayThread = new Thread(
                delegate ()
                {
                    tray = new Tray();
                    tray.SetExternalFunc(Exit, 5); //HACK
                    Application.Run();
                });
            trayThread.Start();

            MainLoop();
            Exit();
            Application.Exit();
        }

        static void MainLoop()
        {
            client.Invoke();
            Thread.Sleep(7000);

            while (client != null && !client.Disposed)
            {
                string winTitle;
                if (client != null) 
                {
                    winTitle = GetFLTitle();
                    if (winTitle != null)
                    {
                        UpdatePresence(winTitle);
                        Thread.Sleep(15000);
                    }
                    else
                    {
                        UpdatePresence(null);
                    }
                }
            }
        }

        static void OnReady(object sender, ReadyMessage args)
        {
            Console.WriteLine("On Ready. RPC Version: {0}", args.Version);
            
        }

        static void UpdatePresence(string title)
        {
            if (!tray.IsPresenceActive || title == null)
                client.SetPresence(null);
            else
            {
                string[] splitTitle = title.Split(' ');
                string version = "FL Studio " + splitTitle[splitTitle.Length - 1];
                string updateTitle = "";
                if (title == version)
                {
                    updateTitle = "Unsaved project";
                }
                else
                {
                    updateTitle = Regex.Match(title, ".+?(?= - FL Studio [0-9]?[0-9]$)").Value;
                }
                if (updateTitle != presence.State)
                {
                    presence.Timestamps.Start = DateTime.UtcNow;
                }
                presence.Assets.LargeImageText = version;
                presence.State = updateTitle;
                client.SetPresence(presence);
            } 
        }

        static string GetFLTitle()
        {
            string processName = null;
            Process procFL;

            Process[] process64 = Process.GetProcessesByName("FL64");
            if (process64.Length == 0)
            {
                Process[] process32 = Process.GetProcessesByName("FL");
                if (process32.Length == 0)
                {
                    return null;
                }

                procFL = process32[0];
            }
            else
            {
                procFL = process64[0];
            }

            uint threadID = 0;

            EnumWindows(delegate (IntPtr wnd, IntPtr param)
            {
                threadID = GetWindowThreadProcessId(wnd, IntPtr.Zero);

                StringBuilder className = new StringBuilder(256);
                int cint = GetClassName(wnd, className, 256);
                if (cint == 0)
                    return false;

                IntPtr thr = OpenThread(ThreadAccess.QUERY_INFORMATION, false, threadID);
                uint procID = GetProcessIdOfThread(thr);

                if (procID == procFL.Id && className.ToString() == "TFruityLoopsMainForm")
                {
                    int textLength = GetWindowTextLength(wnd);
                    StringBuilder outText = new StringBuilder(textLength + 1);
                    GetWindowText(wnd, outText, textLength + 1);
                    processName = outText.ToString();
                    return false;
                }

                return true;
            }, IntPtr.Zero);


            return processName;
        }

        static void Exit()
        {
            client.Dispose();
            tray.Dispose();
            Console.WriteLine("RPC closed.");
            sw.Close();
            logFile.Dispose();
        }
    }
}
