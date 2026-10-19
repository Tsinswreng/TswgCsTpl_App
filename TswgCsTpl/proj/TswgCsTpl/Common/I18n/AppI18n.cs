namespace TswgCsTpl.Infra.I18n;

using System.Globalization;
using Tsinswreng.CsCfg;
using Tsinswreng.CsCore;
using Tsinswreng.CsErr;
using Tsinswreng.CsI18n;
using Tsinswreng.CsKeyNode;

[Doc($$"""
以「鍵 → 模板」字典為後端的 {{nameof(II18n)}} 實現。

*資料從哪來*：直接吃 C# 的 {nameof(I18nEntry)} 列表（見各 `Msg<語言>` 類），
不讀檔、不生成 JSON——少了轉義、少了生成步驟、也少了「源與產物漂移」這類問題。

*模板格式*：`{0}`、`{1}` 的標準複合格式（僅位置參數，見 msg-fmt-key 規範）。
""")]
public class I18nDict : II18n {
	[Doc("字典鍵 → 模板。鍵的規範化見 {nameof(I18nKeys.Of)}。")]
	public IDictionary<str, str> Map { get; set; } = new Dictionary<str, str>();

	[Doc($$"""
	界面文案鍵找不到時的兜底。默認退回路徑末段——漏翻一條文案不該讓界面崩掉，
	顯示鍵名也足以讓人看出「這裡沒翻」。
	""")]
	public OnKeyNotFound? OnKeyNotFound { get; set; } = (Self, Key, Args) => {
		var Segs = Key.GetFullPathSegs();
		return Segs.Count == 0 ? "" : Segs[^1];
	};

	[Doc($$"""
	按界面文案鍵取文案並填參數。
	#Prm[{{nameof(Key)}}][文案鍵。]
	#Prm[{{nameof(Args)}}][填進 `{0}`、`{1}` 的參數。]
	#Rtn[填好的文案；鍵不存在時走 {nameof(OnKeyNotFound)}。]
	""")]
	public str Get(II18nKey Key, params obj[] Args) {
		var K = I18nKeys.Of(Key);
		if (!Map.TryGetValue(K, out var Template)) {
			return OnKeyNotFound?.Invoke(this, Key, Args) ?? K;
		}
		return Format(Template, Args);
	}

	[Doc($$"""
	按錯誤鍵取文案並填參數。錯誤鍵的字典鍵是 `Error/` ＋ 錯誤路徑（見 {nameof(I18nKeys)}）。
	#Prm[{{nameof(Key)}}][錯誤鍵。]
	#Prm[{{nameof(Args)}}][填進模板的參數。]
	#Rtn[填好的文案；沒有譯文時回鍵路徑本身（方便一眼看出漏翻的是哪條）。]
	""")]
	public str Get(IErrNode Key, params obj[] Args) {
		var K = I18nKeys.Of(Key);
		if (!Map.TryGetValue(K, out var Template)) {
			return K;
		}
		return Format(Template, Args);
	}

	[Doc("索引器形式：`I18n[KeysI18n.Login]`。")]
	public str this[II18nKey Key] {
		get {
			return Get(Key);
		}
	}

	[Doc($$"""
	填模板參數。模板寫錯（例如佔位符的花括號沒配對）時退回原文，避免一條文案的錯炸掉整頁。
	#Prm[{{nameof(Template)}}][模板。]
	#Prm[{{nameof(Args)}}][位置參數。]
	""")]
	static str Format(str Template, obj[] Args) {
		if (Args.Length == 0) {
			return Template;
		}
		try {
			return string.Format(CultureInfo.InvariantCulture, Template, Args);
		} catch (FormatException) {
			return Template;
		}
	}

	[Doc($$"""
	由譯文條目建字典。同一個鍵出現兩次視為編程錯誤，直接拋——那一定是複製貼上沒刪。
	#Prm[{{nameof(Entries)}}][某個語言的全部條目。]
	#Rtn[可用的 {nameof(I18nDict)}。]
	#Throw[{{nameof(InvalidOperationException)}}][同一鍵重複出現時。]
	""")]
	public static I18nDict Mk(IEnumerable<I18nEntry> Entries) {
		var R = new I18nDict();
		foreach (var Entry in Entries) {
			if (R.Map.ContainsKey(Entry.Key)) {
				throw new InvalidOperationException($"譯文鍵重複：{Entry.Key}");
			}
			R.Map[Entry.Key] = Entry.Text;
		}
		return R;
	}
}

