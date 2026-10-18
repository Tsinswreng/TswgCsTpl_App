namespace TswgCsTpl.Browser;

using System.Threading.Tasks;

using Avalonia;
using Avalonia.Browser;

using Microsoft.Extensions.DependencyInjection;

using TswgCsTpl.Common;
using TswgCsTpl.Log;
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

		// step 1: 應用儲存區根目錄。Browser 沒有檔案系統，
		//         故註冊一個明確拒絕的實現，免得日後有人在 WASM 上寫檔卻靜默失敗
		//         （要真的落地檔案得接 LocalStorage／IndexedDB，那是另一個決定）。
		Svc.AddSingleton<I_GetBaseDir>(NoBaseDirMgr.Inst);

		// step 2: 日誌。瀏覽器沒有終端，但 WASM 的 stdout 被導向開發者工具的 Console，
		//         故與 Linux 用同一份主控台實現。
		Svc.AddAppLog(MkrLogger.Console(App.AppName));

		// step 3: UI 層的共用服務（HTTP 客戶端、會話、各 ViewModel）。
		Svc.SetupUi();

		// step 4: 本平臺的服務實現日後加在這裡（例如 LocalStorage 版的鍵值存儲）。

		// step 5: （已由上面的 SetupUi 完成）

		return Svc.BuildServiceProvider();
	}
}

