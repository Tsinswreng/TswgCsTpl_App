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

		// 本平臺的全部註冊都集中在 DiBrowser。
		Svc.SetupBrowser();

		return Svc.BuildServiceProvider();
	}
}


