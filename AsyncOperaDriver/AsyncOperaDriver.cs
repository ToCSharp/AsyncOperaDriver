// Copyright (c) Oleg Zudov. All Rights Reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Zu.Chrome;
using Zu.WebBrowser;

namespace Zu.Opera
{
    public class AsyncOperaDriver: AsyncChromeDriver
    {
        private bool isTempUserDir;
        public AsyncOperaDriver(bool openInTempDir = true)
            : this(11000 + new Random().Next(2000))
        {
            if (openInTempDir)
            {
                isTempUserDir = true;
                UserDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }
        }

        public AsyncOperaDriver(int port)
        {
            CurrentContext = Contexts.Chrome;
            Port = port;
            DevTools = new ChromeDevTools(Port);
            CreateDriverCore();
        }

        public override async Task<string> Connect(CancellationToken cancellationToken = default(CancellationToken))
        {
            UnsubscribeDevToolsSessionEvent();
            DoConnectWhenCheckConnected = false;
            if (!string.IsNullOrWhiteSpace(UserDir)) chromeProcess = await OpenOperaProfile(UserDir);
            await DevTools.Connect();
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
