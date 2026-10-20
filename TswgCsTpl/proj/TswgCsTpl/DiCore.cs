namespace TswgCsTpl;

using Microsoft.Extensions.DependencyInjection;
using Tsinswreng.CsCore;
using Tsinswreng.CsI18n;
using Tsinswreng.CsJson;
using Tsinswreng.CsRefl;
using TswgCsTpl.Infra;
using TswgCsTpl.Infra.I18n;
using TswgCsTpl.Infra.Json;

[Doc($$"""
業務／共享程序集的依賴注入裝配。

*命名與寫法照文檔約定*：每個需要配 DI 的程序集各有 `Di<程序集>` 類，
裡面是 `Setup<程序集>(this IServiceCollection z)`；這裡是 `TswgCsTpl` 本體，故叫 {nameof(DiCore)}。

*放什麼*：只放「本程序集提供的東西」——I18n 門面、型別元資料來源、JSON 序列化門面。
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

		// 3: JSON 序列化門面。注入的就是 AppJson.Inst 那一個——
		//    靜態場合（HttpRtn／GlobalErrHandler）與注入場合共用同一份 options 與轉換器，
		//    不會出現「同一段 JSON 兩條路徑寫出不同結果」。
		z.AddSingleton<IJsonSerializer>(AppJson.Inst);

		// 4: 文案覆蓋率自檢不再放這裏。
		//    語言包是構建期生成的（proj/TswgCsTpl.LangPack），漏翻／翻成空白會讓構建失敗；
		//    執行時只載入當前語言，故此刻根本看不到其他語言，跑 Verify 只會誤判。
		return z;
	}
}



