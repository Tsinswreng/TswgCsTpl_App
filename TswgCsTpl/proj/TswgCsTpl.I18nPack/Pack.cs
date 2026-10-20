namespace TswgCsTpl.I18nPack;

using Tsinswreng.CsCore;
using Tsinswreng.CsI18n;

[Doc($$"""
本專案的語言包清單：有哪些語言、各自的譯文。

*為甚麼要有這一個類*：語言清單只該有一份。執行時要靠它知道「可以切到哪些語言」，
構建期的生成器要靠它知道「該導出哪幾個 JSON」。
加一種語言＝加一個 `Msg<語言>` 類 ＋ 這裏多一行。

*不會被打包進主程式*：主程式只引用 {nameof(Pack)} 裡的語言代碼，
譯文本身（這幾個 `Msg*`）只被構建期工具讀。
""")]
public static class Pack {
	[Doc("默認語言；也是啟動時顯示的語言。")]
	public const str DfltLang = "zh-CN";

	[Doc("全部語言代碼。")]
	public static IList<str> Langes { get; } = ["zh-CN", "en"];

	[Doc($$"""
	某個語言的全部譯文條目。
	#Prm[{{nameof(Lang)}}][語言代碼。]
	#Rtn[該語言的條目；沒有的語言直接拋，避免靜默回空表。]
	""")]
	public static IList<MsgEntry> EntriesOf(str Lang) {
		return Lang switch {
			"zh-CN" => MsgZhCn.Entries,
			"en" => MsgEn.Entries,
			_ => throw new ArgumentException($"未提供語言：{Lang}"),
		};
	}

	[Doc($$"""
	把某個語言的條目攤成「鍵 → 模板」。

	*為何要這一步*：{{nameof(Tsinswreng.CsI18n)}} 只認「鍵 → 模板」；
	條目的是應用側的宣告形狀，交出去之前要自己攤平。
	重複鍵在 `Dictionary.Add` 這一步就會拋，不必另外檢查。
	#Prm[{{nameof(Lang)}}][語言代碼。]
	#Rtn[字典鍵 → 模板。]
	""")]
	public static IDictionary<str, str> EntriesToMap(str Lang) {
		var R = new Dictionary<str, str>();
		foreach (var Entry in EntriesOf(Lang)) {
			R.Add(Entry.KeyStr, Entry.Text);
		}
		return R;
	}

	[Doc($$"""
	把所有語言裝成一個 {{nameof(I18nSet)}}（生成器用它做覆蓋率自檢與導出）。
	#Rtn[含全部語言的集合，默認語言已設好。]
	""")]
	public static I18nSet MkSet() {
		var R = new I18nSet { DfltLang = DfltLang };
		foreach (var Lang in Langes) {
			R.AddLang(Lang, EntriesToMap(Lang));
		}
		return R;
	}
}
