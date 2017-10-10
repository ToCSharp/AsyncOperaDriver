// Copyright (c) Oleg Zudov. All Rights Reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Zu.Chrome
{
    public static class OperaProfilesWorker
    {
        static OperaProfilesWorker()
        {
            var f1 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string f2 = null;
            if (f1.EndsWith("(x86)"))
            {
                f2 = f1;
                f1 = f1.Substring(0, f1.Length - "(x86)".Length).TrimEnd();
            }
            else
            {
                f2 = f1 + " (x86)";
            }
            OperaBinaryFileName = GetOperaExecutablePath(f1, f2);
            if (OperaBinaryFileName == null)
            {
                var userDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"AppData\Local\Programs");
                OperaBinaryFileName = GetOperaExecutablePath(userDir);
            }

        }

        private static string GetOperaExecutablePath(params string[] folders)
        {
            foreach (var f in folders)
            {
                if (!Directory.Exists(f)) continue;
                //var path = Path.Combine(f, @"Opera\launcher.exe");
                foreach (var dir in Directory.GetDirectories(Path.Combine(f, "Opera")))
                {
                    var path = Path.Combine(dir, "opera.exe");
                    if (File.Exists(path)) return path;
                }
            }
            return null;
        }

        public static string OperaBinaryFileName { get; set; }


        public static OperaProcessInfo OpenOperaProfile(string userDir, int port = 5999)
        {
            //if (string.IsNullOrWhiteSpace(userDir)) throw new ArgumentNullException(nameof(userDir));
            if (port < 1 || port > 65000) throw new ArgumentOutOfRangeException(nameof(port));
            bool firstRun = false;
            if (!string.IsNullOrWhiteSpace(userDir) && !Directory.Exists(userDir))
            {
                firstRun = true;
                Directory.CreateDirectory(userDir);
            }
                var args = "--remote-debugging-port=" + port + " "
                + (string.IsNullOrWhiteSpace(userDir) ? "" : "--user-data-dir=\"" + userDir + "\"")
                + (firstRun ? " --bwsi --no-first-run" : "");
            var process = new ProcessWithJobObject();
            process.StartProc(OperaBinaryFileName, args);

            return new OperaProcessInfo { ProcWithJobObject = process, UserDir = userDir, Port = port };
        }

        internal static OperaProcessInfo OpenOperaProfile(ChromeDriverConfig config)
        {
            if (config.Port < 1 || config.Port > 65000) throw new ArgumentOutOfRangeException(nameof(config.Port));
            bool firstRun = false;
            if (!string.IsNullOrWhiteSpace(config.UserDir) && !Directory.Exists(config.UserDir))
            {
                firstRun = true;
                Directory.CreateDirectory(config.UserDir);
            }

            var args = "--remote-debugging-port=" + config.Port
                + (string.IsNullOrWhiteSpace(config.UserDir) ? "" : " --user-data-dir=\"" + config.UserDir + "\"")
                + (firstRun ? " --bwsi --no-first-run" : "")
                + (config.Headless ? " --headless --disable-gpu" : "")
                + (config.WindowSize != null ? $" --window-size={config.WindowSize.Width},{config.WindowSize.Height}" : "")
                + (string.IsNullOrWhiteSpace(config.CommandLineArgumets) ? "" : " " + config.CommandLineArgumets);


            //if (config.Headless)
            //{
                var process = new ProcessWithJobObject();
                process.StartProc(OperaBinaryFileName, args);
                Thread.Sleep(1000);
                return new OperaProcessInfo { ProcWithJobObject = process, UserDir = config.UserDir, Port = config.Port };
            //}
            //else
            //{
            //    var process = new Process();
            //    process.StartInfo.FileName = OperaBinaryFileName;
            //    process.StartInfo.Arguments = args;
            //    process.StartInfo.UseShellExecute = false;
            //    process.Start();
            //    Thread.Sleep(1000);
            //    return new OperaProcessInfo { Proc = process, UserDir = config.UserDir, Port = config.Port };
            //}
        }
    }
}
