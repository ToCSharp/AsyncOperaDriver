# AsyncOperaDriver
Opera WebDriver and Chrome DevTools in one library.  
Chrome DevTools because Opera is based on Chromium.  
So AsyncOperaDriver is based on [AsyncChromeDriver](https://github.com/ToCSharp/AsyncChromeDriver)

It connects directly to [Chrome DevTools](https://chromedevtools.github.io/devtools-protocol/) and is async from this connection.  
No need in operadriver.

AsyncOperaDriver implements [IAsyncWebBrowserClient](https://github.com/ToCSharp/IAsyncWebBrowserClient) and can be used as [AsyncWebDriver](https://github.com/ToCSharp/AsyncWebDriver).

It also has DevTools property and you can easily use all power of Chrome DevTools from your .Net app. Thanks to [BaristaLabs/chrome-dev-tools-sample](https://github.com/BaristaLabs/chrome-dev-tools-sample)

[![Join the chat at https://gitter.im/AsyncWebDriver/Lobby](https://badges.gitter.im/AsyncWebDriver/Lobby.svg)](https://gitter.im/AsyncWebDriver/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

## Usage
### Install AsyncOperaDriver via NuGet

If you want to include AsyncOperaDriver in your project, you can [install it directly from NuGet](https://www.nuget.org/packages/AsyncOperaDriver/)
```
PM> Install-Package AsyncOperaDriver
```
### Write code example
```csharp
     var asyncOperaDriver = new AsyncOperaDriver();
     var webDriver = new WebDriver(asyncOperaDriver);
     await webDriver.GoToUrl("https://www.google.com/");
     var query = await webDriver.FindElement(By.Name("q"));
     foreach (var v in "ToCSharp".ToList())
     {
        await Task.Delay(500 + new Random().Next(500));
        await query.SendKeys(v.ToString());
      }
      await Task.Delay(500);
      await query.SendKeys(Keys.Enter);
      var allCookies = await asyncOperaDriver.DevTools.Session.Network.GetAllCookies(new GetAllCookiesCommand());

      var screenshot = await webDriver.GetScreenshot();
      screenshot.SaveAsFile(GetFilePathToSaveScreenshot(), Zu.WebBrowser.BasicTypes.ScreenshotImageFormat.Png);

```
### Using DevTools
```csharp
    asyncOperaDriver = new AsyncOperaDriver();
    await asyncOperaDriver.CheckConnected();
    await asyncOperaDriver.DevTools.Session.Page.Enable(new BaristaLabs.ChromeDevTools.Runtime.Page.EnableCommand());
    asyncOperaDriver.DevTools.Session.Page.SubscribeToDomContentEventFiredEvent(async (e2) =>
    {
        var screenshot = await asyncOperaDriver.DevTools.Session.Page.CaptureScreenshot(new BaristaLabs.ChromeDevTools.Runtime.Page.CaptureScreenshotCommand());
        SaveScreenshot(screenshot.Data);
    });
    //await asyncOperaDriver.GoToUrl("https://www.google.com/");
    await asyncOperaDriver.DevTools.Session.Page.Navigate(new BaristaLabs.ChromeDevTools.Runtime.Page.NavigateCommand
    {
        Url = "https://www.google.com/"
    });
```

## Examples
Look at AsyncOperaDriverExample.

Run built Example in release tab.

## Implemented
Most functionality is in [AsyncChromeDriver](https://github.com/ToCSharp/AsyncChromeDriver) project and AsyncOperaDriver depends on its implementation.

## Contribute!
If you see NotImplementedException, means you need functionality, which is not implemented yet.

Write issue or to [![Join the chat at https://gitter.im/AsyncWebDriver/Lobby](https://badges.gitter.im/AsyncWebDriver/Lobby.svg)](https://gitter.im/AsyncWebDriver/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge). I implemented what I myself use at first.

Feel free to submit pull requests.

Please star the project. More stars - more time I'll spend on it. There's so much to do here.
