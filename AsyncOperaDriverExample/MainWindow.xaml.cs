﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Threading;
using System.IO;
using BaristaLabs.ChromeDevTools.Runtime.Network;
using Zu.AsyncWebDriver;
using Zu.AsyncWebDriver.Remote;
using AsyncChromeDriverExample;
using Zu.Opera;
using Zu.WebBrowser.BasicTypes;

namespace AsyncOperaDriverExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AsyncOperaDriver asyncOperaDriver;
        private WebDriver webDriver;
        private ChromeRequestListener chromeRequestListener;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                asyncOperaDriver = new AsyncOperaDriver();
                webDriver = new WebDriver(asyncOperaDriver);
                await asyncOperaDriver.Connect();
                tbDevToolsRes.Text = "opened";
                tbDevToolsRes2.Text = $"opened on port {asyncOperaDriver.Port} in dir {asyncOperaDriver.UserDir} \nWhen close, dir will be DELETED";
            }
            catch (Exception ex)
            {
                tbDevToolsRes.Text = ex.ToString();
                tbDevToolsRes2.Text = ex.ToString();
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            await webDriver?.Close();
            //await asyncChromeDriver?.Close();
            tbDevToolsRes.Text = "closed";
            tbDevToolsRes2.Text = "closed";
        }

        ObservableCollection<ResponseReceivedEventInfo> responseEvents = new ObservableCollection<ResponseReceivedEventInfo>();
        ObservableCollection<WebSocketFrameReceivedEventInfo> wsEvents = new ObservableCollection<WebSocketFrameReceivedEventInfo>();

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            lbDevToolsRequests.ItemsSource = responseEvents;
            lbDevToolsWS.ItemsSource = wsEvents;

            chromeRequestListener = new ChromeRequestListener(asyncOperaDriver);
            chromeRequestListener.ResponseReceived += (s, ev) => Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate () { responseEvents.Insert(0, ev); });
            chromeRequestListener.WebSocketFrameReceived += (s, ev) => Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate () { wsEvents.Insert(0, ev); });
            await chromeRequestListener.StartListen();
            tbDevToolsRes.Text = "enabled";

        }

        private async void Button_Click_26(object sender, RoutedEventArgs e)
        {
            if (chromeRequestListener != null)
            {
                var r = lbDevToolsRequests.SelectedItem as ResponseReceivedEventInfo;
                if (r == null) return;
                var res = await chromeRequestListener.GetCookies(r);
                if (res == null)
                {
                    tbDevToolsRes.Text = "";
                }
                else
                {
                    tbDevToolsRes.Text = string.Join(Environment.NewLine, res.Select(c => CookieToString(c))); //.ToString()));
                }
            }
        }

        private async void Button_Click_27(object sender, RoutedEventArgs e)
        {
            if (chromeRequestListener != null)
            {
                var res = await chromeRequestListener.GetAllCookies();
                if (res == null)
                {
                    tbDevToolsRes.Text = "";
                }
                else
                {
                    tbDevToolsRes.Text = string.Join(Environment.NewLine, res.Select(c => CookieToString(c)));
                }
            }
        }

        string CookieToString(BaristaLabs.ChromeDevTools.Runtime.Network.Cookie c)
        {
            var c2 = new Zu.WebBrowser.BasicTypes.Cookie(c.Name, c.Value, c.Domain, c.Path,
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(c.Expires).ToLocalTime()); //, DateTimeOffset.FromUnixTimeMilliseconds((long)c.Expires).UtcDateTime);
            return c2.ToString();
        }

        private async void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (webDriver == null)
            {
                asyncOperaDriver = new AsyncOperaDriver();
                webDriver = new WebDriver(asyncOperaDriver);
            }
            try
            {
                var res2 = await webDriver.GoToUrl("https://www.google.com/");
                var query = await webDriver.FindElement(By.Name("q"));
                if (query == null)
                {
                    await Task.Delay(1000);
                    query = await webDriver.FindElement(By.Name("q"));
                }
                foreach (var v in "ToCSharp")
                {
                    await Task.Delay(500 + new Random().Next(1000));
                    await query.SendKeys(v.ToString());
                }
                await Task.Delay(500);
                await query.SendKeys(Keys.Enter);
                await Task.Delay(2000);
                query = await webDriver.FindElement(By.Name("q"));
                await query.SendKeys(Keys.ArrowDown);
                await Task.Delay(1000);
                await query.SendKeys(Keys.ArrowDown);
                await Task.Delay(2000);
                await query.SendKeys(Keys.ArrowDown);
                await Task.Delay(1000);
                await query.SendKeys(Keys.ArrowUp);
                await Task.Delay(500);
                await query.SendKeys(Keys.Enter);
                var el = await webDriver.SwitchTo().ActiveElement();
                await webDriver.Keyboard.SendKeys(Keys.PageDown);
                var allCookies = await asyncOperaDriver.DevTools.Session.Network.GetAllCookies(new GetAllCookiesCommand());
                var screenshot = await asyncOperaDriver.DevTools.Session.Page.CaptureScreenshot(new BaristaLabs.ChromeDevTools.Runtime.Page.CaptureScreenshotCommand());
                if (!string.IsNullOrWhiteSpace(screenshot.Data))
                {
                    var dir = @"C:\temp";
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    var i = 0;
                    var path = "";
                    do
                    {
                        i++;
                        path = Path.Combine(dir, $"screenshot{i}.png");
                    } while (File.Exists(path));
                    File.WriteAllBytes(path, Convert.FromBase64String(screenshot.Data));
                }

            }
            catch (Exception ex)
            {
                tbDevToolsRes.Text = ex.ToString();
            }
        }

        private async void lbDevToolsRequests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (chromeRequestListener != null)
            {
                var res = await chromeRequestListener.GetResponseBody(lbDevToolsRequests.SelectedItem as ResponseReceivedEventInfo);
                tbDevToolsRes.Text = res;
            }
        }

        private async void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (webDriver != null)
            {
                await webDriver.Keyboard.SendKeys(Keys.Up);
            }

        }

        private async void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (webDriver != null)
            {
                await webDriver.Keyboard.SendKeys(Keys.Down);
            }

        }

        private async void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if (webDriver == null)
            {
                asyncOperaDriver = new AsyncOperaDriver();
                webDriver = new WebDriver(asyncOperaDriver);
            }
            try
            {
                await asyncOperaDriver.CheckConnected();
                await asyncOperaDriver.DevTools.Session.Page.Enable(new BaristaLabs.ChromeDevTools.Runtime.Page.EnableCommand());
                asyncOperaDriver.DevTools.Session.Page.SubscribeToDomContentEventFiredEvent(async (e2) =>
                {
                    var screenshot = await asyncOperaDriver.DevTools.Session.Page.CaptureScreenshot(new BaristaLabs.ChromeDevTools.Runtime.Page.CaptureScreenshotCommand());
                    SaveScreenshot(screenshot.Data);

                });
                var res2 = await webDriver.GoToUrl("https://www.google.com/");

            }
            catch (Exception ex)
            {
                tbDevToolsRes.Text = ex.ToString();
            }
        }

        private static void SaveScreenshot(string base64String)
        {
            if (!string.IsNullOrWhiteSpace(base64String))
            {
                string path = GetFilePathToSaveScreenshot();
                File.WriteAllBytes(path, Convert.FromBase64String(base64String));
            }
        }

        private static string GetFilePathToSaveScreenshot()
        {
            var dir = @"C:\temp";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var i = 0;
            var path = "";
            do
            {
                i++;
                path = Path.Combine(dir, $"screenshot{i}.png");
            } while (File.Exists(path));
            return path;
        }

        private async void Button_Click_7(object sender, RoutedEventArgs e)
        {
            if (webDriver == null)
            {
                asyncOperaDriver = new AsyncOperaDriver();
                webDriver = new WebDriver(asyncOperaDriver);
            }
            try
            {
                var res2 = await webDriver.GoToUrl("https://www.google.com/");
                var screenshot = await asyncOperaDriver.DevTools.Session.Page.CaptureScreenshot(new BaristaLabs.ChromeDevTools.Runtime.Page.CaptureScreenshotCommand());
                SaveScreenshot(screenshot.Data);

            }
            catch (Exception ex)
            {
                tbDevToolsRes.Text = ex.ToString();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (webDriver != null)
            {
                try
                {
                    webDriver.CloseSync();
                }
                catch { }
            }
        }

        private async void Button_Click_8(object sender, RoutedEventArgs e)
        {
            if (webDriver == null)
            {
                asyncOperaDriver = new AsyncOperaDriver();
                webDriver = new WebDriver(asyncOperaDriver);
            }
            try
            {
                var res2 = await webDriver.GoToUrl("https://www.google.com/");
                var screenshot = await webDriver.GetScreenshot();
                string path = GetFilePathToSaveScreenshot();
                //screenshot.SaveAsFile(path, Zu.WebBrowser.BasicTypes.ScreenshotImageFormat.Png);
                using (MemoryStream imageStream = new MemoryStream(screenshot.AsByteArray))
                {
                    System.Drawing.Image screenshotImage = System.Drawing.Image.FromStream(imageStream);
                    screenshotImage.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            catch (Exception ex)
            {
                tbDevToolsRes.Text = ex.ToString();
            }
        }

        private async void Button_Click_9(object sender, RoutedEventArgs e)
        {
            var userDir = tbOpenProfileDir.Text;
            try
            {
                asyncOperaDriver = new AsyncOperaDriver(userDir);
                webDriver = new WebDriver(asyncOperaDriver);
                // await asyncChromeDriver.Connect(); // browser opens here
                await webDriver.GoToUrl("https://www.google.com/"); // browser opens here
                var mess = $"opened on port {asyncOperaDriver.Port} in dir {asyncOperaDriver.UserDir} \nWhen close, dir will NOT be deleted";
                tbDevToolsRes.Text = mess;
                tbDevToolsRes2.Text = mess;
            }
            catch (Exception ex)
            {
                tbDevToolsRes.Text = ex.ToString();
                tbDevToolsRes2.Text = ex.ToString();
            }

        }

        private async void Button_Click_10(object sender, RoutedEventArgs e)
        {
            var userDir = tbOpenProfileDir.Text;
            if (int.TryParse(tbOpenProfilePort.Text, out int port))
            {
                try
                {
                    asyncOperaDriver = new AsyncOperaDriver(userDir, port);
                    webDriver = new WebDriver(asyncOperaDriver);
                    // await asyncChromeDriver.Connect(); // browser opens here
                    await webDriver.GoToUrl("https://www.google.com/"); // browser opens here
                    var mess = $"opened on port {asyncOperaDriver.Port} in dir {asyncOperaDriver.UserDir} \nWhen close, dir will NOT be deleted";
                    tbDevToolsRes.Text = mess;
                    tbDevToolsRes2.Text = mess;
                }
                catch (Exception ex)
                {
                    tbDevToolsRes.Text = ex.ToString();
                    tbDevToolsRes2.Text = ex.ToString();
                }
            }
        }
    }
}
