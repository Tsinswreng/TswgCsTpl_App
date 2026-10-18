namespace TswgCsTpl.Browser;

using System.Threading.Tasks;

using Avalonia;
using Avalonia.Browser;

using Tsinswreng.CsCore;

[Doc(@"Browser（WebAssembly）入口層。

*宿主是瀏覽器*：沒有視窗、也沒有 Activity，Avalonia 以 `StartBrowserAppAsync`
接管 `wwwroot/index.html` 裡那個 id 為 out 的 div 作為渲染根。
根控件由 App.OnFrameworkInitializationCompleted 依單視圖生命週期掛載，
故與 Android、桌面共用同一份界面。
")]
internal sealed partial class Program{

	[Doc(@"WASM 的程式入口（由執行期呼叫）。
#Prm[命令列參數（瀏覽器環境下通常為空）。]
#Rtn[應用執行期間的工作]
")]
	private static partial Task Main(str[] Args);

	[Doc(@"配置 Avalonia。公開是因為瀏覽器的開發伺服器與設計器會用它。
#Rtn[配置好的 AppBuilder]
")]
	public static partial AppBuilder BuildAvaloniaApp();

	[Doc(@"組裝依賴注入容器：註冊本平臺的服務實現。
*必須在建立任何視圖之前*完成並交給業務層的 `Di` 門面。
#Rtn[建好的服務提供源。]
")]
	private static partial IServiceProvider MkServices();
}
