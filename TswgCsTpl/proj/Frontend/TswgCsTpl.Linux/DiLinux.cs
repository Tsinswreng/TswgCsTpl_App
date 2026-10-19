namespace TswgCsTpl.Linux;

using Microsoft.Extensions.DependencyInjection;
using Tsinswreng.CsCore;
using TswgCsTpl.Common;
using TswgCsTpl.Log;
using TswgCsTpl.Ui;

[Doc($$"""
Linux 入口層的依賴注入裝配（程序集 `TswgCsTpl.Linux` → {nameof(DiLinux)}）。

*與 Windows 的差異只有日誌*：Linux 從終端啟動，stdout 就是控制台，故不必另寫檔案。
""")]
public static class DiLinux {
	[Doc("註冊本宿主需要的服務：儲存區、主控台日誌，以及共用的 UI 層。")]
	public static IServiceCollection SetupLinux(
		this IServiceCollection z
	) {
		// 1: 應用儲存區根目錄。桌面取可執行檔所在目錄（與 Windows 同一個實現）。
		z.AddSingleton<I_GetBaseDir>(AppDirBaseDirMgr.Inst);

		// 2: 日誌出口：主控台。
		z.AddAppLog(MkrLogger.Console(App.AppName));

		// 3: 共用的 UI 層（內部會先接上共享層的 SetupCore）。
		z.SetupUi();

		return z;
	}
}