[Doc($$"""
全局 I18n 門面：不方便依賴注入的地方（視圖、靜態工具）直接用它。

*語言放哪*：{nameof(Langs)} 把語言代碼映射到譯文條目；加一種語言＝加一個 `Msg<語言>` 類 ＋ 這裡多一行。
*為何是靜態*：與 `AppLog` 同理——界面首次組裝時就需要文案，那時容器可能還沒交給 `Di`。
容器裡如需注入，註冊 `II18n` 指向 {nameof(Inst)}（見 `DiCore`）。
""")]
public static class AppI18n {
	[Doc("默認語言。真實項目宜由配置或系統語言決定。")]
	public const str DfltLang = "zh-CN";

	[Doc("語言代碼 → 譯文條目。")]
	public static IDictionary<str, IList<I18nEntry>> Langs { get; } = new Dictionary<str, IList<I18nEntry>> {
		["zh-CN"] = MsgZhCn.Entries,
		["en"] = MsgEn.Entries,
	};

	[Doc("當前語言；第一次取用時載入默認語言。")]
	public static II18n Inst { get; set; } = MkDict(DfltLang);

	[Doc($$"""
	按語言代碼造一份字典。
	#Prm[{{nameof(Lang)}}][語言代碼，例如 `zh-CN`、`en`。]
	#Rtn[該語言的 {nameof(I18nDict)}。]
	#Throw[{{nameof(ArgumentException)}}][沒有這個語言的譯文時。]
	""")]
	public static I18nDict MkDict(str Lang) {
		if (!Langs.TryGetValue(Lang, out var Entries)) {
			throw new ArgumentException($"未提供語言：{Lang}");
		}
		return I18nDict.Mk(Entries);
	}

	[Doc($$"""
	切換語言。切換後所有透過 {nameof(Inst)} 取的文案都跟著變（已渲染的界面需自行重繪）。
	#Prm[{{nameof(Lang)}}][語言代碼。]
	#Rtn[新的語言資源，便於鏈式取用。]
	""")]
	public static II18n Use(str Lang) {
		Inst = MkDict(Lang);
		return Inst;
	}

	[Doc($$"""
	覆蓋率自檢：逐語言比對「界面文案鍵全集」與實際提供的條目。

	檢查三件事：同一語言內鍵重複、譯文為空、界面文案缺譯或未登記。
	錯誤文案不在檢查範圍（那要列舉 `KeysErr` 的全部葉子，體量小、可日後用腳本補）。

	#Rtn[問題列表；空表示沒問題。]
	""")]
	public static IList<str> Verify() {
		var R = new List<str>();
		// 1: 界面文案鍵的規範化字符串全集。
		var UiKeys = new HashSet<str>();
		foreach (var Key in KeysI18n.All) {
			UiKeys.Add(I18nKeys.Of(Key));
		}
		// 2: 逐語言檢查。
		foreach (var Pair in Langs) {
			var Seen = new HashSet<str>();
			foreach (var Entry in Pair.Value) {
				if (!Seen.Add(Entry.Key)) {
					R.Add($"[{Pair.Key}] 鍵重複：{Entry.Key}");
				}
				if (string.IsNullOrWhiteSpace(Entry.Text)) {
					R.Add($"[{Pair.Key}] 譯文為空：{Entry.Key}");
				}
			}
			// 3: 缺譯。
			foreach (var Key in UiKeys) {
				if (!Seen.Contains(Key)) {
					R.Add($"[{Pair.Key}] 缺少界面文案：{Key}");
				}
			}
			// 4: 未登記的鍵（既不是界面文案鍵、也不是錯誤文案）。
			foreach (var Key in Seen) {
				if (!UiKeys.Contains(Key) && !Key.StartsWith(I18nKeys.ErrPrefix, StringComparison.Ordinal)) {
					R.Add($"[{Pair.Key}] 未登記的鍵：{Key}");
				}
			}
		}
		return R;
	}
}


