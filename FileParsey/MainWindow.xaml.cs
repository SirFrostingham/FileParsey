using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using ExecuteCommand;
using FirstFloor.ModernUI.Windows.Controls;
using FileParsey.Model;
using NLog;

namespace FileParsey
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public DateTime CurrentTime { set; get; }

        public DateTime LastTime { get; set; }

        public Whitelist TheWhitelist { get; set; }

        public TargetFiles TargetFiles { get; set; }

        public List<string> ExecutedJobsDaily { get; set; }

        public List<string> ExecutedJobsMaster { get; set; }

        /// <summary>
        /// The function checks whether the current process is run as administrator.
        /// In other words, it dictates whether the primary access token of the 
        /// process belongs to user account that is a member of the local 
        /// Administrators group and it is elevated.
        /// </summary>
        /// <returns>
        /// Returns true if the primary access token of the process belongs to user 
        /// account that is a member of the local Administrators group and it is 
        /// elevated. Returns false if the token does not.
        /// </returns>
        internal bool IsRunAsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public MainWindow()
        {
            var isRunElevated = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("isRunElevated"));

            if(isRunElevated)
                ElevatePermissions();

            InitializeComponent();

            if (isRunElevated)
            {
                if (!IsRunAsAdmin())
                    Application.Current.Shutdown();
            }

            //get whitelist
            LoadWhitelist();

            //get target files
            LoadTargetFiles();

            var outputFile = string.Format("{0}.txt", Guid.NewGuid());

            foreach (var targetFile in TargetFiles.Files)
            {
                try
                {
                    using (var reader = new StreamReader(targetFile))
                    {
                        using (var writer = new StreamWriter(outputFile))
                        {
                            string line = null;
                            while ((line = reader.ReadLine()) != null)
                            {
                                var shouldWriteLine = true;
                                //bans to undo
                                foreach (var item in TheWhitelist.Items)
                                {
                                    if (String.CompareOrdinal(line, item) == 0)
                                        shouldWriteLine = false;
                                }

                                if (!shouldWriteLine)
                                    continue;

                                writer.WriteLine(line);
                            }
                        }
                    }

                    File.Delete(targetFile);
                    File.Copy(outputFile, targetFile);
                    File.Delete(outputFile);
                }
                catch (Exception e)
                {
                    Log.Instance.Error(e.Message);
                }
            }

            Application.Current.Shutdown();
        }

        private void LoadWhitelist()
        {
            TheWhitelist = new Whitelist();
            var serializer = new XmlSerializer(typeof(Whitelist));

            var fs = new FileStream(ConfigurationManager.AppSettings.Get("whitelistFileName"), FileMode.Open);
            TheWhitelist = (Whitelist)serializer.Deserialize(fs);
            fs.Dispose();
        }

        private void LoadTargetFiles()
        {
            TargetFiles = new TargetFiles();
            var serializer = new XmlSerializer(typeof(TargetFiles));

            var fs = new FileStream(ConfigurationManager.AppSettings.Get("targetFilesFileName"), FileMode.Open);
            TargetFiles = (TargetFiles)serializer.Deserialize(fs);
            fs.Dispose();
        }

        private void ElevatePermissions()
        {
            // Elevate the process if it is not run as administrator.
            if (!IsRunAsAdmin())
            {
                // Launch itself as administrator
                ProcessStartInfo proc = new ProcessStartInfo();
                proc.UseShellExecute = true;
                proc.WorkingDirectory = Environment.CurrentDirectory;
                proc.FileName = System.Reflection.Assembly.GetEntryAssembly().Location;  // or System.Windows.Forms.Application.ExecutablePath ;
                proc.Verb = "runas";

                try
                {
                    Process.Start(proc);
                }
                catch
                {
                    // The user refused to allow privileges elevation.;
                    // Do nothing and return directly ...
                    return;
                }

                Application.Current.Shutdown();  // Quit itself
            }
            else
            {
                Console.WriteLine("UAC: The process is running as administrator.");
            }
        }
    }
}
