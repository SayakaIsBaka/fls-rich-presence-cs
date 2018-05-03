using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Win32;

namespace fls_rich_presence_cs
{
    class Tray
    {
        public NotifyIcon NotifyTray { get; private set; }
        public bool IsPresenceActive { get; private set; }

        public Tray()
        {
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
            menu.MenuItems.Add("Disable Rich Presence", new EventHandler(SwitchPresence));
            menu.MenuItems.Add("Project repository (GitHub)", new EventHandler(ProjectURL));
            menu.MenuItems.Add("-");
            menu.MenuItems.Add("Exit");
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
                NotifyTray.ContextMenu.MenuItems[0].Text = "Disable Rich Presence";
                NotifyTray.Text = "FL Studio Rich Presence: Enabled";
            }
            else
            {
                NotifyTray.ContextMenu.MenuItems[0].Text = "Enable Rich Presence";
                NotifyTray.Text = "FL Studio Rich Presence: Disabled";
            }  
        }
    }
}
