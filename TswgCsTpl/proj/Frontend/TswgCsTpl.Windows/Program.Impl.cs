namespace TswgCsTpl.Windows;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using TswgCsTpl.Common;
using TswgCsTpl.Ui;

public static partial class Program{

	[STAThread]
	public static partial void Main(str[] Args){
		// step 1: 先建容器並交給業務層的 Di 門面。
		//         Ui 層的視圖透過 Di.DiOrMk<T>() 取得 ViewModel，
		//         故這一步必須早於任何視圖被建立。
		Di.SvcProvider = MkServices();

		// step 2: 桌面生命週期。本應用只有一個視窗，關掉它就等於退出。
		var lifetime = new ClassicDesktopStyleApplicationLifetime{
			Args = Args,
			ShutdownMode = ShutdownMode.OnMainWindowClose,
		};

		// step 3: 配置 Avalonia 並啟動。
		//         主視窗不在這裡建立，而是由 App.OnFrameworkInitializationCompleted
		//         依生命週期掛載——這樣同一份 App 在 Android／Browser 的單視圖
		//         生命週期下也能用。
		AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace()
			.SetupWithLifetime(lifetime);

		lifetime.Start(Args);
	}

	private static partial IServiceProvider MkServices(){
		var Svc = new ServiceCollection();

		// step 1: 日誌。全局門面 AppLog 與容器裡的 ILogger 指向同一份實現，
		//         這樣「有注入」與「沒注入」兩種寫法看到的是同一串日誌。
		AppLog.Inst.InnerLogger = LoggerFactory
			.Create(B => B.AddConsole())
			.CreateLogger(nameof(TswgCsTpl));
		Svc.AddSingleton<ILogger>(AppLog.Inst);
		Svc.AddLogging(B => B.AddConsole());

		// step 2: 本平臺的服務實現（例如文件系統、剪貼板…）日後加在這裡。
		//         Ui 層只認介面，故換實現時不必動視圖。

		// step 3: 共同 UI 層的 ViewModel 日後由 Ui 層的 DiUi.SetupUi() 統一註冊，
		//         四個入口層各呼叫一次。

		return Svc.BuildServiceProvider();
	}
}
