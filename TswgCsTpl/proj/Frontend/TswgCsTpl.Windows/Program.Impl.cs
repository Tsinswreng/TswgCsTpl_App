namespace TswgCsTpl.Windows;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

using Microsoft.Extensions.DependencyInjection;

using TswgCsTpl.Common;
using TswgCsTpl.Log;
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

		// step 1: 應用儲存區根目錄。桌面取可執行檔所在目錄，
		//         日誌等一切執行期寫入都以它為根（理由見 AppDirBaseDirMgr 的說明）。
		Svc.AddSingleton<I_GetBaseDir>(AppDirBaseDirMgr.Inst);

		// step 2: 日誌。桌面視窗沒有附著終端，故內層實現寫檔；
		//         注入 ILogger、注入泛型 ILogger、直接調 AppLog 三者共用這一條出口。
		Svc.AddAppLog(new WindowsFileLogger());

		// step 3: UI 層的共用服務（HTTP 客戶端、會話、各 ViewModel）。
		Svc.SetupUi();

		// step 4: 本平臺的服務實現（例如文件系統、剪貼板…）日後加在這裡。
		//         Ui 層只認介面，故換實現時不必動視圖。

		// step 5: （已由上面的 SetupUi 完成）共同 UI 層的 ViewModel
		//         四個入口層各呼叫一次。

		return Svc.BuildServiceProvider();
	}
}

