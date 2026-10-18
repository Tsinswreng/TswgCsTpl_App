namespace TswgCsTpl;

using Microsoft.Extensions.DependencyInjection;
using Tsinswreng.CsCore;
using Tsinswreng.CsI18n;
using Tsinswreng.CsRefl;
using TswgCsTpl.Infra;
using TswgCsTpl.Infra.I18n;

[Doc($$"""
業務／共享程序集的依賴注入裝配。

*命名與寫法照文檔約定*：每個需要配 DI 的程序集各有 `Di<程序集>` 類，
裡面是 `Setup<程序集>(this IServiceCollection z)`；這裡是 `TswgCsTpl` 本體，故叫 {nameof(DiCore)}。

*放什麼*：只放「本程序集提供的東西」——I18n 門面、型別元資料來源。
宿主專屬的實現（儲存區、日誌內層）不屬於這裡，由各宿主自己的 `DiXxx` 註冊。

#Eg[
	```cs
	// Ui 層與服務端都會先呼叫它
	z.SetupCore();
	```
]
""")]
public static class DiCore {
	[Doc($$"""
	註冊共享層提供的服務。
	#Rtn[同一個 {nameof(IServiceCollection)}，便於串接。]
	""")]
	public static IServiceCollection SetupCore(
		this IServiceCollection z
	) {
		// 1: 界面文案的門面。注入 II18n 的地方與直接調 AppI18n.Inst 的地方指向同一份。
		z.AddSingleton<II18n>(AppI18n.Inst);

		// 2: 型別元資料（建表、倉儲映射、按名讀寫屬性都靠它）。
		z.AddSingleton<ITypeInfoSrc>(AppTypeInfoSrc.Inst);

		return z;
	}
}
