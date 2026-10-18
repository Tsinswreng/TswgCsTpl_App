namespace TswgCsTpl.Browser;

using Microsoft.Extensions.DependencyInjection;
using Tsinswreng.CsCore;
using TswgCsTpl.Common;
using TswgCsTpl.Log;
using TswgCsTpl.Ui;

[Doc($$"""
Browser 入口層的依賴注入裝配（程序集 `TswgCsTpl.Browser` → {nameof(DiBrowser)}）。

*沒有檔案系統*：儲存區註冊 {nameof(NoBaseDirMgr)}（取用時明確拋出），
免得日後有人在 WASM 上寫檔卻靜默失敗。日誌走主控台——WASM 的 stdout 被導向開發者工具。
""")]
public static class DiBrowser {
	[Doc("註冊本宿主需要的服務：明確拒絕的儲存區、主控台日誌，以及共用的 UI 層。")]
	public static IServiceCollection SetupBrowser(
		this IServiceCollection z
	) {
		// 1: 沒有檔案系統，故用「明確拒絕」的實現。
		z.AddSingleton<I_GetBaseDir>(NoBaseDirMgr.Inst);

		// 2: 日誌出口：主控台（開發者工具的 Console）。
		z.AddAppLog(MkrLogger.Console(App.AppName));

		// 3: 共用的 UI 層（內部會先接上共享層的 SetupCore）。
		z.SetupUi();

		return z;
	}
}
