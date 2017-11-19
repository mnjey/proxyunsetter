﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace ProxyUnsetter.Helpers
{
    internal class ReleaseChecker
    {
        private Timer _checkForNewReleaseTimer;

        public void Start()
        {
            _checkForNewReleaseTimer = new Timer { Interval = 7 * 24 * 60 * 1000 }; // week interval
            _checkForNewReleaseTimer.Tick += CheckForNewReleaseTimer_Tick;
            _checkForNewReleaseTimer.Start();
        }

        public void Stop()
        {
            _checkForNewReleaseTimer.Stop();
            _checkForNewReleaseTimer.Dispose();
        }

        public void CheckNow()
        {
            CheckForNewReleaseTimer_Tick(this, new ReleaseCheckEventArgs(true));
        }

        private async void CheckForNewReleaseTimer_Tick(object sender, EventArgs e)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("proxyunsetter", Application.ProductVersion));
            var latestReleaseJson = await
                httpClient.GetStringAsync("https://api.github.com/repos/tjeerdhans/proxyunsetter/releases/latest");

            var latestRelease = latestReleaseJson.FromJson<GitHubRelease>();
            if (latestRelease == null)
            {
                Program.SimpleLogLines.Add($"{DateTime.Now:g} Couldn't parse json response while checking for a new release at GitHub.");
                return;
            }

            var latestReleaseTagSplitted = latestRelease.tag_name.Split('.');
            var currentReleaseSplitted = Application.ProductVersion.Split('.');
            var latestMajor = int.Parse(latestReleaseTagSplitted[0]);
            var latestMinor = int.Parse(latestReleaseTagSplitted[1]);
            var currentMajor = int.Parse(currentReleaseSplitted[0]);
            var currentMinor = int.Parse(currentReleaseSplitted[1]);
            var newerAvailable = false;
            if (latestMajor > currentMajor)
            {
                newerAvailable = true;
            }
            else if (latestMajor == currentMajor && latestMinor > currentMinor)
            {
                newerAvailable = true;
            }

            if (newerAvailable)
            {
                if (MessageBox.Show(
                        $@"There is a new version available ({latestRelease.tag_name}, '{latestRelease.name}'). Click 'OK' to browse to the release at https://github.com/tjeerdhans/proxyunsetter/releases/latest. Download the latest executable and replace your current one with it.",
                        @"New version", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                {
                    System.Diagnostics.Process.Start("https://github.com/tjeerdhans/proxyunsetter/releases/latest");
                }
            }
            else if (e is ReleaseCheckEventArgs && ((ReleaseCheckEventArgs)e).DeliberateCheck)
            {
                MessageBox.Show(@"No new release available.", @"Proxy Unsetter", MessageBoxButtons.OK);
            }
        }
    }
}
