﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using SPCodeUpdater.Properties;

namespace SPCodeUpdater
{
    public static class Program
    {
        public delegate void InvokeDel();

        [STAThread]
        public static void Main()
        {
            var processes = Process.GetProcessesByName("SPCode");
            foreach (var process in processes)
            {
                try
                {
                    process.WaitForExit();
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            Application.EnableVisualStyles();
            Thread.Sleep(2000);
            Application.SetCompatibleTextRenderingDefault(true);
            var um = new UpdateMarquee();
            um.Show();
            Application.DoEvents(); //execute Visual
            var t = new Thread(Worker);
            t.Start(um);
            Application.Run(um);
        }

        private static void Worker(object arg)
        {
            var um = (UpdateMarquee)arg;
            var zipFile = Path.Combine(Environment.CurrentDirectory, "updateZipFile.zip");

            var zipFileContent = Resources.Update;

            File.WriteAllBytes(zipFile, zipFileContent);

            var zipInfo = new FileInfo(zipFile);

            using (var archive = ZipFile.OpenRead(zipInfo.FullName))
            {
                // Dont override the sourcemod files
                foreach (var entry in archive.Entries)
                {
                    if (!entry.FullName.StartsWith(@"sourcepawn\"))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(entry.FullName));
                        entry.ExtractToFile(entry.FullName, true);
                    }
                }
            }

            zipInfo.Delete();

            um.Invoke((InvokeDel)(() => { um.SetToReadyState(); }));
        }
    }
}