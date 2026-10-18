namespace TswgCsTpl.Android;

using Microsoft.Extensions.DependencyInjection;
using Tsinswreng.CsCore;
using TswgCsTpl.Common;
using TswgCsTpl.Log;

[Doc($$"""
Android 入口層的依賴注入裝配（程序集 `TswgCsTpl.Android` → {nameof(DiAndroid)}）。

*與桌面兩頭的差異*：儲存區是應用的外部私有目錄（`GetExternalFilesDir`，不需權限、卸載即清），
日誌進 logcat（固定 tag 便於 `adb logcat -s` 單獨過濾）。
""")]
public static class DiAndroid {
	[Doc("註冊本宿主需要的服務：外部私有儲存區、logcat 日誌，以及共用的 UI 層。")]
	public static IServiceCollection SetupAndroid(
		this IServiceCollection z
	) {
		// 1: 儲存區根目錄。Context 由 Application 建構子交給了 ExternalFilesBaseDirMgr。
		z.AddSingleton<I_GetBaseDir>(ExternalFilesBaseDirMgr.Inst);

		// 2: 日誌出口：logcat。tag 用全名寫出，避免與 Android 的命名空間混淆。
		z.AddAppLog(new AndroidLogger(TswgCsTpl.Ui.App.AppName));

		// 3: 共用的 UI 層（內部會先接上共享層的 SetupCore）。
		TswgCsTpl.Ui.DiUi.SetupUi(z);

		return z;
	}
}
