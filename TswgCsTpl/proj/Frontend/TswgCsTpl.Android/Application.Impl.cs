namespace TswgCsTpl.Android;

using global::Android.Runtime;
using Avalonia;
using Avalonia.Android;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using TswgCsTpl.Common;

public sealed partial class Application{

	public partial Application(nint JavaReference, JniHandleOwnership Transfer)
		: base(JavaReference, Transfer){
	}

	protected override partial AppBuilder CustomizeAppBuilder(AppBuilder Builder){
		// step 1: 容器必須在建立視圖之前交給業務層的 Di 門面。
		//         Ui 層的視圖透過 Di.DiOrMk<T>() 取得 ViewModel，
		//         晚一步設置就會拿到沒有依賴的物件、或直接拋錯。
		Di.SvcProvider = MkServices();

		// step 2: 單視圖生命週期的根控件由 App.OnFrameworkInitializationCompleted 掛載，
		//         與桌面入口層共用同一份 App，故這裡只需補上 Android 需要的字型。
		return base.CustomizeAppBuilder(Builder)
			.WithInterFont();
	}

	private partial IServiceProvider MkServices(){
		var Svc = new ServiceCollection();

		// step 1: 日誌。Android 上真正的日誌實現（Logcat）日後由本層提供，
		//         現在先用控制台版本，讓四個入口層的行為保持一致。
		AppLog.Inst.InnerLogger = LoggerFactory
			.Create(B => B.AddConsole())
			.CreateLogger(nameof(TswgCsTpl));
		Svc.AddSingleton<ILogger>(AppLog.Inst);
		Svc.AddLogging(B => B.AddConsole());

		// step 2: 本平臺的服務實現日後加在這裡（文件系統、剪貼板…）。
		// step 3: 共同 UI 層的 ViewModel 日後由 Ui 層的 DiUi.SetupUi() 統一註冊。

		return Svc.BuildServiceProvider();
	}
}
