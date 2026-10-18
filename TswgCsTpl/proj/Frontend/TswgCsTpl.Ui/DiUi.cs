namespace TswgCsTpl.Ui;

using Microsoft.Extensions.DependencyInjection;
using Tsinswreng.CsCore;
using TswgCsTpl.Common;
using TswgCsTpl.Shared.User.Svc;
using TswgCsTpl.Ui.Client;
using TswgCsTpl.Ui.Views.Main;
using TswgCsTpl.Ui.Views.User;

[Doc($$"""
UI 層的依賴注入裝配（程序集 `TswgCsTpl.Ui` → {nameof(DiUi)}）。

*寫法照文檔約定*：`public static class DiXxx` ＋ `SetupXxx(this IServiceCollection z)`。

*為何集中在 Ui 而不是各入口層*：界面要什麼依賴，只有 UI 層自己知道；
入口層只負責「本平臺服務 ＋ 呼叫 {nameof(SetupUi)}」。這樣四份入口的差異只剩日誌與儲存區實現。

*生命週期*：{nameof(UserSession)}、{nameof(IHttpCaller)}、{nameof(ISvcUser)} 都是單例——
令牌與 HttpClient 都必須跨頁共用；Vm 則是每次建立視圖一份。
""")]
public static class DiUi {
	[Doc($$"""
	註冊 UI 層需要的全部服務，並先把共享層的服務接上。
	#Rtn[同一個 {nameof(IServiceCollection)}，便於串接。]
	""")]
	public static IServiceCollection SetupUi(
		this IServiceCollection z
	) {
		// 1: 共享層（I18n、型別元資料）。
		z.SetupCore();

		// 2: 服務端位址。默認本機；換部署時在此覆寫（或由入口層註冊自己的實現）。
		z.AddSingleton<I_GetBaseUrl>(DfltBaseUrl.Inst);

		// 3: 會話狀態與 HTTP 門面：必須單例，否則換頁就丟登錄。
		z.AddSingleton<UserSession>();
		z.AddSingleton<IHttpCaller, HttpCaller>();
		z.AddSingleton<ISvcUser, ClientUser>();

		// 4: ViewModel：每次建立視圖一份，避免上一頁的輸入殘留到下一頁。
		z.AddTransient<VmMain>();
		z.AddTransient<VmAccount>();

		return z;
	}
}
