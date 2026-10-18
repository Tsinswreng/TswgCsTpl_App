namespace TswgCsTpl.Ui;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using TswgCsTpl.Common;
using TswgCsTpl.Ui.Views.Main;

[Doc(@$"應用實例：四個入口層（Windows／Linux／Android／Browser）共用同一份。

這裡是全項目唯一需要判斷「生命週期型別」的地方：
桌面（Windows／Linux）走 {nameof(IClassicDesktopStyleApplicationLifetime)}，
Android／Browser 走 {nameof(ISingleViewApplicationLifetime)}。
兩種生命週期掛的是同一個 `ViewMain`，故視圖層完全不必知道自己在哪個平臺。

但**入口層**仍然各平臺一份（`Program.cs` / `Application.cs`），
因為啟動方式（可執行檔、Android Application、WASM 主機）本質上不同。
")]
public partial class App : Application{

	[Doc("應用名稱：視窗標題用它，各入口層不再各寫一份。")]
	public const str AppName = "TswgCsTpl";

	[Doc(@$"建立主視窗（只有走桌面生命週期的入口層會用到）。

抽成方法而不是寫在建構流程裡：日後要加圖標、最小尺寸、自繪標題欄時只有一處要改。
#Rtn[尚未顯示、內容已掛好 `ViewMain` 的視窗。]
")]
	public static partial Window MkMainWindow();

	[Doc(@"初始化全局樣式與主題。取代 Avalonia 模板中的 App.axaml——本專案不寫任何 XAML。
這裡也是「主題的唯一入口」：換主題包時只改本方法。
")]
	public override partial void Initialize();

	[Doc(@"框架初始化完成：依當下的生命週期把根視圖掛上去。

*為何集中在 App 而不是各入口層*：入口層各自掛根視圖的話，
「桌面開視窗、單視圖掛控件」這件差異就會在三、四個地方各寫一遍，改一處漏三處。
")]
	public override partial void OnFrameworkInitializationCompleted();
}
