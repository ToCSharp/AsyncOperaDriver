// Copyright (c) Oleg Zudov. All Rights Reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Zu.Chrome;
using Zu.WebBrowser;
using Zu.WebBrowser.BasicTypes;

namespace Zu.Opera
{
    public class AsyncOperaDriver : AsyncChromeDriver
    {
        private DriverConfig config;
        private bool _isClosed = false;

        public AsyncOperaDriver(bool openInTempDir = true)
            : this(11000 + new Random().Next(2000))
        {
            if (openInTempDir)
            {
                IsTempProfile = true;
                UserDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }
        }
        public AsyncOperaDriver(string profileDir, int port)
            : this(port)
        {
            IsTempProfile = false;
            UserDir = profileDir;
        }

        public AsyncOperaDriver(string profileDir)
            : this(11000 + new Random().Next(2000))
        {
            IsTempProfile = false;
            UserDir = profileDir;
        }

        public AsyncOperaDriver(int port)
        {
            Port = port;
            DevTools = new ChromeDevToolsConnection(Port);
            CreateDriverCore();
        }

        public AsyncOperaDriver(DriverConfig config)
            :base(config)
        {
            
        }

        public override async Task<string> Connect(CancellationToken cancellationToken = default(CancellationToken))
        {
            UnsubscribeDevToolsSessionEvent();
            DoConnectWhenCheckConnected = false;
            /*if (!string.IsNullOrWhiteSpace(UserDir)) */chromeProcess = await OpenOperaProfile(UserDir);
            int connection_attempts = 0;
            const int MAX_ATTEMPTS = 10;
            while (true)
            {
                connection_attempts++;
                try
                {
                    await DevTools.Connect();
                    break;
                }
                catch (Exception ex)
                {
                    //LiveLogger.WriteLine("Connection attempt {0} failed with: {1}", connection_attempts, ex);
                    if (_isClosed || connection_attempts >= MAX_ATTEMPTS)
                    {
                        throw;
                    }
                    else
                    {
                        await Task.Delay(200);
                    }
                }
            }
            SubscribeToDevToolsSessionEvent();
            await FrameTracker.Enable();
            return $"Connected to Opera port {Port}";
        }



        private async Task<OperaProcessInfo> OpenOperaProfile(string userDir)
        {
            OperaProcessInfo res = null;
            await Task.Run(() => res = OperaProfilesWorker.OpenOperaProfile(userDir, Port));
            return res;
        }

    }
}
