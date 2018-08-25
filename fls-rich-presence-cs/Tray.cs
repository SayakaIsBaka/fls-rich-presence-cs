using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Win32;
using System.IO;

namespace fls_rich_presence_cs
{
    class Tray
    {
        public NotifyIcon NotifyTray { get; private set; }
        public bool IsPresenceActive { get; private set; }
        private bool RunOnStartup;

        public Tray()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            Object tmp = rk.GetValue("FLS Rich Presence");
            if (tmp == null)
                RunOnStartup = false;
            else
                RunOnStartup = true;
            NotifyTray = CreateTray();
            IsPresenceActive = true;
        }

        private NotifyIcon CreateTray()
        {
            NotifyIcon tray = new NotifyIcon();
            tray.Text = "FL Studio Rich Presence: Enabled";

            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Image-Line\\Shared\\Paths");
                if (key != null)
                {
                    Object obj = key.GetValue("FL Studio");
                    if (obj != null)
                    {
                        tray.Icon = Icon.ExtractAssociatedIcon(obj as String);
                    }
                    else
                        throw new KeyNotFoundException("FL registry key not found");
                }
                else
                    throw new KeyNotFoundException("FL registry key not found");
            }
            catch (Exception)
            {
                tray.Icon = new Icon(SystemIcons.Application, 40, 40);
            }

            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add("FL Studio not detected...");
            menu.MenuItems[0].Enabled = false;
            menu.MenuItems.Add("-");
            menu.MenuItems.Add("Disable Rich Presence", new EventHandler(SwitchPresence));
            menu.MenuItems.Add("Run on startup", new EventHandler(SetStartup));
            menu.MenuItems[3].Checked = RunOnStartup;
            menu.MenuItems.Add("Project repository (GitHub)", new EventHandler(ProjectURL));
            menu.MenuItems.Add("-");
            menu.MenuItems.Add("View log", new EventHandler(ViewLog));
            menu.MenuItems.Add("-");
            menu.MenuItems.Add("Exit", new EventHandler(Exit));
            tray.ContextMenu = menu;
            tray.Visible = true;

            return tray;
        }

        private void ProjectURL(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/SayakaIsBaka/fls-rich-presence-cs");
        }

        public void SetExternalFunc(Action func, int menuID)
        {
            NotifyTray.ContextMenu.MenuItems[menuID].Click += delegate (object sender, EventArgs e) { func(); };
        }

        private void ViewLog(object sender, EventArgs e)
        {
            Process.Start(Directory.GetCurrentDirectory() + "\\log.txt");
        }

        public void Dispose()
        {
            NotifyTray.Dispose();
            IsPresenceActive = false;
        }

        public void SwitchPresence(object sender, EventArgs e)
        {
            IsPresenceActive = !IsPresenceActive;
            if (IsPresenceActive)
            {
                NotifyTray.ContextMenu.MenuItems[2].Text = "Disable Rich Presence";
                NotifyTray.Text = "FL Studio Rich Presence: Enabled";
            }
            else
            {
                NotifyTray.ContextMenu.MenuItems[2].Text = "Enable Rich Presence";
                NotifyTray.Text = "FL Studio Rich Presence: Disabled";
            }  
        }
        private void SetStartup(object sender, EventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (!RunOnStartup)
            {
                rk.SetValue("FLS Rich Presence", Application.ExecutablePath);
                RunOnStartup = true;
            }
            else
            {
                rk.DeleteValue("FLS Rich Presence", false);
                RunOnStartup = false;
            }

            NotifyTray.ContextMenu.MenuItems[3].Checked = RunOnStartup;
        }

        private void Exit(object sender, EventArgs e)
        {
            Program.Exit();
        }

        public void Detected(bool isDetected)
        {
            if (isDetected)
            {
                NotifyTray.ContextMenu.MenuItems[0].Text = "FL Studio detected!";
            }
            else
            {
                NotifyTray.ContextMenu.MenuItems[0].Text = "FL Studio not detected...";
            }
        }
    }
}
