namespace TswgCsTpl.Android;

using global::Android.App;
using global::Android.Content.PM;
using Avalonia.Android;
using Tsinswreng.CsCore;

[Doc(@$"Android 的視圖宿主。

Avalonia 12 的 AppBuilder 由 {nameof(Application)} 配置，Activity 只負責窗口生命週期，
故本類目前不含任何邏輯；日後要做「切到背景時補寫狀態」（`OnPause`／`OnStop`）
或查詢系統權限時，加在這裡。

*這兩個屬性不能省*：
`Theme` 指向 Resources/values 裡宣告的 NoActionBar 主題（缺它會多一條系統標題欄）；
`ConfigurationChanges` 讓螢幕旋轉、尺寸與深色模式切換時**不重建 Activity**
——重建等同重新開機，Avalonia 的整個視覺樹與瀏覽狀態都會一起丟掉。
")]
[Activity(
	Label = "TswgCsTpl",
	Theme = "@style/MyTheme.NoActionBar",
	Icon = "@drawable/icon",
	MainLauncher = true,
	ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public partial class MainActivity : AvaloniaMainActivity{
}
