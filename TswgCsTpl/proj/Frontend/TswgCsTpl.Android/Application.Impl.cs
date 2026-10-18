namespace TswgCsTpl.Android;

using global::Android.Runtime;
using Avalonia;
using Avalonia.Android;

using Microsoft.Extensions.DependencyInjection;

using TswgCsTpl.Common;
using TswgCsTpl.Log;

public sealed partial class Application{

	public partial Application(nint JavaReference, JniHandleOwnership Transfer)
		: base(JavaReference, Transfer){
		// 本類自己就是 Android 的 Context，故直接把它交給儲存區實現；
		// 這一步必須早於任何可能需要寫檔的元件（日誌、配置）被建立。
		ExternalFilesBaseDirMgr.Inst._Ctx = this;
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

		// step 1: 應用儲存區根目錄。Android 取應用的外部私有目錄：
		//         不需要權限，卸載時由系統一併清理（理由見 ExternalFilesBaseDirMgr 的說明）。
		Svc.AddSingleton<I_GetBaseDir>(ExternalFilesBaseDirMgr.Inst);

		// step 2: 日誌。Android 的輸出進 logcat，固定 tag 讓 `adb logcat -s` 能單獨過濾；
		//         注入 ILogger、注入泛型 ILogger、直接調 AppLog 三者共用這一條出口。
		//         tag 用 TswgCsTpl.Ui.App.AppName 全名寫出：本檔同時 using 了 Android 的命名空間，
		//         簡單名 `App` 有機會被解析成命名空間而非類型。
		Svc.AddAppLog(new AndroidLogger(TswgCsTpl.Ui.App.AppName));

		// step 3: UI 層的共用服務（HTTP 客戶端、會話、各 ViewModel）。
		TswgCsTpl.Ui.DiUi.SetupUi(Svc);

		// step 4: 本平臺的服務實現日後加在這裡（文件系統、剪貼板…）。

		// step 4: 共同 UI 層的 ViewModel 日後由 Ui 層的 DiUi.SetupUi() 統一註冊。

		return Svc.BuildServiceProvider();
	}
}

