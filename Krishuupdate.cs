using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace Krishu_X_External
{
    public partial class Krishuupdate : Form
    {
        // GitHub Repository Details - CHANGE THESE
        private const string GITHUB_USERNAME = "YOUR_USERNAME";
        private const string GITHUB_REPO = "YOUR_REPO_NAME";
        private const string GITHUB_BRANCH = "main";
        private const string GITHUB_RAW_URL = $"https://raw.githubusercontent.com/{GITHUB_USERNAME}/{GITHUB_REPO}/{GITHUB_BRANCH}/";

        // File URLs
        private const string VERSION_FILE = "version.txt";
        private const string MAIN_FILE = "Main.cs";
        private const string DESIGNER_FILE = "Main.Designer.cs";
        private const string LOGIN_FILE = "Login.cs";
        private const string LOGIN_DESIGNER_FILE = "Login.Designer.cs";
        private const string MEMORY_FILE = "KrishuXmem.cs";
        private const string PROGRAM_FILE = "Program.cs";
        private const string PROJECT_FILE = "Krishu X External.csproj";

        private string currentVersion = "1.0.0";
        private string latestVersion = "";
        private bool updateAvailable = false;
        private List<string> filesToUpdate = new List<string>();

        public Krishuupdate()
        {
            InitializeComponent();
            this.Load += Krishuupdate_Load;
        }

        private async void Krishuupdate_Load(object sender, EventArgs e)
        {
            update.Text = "Checking for updates...";
            Chekstatuslbl.Text = "Connecting to GitHub...";
            guna2ProgressBar1.Value = 10;

            try
            {
                // Step 1: Check version
                await CheckVersion();
                guna2ProgressBar1.Value = 30;

                // Step 2: Check for updates
                await CheckForUpdates();
                guna2ProgressBar1.Value = 60;

                // Step 3: Download updates if available
                if (updateAvailable)
                {
                    await DownloadUpdates();
                    guna2ProgressBar1.Value = 85;

                    // Step 4: Apply updates
                    await ApplyUpdates();
                    guna2ProgressBar1.Value = 100;

                    update.Text = "Update Complete!";
                    Chekstatuslbl.Text = "Restarting application...";
                    await Task.Delay(1000);
                    RestartApplication();
                }
                else
                {
                    guna2ProgressBar1.Value = 100;
                    update.Text = "No updates available";
                    Chekstatuslbl.Text = "You have the latest version!";
                    await Task.Delay(1000);
                    OpenMainForm();
                }
            }
            catch (Exception ex)
            {
                update.Text = "Update Check Failed";
                Chekstatuslbl.Text = ex.Message;
                Chekstatuslbl.ForeColor = Color.Red;
                guna2ProgressBar1.Value = 0;
                await Task.Delay(2000);
                OpenMainForm();
            }
        }

        private async Task CheckVersion()
        {
            try
            {
                update.Text = "Checking version...";
                Chekstatuslbl.Text = "Fetching version info...";

                using (WebClient client = new WebClient())
                {
                    // Read current version from file or assembly
                    currentVersion = GetCurrentVersion();

                    // Get latest version from GitHub
                    string versionUrl = GITHUB_RAW_URL + VERSION_FILE;
                    latestVersion = await client.DownloadStringTaskAsync(versionUrl);
                    latestVersion = latestVersion.Trim();

                    Chekstatuslbl.Text = $"Current: v{currentVersion} | Latest: v{latestVersion}";
                }
            }
            catch (WebException)
            {
                throw new Exception("Unable to connect to GitHub. Please check your internet connection.");
            }
        }

        private string GetCurrentVersion()
        {
            // Read from embedded resource or file
            try
            {
                string versionPath = Path.Combine(Application.StartupPath, "version.txt");
                if (File.Exists(versionPath))
                {
                    return File.ReadAllText(versionPath).Trim();
                }
            }
            catch { }

            // Default version
            return "1.0.0";
        }

        private async Task CheckForUpdates()
        {
            update.Text = "Checking files...";
            Chekstatuslbl.Text = "Comparing files with GitHub...";

            if (string.IsNullOrEmpty(latestVersion) || latestVersion == currentVersion)
            {
                updateAvailable = false;
                return;
            }

            // Compare versions
            Version current = new Version(currentVersion);
            Version latest = new Version(latestVersion);

            if (latest > current)
            {
                updateAvailable = true;
                filesToUpdate = new List<string>
                {
                    MAIN_FILE,
                    DESIGNER_FILE,
                    LOGIN_FILE,
                    LOGIN_DESIGNER_FILE,
                    MEMORY_FILE,
                    PROGRAM_FILE,
                    PROJECT_FILE
                };

                Chekstatuslbl.Text = $"Update available: v{latestVersion}";
            }
            else
            {
                updateAvailable = false;
            }
        }

        private async Task DownloadUpdates()
        {
            update.Text = "Downloading updates...";
            Chekstatuslbl.Text = "Downloading files from GitHub...";

            // Create backup folder
            string backupPath = Path.Combine(Application.StartupPath, "Backup");
            if (!Directory.Exists(backupPath))
                Directory.CreateDirectory(backupPath);

            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += (s, e) =>
                {
                    int progress = 60 + (e.ProgressPercentage / 2);
                    if (progress > 95) progress = 95;
                    guna2ProgressBar1.Value = progress;
                };

                int totalFiles = filesToUpdate.Count;
                int downloaded = 0;

                foreach (string file in filesToUpdate)
                {
                    try
                    {
                        string fileUrl = GITHUB_RAW_URL + file;
                        string tempPath = Path.Combine(Application.StartupPath, "temp_" + file);
                        string backupFile = Path.Combine(backupPath, file);

                        // Backup existing file
                        string originalPath = Path.Combine(Application.StartupPath, file);
                        if (File.Exists(originalPath))
                        {
                            File.Copy(originalPath, backupFile, true);
                        }

                        // Download new file
                        await client.DownloadFileTaskAsync(fileUrl, tempPath);

                        // Verify download
                        if (new FileInfo(tempPath).Length > 0)
                        {
                            // Replace original with new file
                            if (File.Exists(originalPath))
                                File.Delete(originalPath);
                            File.Move(tempPath, originalPath);
                        }

                        downloaded++;
                        Chekstatuslbl.Text = $"Downloaded {downloaded}/{totalFiles}: {file}";
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue with other files
                        Console.WriteLine($"Error downloading {file}: {ex.Message}");
                    }
                }
            }

            // Download version file
            try
            {
                using (WebClient client = new WebClient())
                {
                    string versionUrl = GITHUB_RAW_URL + VERSION_FILE;
                    string versionContent = await client.DownloadStringTaskAsync(versionUrl);
                    string versionPath = Path.Combine(Application.StartupPath, "version.txt");
                    File.WriteAllText(versionPath, versionContent.Trim());
                }
            }
            catch { }
        }

        private async Task ApplyUpdates()
        {
            update.Text = "Applying updates...";
            Chekstatuslbl.Text = "Finalizing update...";

            // Update the current version
            currentVersion = latestVersion;

            // Save version to file
            string versionPath = Path.Combine(Application.StartupPath, "version.txt");
            File.WriteAllText(versionPath, latestVersion);

            await Task.Delay(500);
        }

        private void RestartApplication()
        {
            // Create a batch file to restart the app after a delay
            string batchPath = Path.Combine(Application.StartupPath, "restart.bat");
            string exePath = Application.ExecutablePath;

            StringBuilder batch = new StringBuilder();
            batch.AppendLine("@echo off");
            batch.AppendLine("timeout /t 2 /nobreak > nul");
            batch.AppendLine($"start \"\" \"{exePath}\"");
            batch.AppendLine("del \"%~f0\"");

            File.WriteAllText(batchPath, batch.ToString());

            // Start the batch file
            Process.Start(new ProcessStartInfo
            {
                FileName = batchPath,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });

            // Close current instance
            Application.Exit();
        }

        private void OpenMainForm()
        {
            this.Hide();
            var main = new Main();
            main.Show();
        }
    }
}