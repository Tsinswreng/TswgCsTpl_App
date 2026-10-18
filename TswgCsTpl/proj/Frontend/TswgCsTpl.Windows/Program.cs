namespace TswgCsTpl.Windows;

using Tsinswreng.CsCore;

[Doc(@"Windows 入口層的可執行入口。

*為何入口層不能只有一行 `AppBuilder.Configure<App>()`*：各平臺的啟動方式本質不同
（桌面是可執行檔、Android 是 Application 與 Activity、瀏覽器是 WASM 主機），
而且各平臺要註冊的服務實現也不同。把這些差異收在入口層，
`TswgCsTpl.Ui` 就能在四個平臺原樣共用。
")]
public static partial class Program{

	[Doc(@"進程入口。**必須在 STA 線程啟動**：缺少 `[STAThread]` 時，
Windows 上會出現難以定位的 COM 初始化失敗。
#Prm[命令列參數。]
")]
	public static partial void Main(str[] Args);

	[Doc(@"組裝依賴注入容器：註冊本平臺的服務實現。
*必須在建立任何視圖之前*完成並交給業務層的 `Di` 門面，
否則視圖解析 ViewModel 時會取不到依賴。

除了本平臺的服務實現，這裡也一併決定兩件全應用共用的事：
應用儲存區根目錄（`AppDirBaseDirMgr`，供寫檔的元件向容器取）與
日誌出口（`AddAppLog`，內層實現是「主控台 ＋ 檔案」雙寫的 `MkrLogger.ConsoleEtFile`）。
#Rtn[建好的服務提供源。]
")]
	private static partial IServiceProvider MkServices();
}
