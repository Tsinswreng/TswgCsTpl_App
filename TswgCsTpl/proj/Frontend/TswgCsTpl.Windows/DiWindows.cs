namespace TswgCsTpl.Windows;

using Microsoft.Extensions.DependencyInjection;
using Tsinswreng.CsCore;
using TswgCsTpl.Common;
using TswgCsTpl.Log;
using TswgCsTpl.Ui;

[Doc($$"""
Windows 入口層的依賴注入裝配（程序集 `TswgCsTpl.Windows` → {nameof(DiWindows)}）。

*寫法照文檔約定*：`public static class DiXxx` ＋ `SetupXxx(this IServiceCollection z)`。
一個程序集一處裝配，入口的 `MkServices` 只剩「呼叫它」。
""")]
public static class DiWindows {
	[Doc("註冊本宿主需要的服務：儲存區、日誌出口，以及共用的 UI 層。")]
	public static IServiceCollection SetupWindows(
		this IServiceCollection z
	) {
		// 1: 應用儲存區根目錄。桌面取可執行檔所在目錄。
		z.AddSingleton<I_GetBaseDir>(AppDirBaseDirMgr.Inst);

		// 2: 日誌出口。「主控台 ＋ 檔案」雙寫：從終端跑能當場看到，
		//    雙擊啟動（WinExe 沒有終端）則留下 logs/<應用名>.log 事後查。
		z.AddAppLog(MkrLogger.ConsoleEtFile(App.AppName));

		// 3: 共用的 UI 層（內部會先接上共享層的 SetupCore）。
		z.SetupUi();

		return z;
	}
}
