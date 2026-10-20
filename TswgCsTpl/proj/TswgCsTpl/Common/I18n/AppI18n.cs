namespace TswgCsTpl.Infra.I18n;

using Tsinswreng.CsCore;
using Tsinswreng.CsI18n;
using TswgCsTpl.Common;

[Doc($$"""
本應用的 i18n 門面——**只剩應用自己的決定**：有哪幾種語言、默認哪一種、語言包放哪。

*語言包是構建期生成的*：譯文寫在 `TswgCsTpl.I18nPack`（不隨主程式發佈），
由 `TswgCsTpl.LangPack` 導出成每種語言一個 JSON（`Languages/<語言>.json`）。
執行時*只載入真的用到的語言*——沒用到的語言既不佔組件體積、也不佔內存。

*為何這裏還有語言清單*：選語言的界面要知道有哪些可選。
它與語言包專案的清單必須一致；構建期工具會比對，不一致就讓構建失敗。
""")]
public static class AppI18n {
	[Doc("默認語言；啟動時顯示的語言。真實項目宜由配置或系統語言決定。")]
	public const str DfltLang = "zh-CN";

	[Doc("可切換的語言代碼；必須與構建期語言包專案的清單一致。")]
	public static IList<str> Langes { get; } = ["zh-CN", "en"];

	[Doc($$"""
	語言包所在目錄：應用儲存區下的 `Languages`。
	#Rtn[目錄路徑。]
	""")]
	public static str LangPackDir {
		get {
			// 延遲解析：本類可能在 Di 裝好之前就被取用（界面首次組裝），那時只取文案，不會走到這裏。
			return Di.GetRSvc<I_GetBaseDir>().Combine("Languages");
		}
	}

	[Doc("本應用的多語言集合；語言不在時由載入器去讀 `Languages/<語言>.json`。")]
	public static I18nSet Set { get; } = MkSet();

	[Doc($$"""
	造集合並掛上「按需讀語言包」的載入器。
	#Rtn[空集合（一個語言都還沒載入）。]
	""")]
	static I18nSet MkSet() {
		var R = new I18nSet { DfltLang = DfltLang };
		R.LangLoader = Lang => I18nJson.LoadFile(PathOf(Lang));
		return R;
	}

	[Doc($$"""
	某個語言的語言包路徑。
	#Prm[{{nameof(Lang)}}][語言代碼。]
	""")]
	static str PathOf(str Lang) {
		return System.IO.Path.Combine(LangPackDir, Lang + ".json");
	}

	[Doc("當前語言，供不經容器的地方直接用。首次取用時才載入默認語言的語言包。")]
	public static II18n Inst {
		get {
			return Set.Cur;
		}
	}

	[Doc($$"""
	切換語言；該語言還沒載入時會先去讀它的語言包。已渲染的界面需自行重繪。
	#Prm[{{nameof(Lang)}}][語言代碼。]
	#Rtn[該語言的字典。]
	""")]
	public static II18n Use(str Lang) {
		return Set.Use(Lang);
	}
}
