namespace TswgCsTpl.Browser;

using System.Threading.Tasks;

using Avalonia;
using Avalonia.Browser;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using TswgCsTpl.Common;
using TswgCsTpl.Ui;

internal sealed partial class Program{

	private static partial Task Main(str[] Args){
		// step 1: 容器必須在建立視圖之前交給業務層的 Di 門面
		//         （App.OnFrameworkInitializationCompleted 會建立根視圖）。
		Di.SvcProvider = MkServices();

		// step 2: 交給瀏覽器宿主啟動。"out" 對應 wwwroot/index.html 裡的 <div id="out">。
		return BuildAvaloniaApp()
			.WithInterFont()
			.StartBrowserAppAsync("out");
	}

	public static partial AppBuilder BuildAvaloniaApp(){
		return AppBuilder.Configure<App>();
	}

	private static partial IServiceProvider MkServices(){
		var Svc = new ServiceCollection();

		// step 1: 日誌。瀏覽器的輸出落在開發者工具的 Console，
		//         故與其他入口層用同一份控制台實現。
		AppLog.Inst.InnerLogger = LoggerFactory
			.Create(B => B.AddConsole())
			.CreateLogger(nameof(TswgCsTpl));
		Svc.AddSingleton<ILogger>(AppLog.Inst);
		Svc.AddLogging(B => B.AddConsole());

		// step 2: 本平臺的服務實現日後加在這裡（例如 LocalStorage 版的鍵值存儲）。
		// step 3: 共同 UI 層的 ViewModel 日後由 Ui 層的 DiUi.SetupUi() 統一註冊。

		return Svc.BuildServiceProvider();
	}
}
