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

		// 本平臺的全部註冊都集中在 DiAndroid。
		Svc.SetupAndroid();

		return Svc.BuildServiceProvider();
	}
}


