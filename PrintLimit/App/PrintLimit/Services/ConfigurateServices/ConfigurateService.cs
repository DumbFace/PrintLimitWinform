using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Management.Automation;
using Serilog;

namespace PrintLimit.Services.ConfigurateServices
{
    public class ConfigurateService : IConfigurateService
    {
        public bool CopyShortcutToStartup()
        {
            bool result = false;
            var shell = new WshShell();
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string sourceFile = @"C:\Program Files (x86)\VinaAi\PrintManager\PrintLimit.exe - Shortcut";
            string shortcutPath = Path.Combine(startupFolder, Path.GetFileNameWithoutExtension(sourceFile) + ".lnk");
            var shortcut = shell.CreateShortcut(shortcutPath) as IWshShortcut;
            try
            {
                shortcut.TargetPath = sourceFile;
                shortcut.WorkingDirectory = startupFolder;
                shortcut.Save();
                result = true;
            }
            catch (IOException iox)
            {
                MessageBox.Show("Có lỗi trong quá copy file vào thư mục startup windows");
                result = false;
            }

            return result;
        }

        public bool EnableLogPrintService()
        {
            bool result = false;

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = "wevtutil set-log \"Microsoft-Windows-PrintService/Operational\" /enabled:true",
                Verb = "runas",  // Run as administrator
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            try
            {
                var process = Process.Start(startInfo);
                Log.Information("Enable Event Viewer");
            }
            catch (Exception ex)
            {
                Log.Information("Enable Event Viewer Error: " + ex.Message);
            }
            return result;
        }

        public bool PreventMultipleThreadStart()
        {
            string processName = Process.GetCurrentProcess().ProcessName;
            Process[] processes = Process.GetProcessesByName(processName);
            return (processes.Length > 1);
        }

        public bool RegisterForceSetCopyCount()
        {
            // Define the Office versions to check
            var officeVersions = new[] {
            new { Version = "12.0", Name = "Office 2007" },
            new { Version = "14.0", Name = "Office 2010" },
            new { Version = "15.0", Name = "Office 2013" },
            new { Version = "16.0", Name = "Office 2016/2019/2021" },
        };
            bool result = false;
            try
            {
                foreach (var version in officeVersions)
                {
                    string keyName = $"Software\\Microsoft\\Office\\{version.Version}\\Word\\Options";
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName, true))
                    {
                        if (key != null)
                        {
                            if (key.GetValue("ForceSetCopyCount") == null)
                            {
                                key.SetValue("ForceSetCopyCount", 1, RegistryValueKind.DWord);
                                result = true;
                                Console.WriteLine("Registry key has been set successfully");
                            }
                            else
                            {
                                Console.WriteLine("The key already exists");
                                result = true;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Cannot find key: {keyName}");
                            result = false;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result = false;
            }
            return result;
        }
    }
}
