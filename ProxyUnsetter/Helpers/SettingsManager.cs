﻿using System;
using System.Windows.Forms;
using Microsoft.Win32;
using ProxyUnsetter.Properties;

namespace ProxyUnsetter.Helpers
{
    internal class SettingsManager
    {
        private readonly ProxyManager _proxyManager;
        public static event EventHandler SettingsChanged;
        public const string AppName = "ProxyUnsetter";

        public SettingsManager(ProxyManager proxyManager)
        {
            _proxyManager = proxyManager;
        }

        public bool GetLaunchAtWindowsStartupState()
        {
            var registryKey = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (registryKey == null)
            {
                MessageBox.Show(
                    @"Could not open registry key for the current user (SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run)",
                    @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return registryKey.GetValue(AppName) != null;
        }

        public static void ToggleCheckForNewReleaseWeekly(bool set, ReleaseChecker releaseChecker)
        {
            Settings.Default.CheckForNewReleaseWeekly = set;
            Settings.Default.Save();
            if (set)
                releaseChecker.Start();
            else
                releaseChecker.Stop();
        }

        public static void ToggleWindowsStartup(bool set)
        {
            var registryKey = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (registryKey == null)
            {
                MessageBox.Show(
                    @"Could not open registry key for the current user (SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run)",
                    @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (set)
                registryKey.SetValue("ProxyUnsetter", Application.ExecutablePath);
            else
                registryKey.DeleteValue("ProxyUnsetter", false);
        }

        public static void ToggleNotifyOfProxySet(bool set)
        {
            Settings.Default.NotifyOfProxySet = set;
            Settings.Default.Save();
        }

        public static void ToggleAutomaticProxyUnset(bool set)
        {
            Settings.Default.UnsetProxyAutomatically = set;
            Settings.Default.Save();
        }

        public static void ToggleUnsetOrFakePac(bool set)
        {
            Settings.Default.UnsetOrFakePac = set;
            Settings.Default.Save();
        }

        public static string[] GetIpAddressAndNetmaskFromSetting(string setting)
        {
            var split = setting.Split('/');
            return new[] {split[0], split[1]};
        }

        public void SetManuallySetProxy(string manualProxy)
        {
            Settings.Default.ManuallySetProxy = manualProxy;
            Settings.Default.Save();
            _proxyManager.ManuallySetProxyServer = manualProxy;
            SettingsChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}