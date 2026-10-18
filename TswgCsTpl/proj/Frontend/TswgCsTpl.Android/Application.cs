namespace TswgCsTpl.Android;

using global::Android.App;
using global::Android.Runtime;
using Avalonia;
using Avalonia.Android;
using Tsinswreng.CsCore;

[Doc(@$"Android 應用入口（托管側）。

*為何沒有 `Main`*：Android 的進程由系統啟動，托管側的入口是
{nameof(Application)} 與 Activity，不由命令列進入。
Avalonia 12 把 AppBuilder 的配置放在 Application（`AvaloniaAndroidApplication<TApp>`），
Activity 只負責窗口生命週期；這與 Avalonia 11 把兩者都放在 Activity 的作法不同。
")]
[Application]
public sealed partial class Application : AvaloniaAndroidApplication<TswgCsTpl.Ui.App>{

	[Doc(@$"Java 物件包裝用的建構子。
系統在部分情境（行程重建）下會以這個簽名建立實例，故必須保留。
#Prm[Java 端物件的控制代碼。]
#Prm[控制代碼所有權的轉移方式。]
")]
	public partial Application(nint JavaReference, JniHandleOwnership Transfer);

	[Doc(@$"配置 Avalonia，並在建立任何視圖之前把服務容器交給業務層的 `Di` 門面。
#Prm[由基類建立的 AppBuilder。]
#Rtn[配置完成的 AppBuilder。]
")]
	protected override partial AppBuilder CustomizeAppBuilder(AppBuilder Builder);

	[Doc(@"組裝依賴注入容器：註冊本平臺的服務實現。
是實例方法而非靜態：本類自己就是 Android 的 Context，
建構子已把它交給儲存區實現，這裡再把儲存區與日誌出口註冊進容器，
故不需要另外去找一個靜態的全域 Context。
日誌走 `AddAppLog`：內層實現是 logcat（見 AndroidLogger），
日後要改成寫檔也只需換這一行傳進去的實現。
#Rtn[建好的服務提供源。]
")]
	private partial IServiceProvider MkServices();
}
