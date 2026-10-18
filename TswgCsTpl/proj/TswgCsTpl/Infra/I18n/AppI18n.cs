namespace TswgCsTpl.Infra.I18n;

using System.Globalization;
using System.Reflection;
using System.Text.Json.Nodes;
using Tsinswreng.CsCore;
using Tsinswreng.CsErr;
using Tsinswreng.CsCfg;
using Tsinswreng.CsI18n;
using Tsinswreng.CsKeyNode;

[Doc($$"""
以「路徑 → 模板」字典為後端的 {{nameof(II18n)}} 實現。

*為何不用 CsI18n 自帶的 {{nameof(Tsinswreng.CsI18n.I18n)}}*：那個實現要以 `ICfgAccessor`（CsCfg）為資料來源，
而本專案這一階段不引入 CsCfg。故只沿用 CsI18n 的**接口與鍵模型**（{{nameof(II18n)}}／{{nameof(II18nKey)}}），
把資料來源換成「內嵌 JSON 攤平成字典」。

*為何內嵌 JSON*：語言資源要跟著程序集走，四個宿主（含 Android／WASM）都不必各自複製檔案。

*模板格式*：`{0}`、`{1}` 之類的標準複合格式佔位符（與 CsI18n 的 I18n 一致）。
""")]
public class I18nDict : II18n {
	[Doc("全路徑 → 模板。鍵用 {nameof(PathSep)} 連接，例如 `Ui.Login`。")]
	public IDictionary<str, str> Map { get; set; } = new Dictionary<str, str>();

	[Doc("路徑分隔符；必須與 `KeysI18n` 的 Mkr 一致。")]
	public str PathSep { get; set; } = ".";

	[Doc($$"""
	鍵找不到時的兜底。默認退回最後一段路徑——這比拋例外好：
	漏翻一條文案不該讓整個界面崩掉，而顯示鍵名也足以讓人看出「這裡沒翻」。
	""")]
	public OnKeyNotFound? OnKeyNotFound { get; set; } = (Self, Key, Args) => {
		var Segs = Key.GetFullPathSegs();
		return Segs.Count == 0 ? "" : Segs[^1];
	};

	[Doc($$"""
	取文案並填入參數。
	#Prm[{{nameof(Key)}}][文案鍵。]
	#Prm[{{nameof(Args)}}][填進 `{0}`、`{1}` 佔位符的參數。]
	#Rtn[填好的文案；鍵不存在時走 {nameof(OnKeyNotFound)}。]
	""")]
	public str Get(II18nKey Key, params obj[] Args) {
		var Full = Key.GetFullPath(PathSep);
		if (!Map.TryGetValue(Full, out var Template)) {
			return OnKeyNotFound?.Invoke(this, Key, Args) ?? "";
		}
		if (Args.Length == 0) {
			return Template;
		}
		try {
			return string.Format(CultureInfo.InvariantCulture, Template, Args);
		} catch (FormatException) {
			// 模板寫錯時退原文，避免一條文案的錯誤炸掉整頁。
			return Template;
		}
	}

	[Doc("索引器形式：`I18n[KeysI18n.Ui.Login]`。")]
	public str this[II18nKey Key] {
		get {
			return Get(Key);
		}
	}

	[Doc($$"""
	從程序集內嵌資源載入指定語言。
	#Prm[{{nameof(Lang)}}][語言代碼，例如 `zh-CN`、`en`；對應 `Languages/<語言>.json`。]
	#Rtn[載入完成的實例；找不到資源時拋例外（語言檔缺失是部署錯誤，不該靜默）。]
	""")]
	public static I18nDict FromEmbeddedResource(str Lang) {
		var Asm = typeof(I18nDict).Assembly;
		var Suffix = "." + Lang + ".json";
		// 資源名由根命名空間與目錄拼成，故用「以語言檔名結尾」匹配，避免寫死整段資源名。
		var ResName = Asm.GetManifestResourceNames()
			.FirstOrDefault(x => x.EndsWith(Suffix, StringComparison.OrdinalIgnoreCase));
		if (ResName is null) {
			throw new FileNotFoundException(
				$"內嵌語言資源不存在：{Lang}。已內嵌的有：{string.Join(", ", Asm.GetManifestResourceNames())}"
			);
		}
		using var Stream = Asm.GetManifestResourceStream(ResName)!;
		var R = new I18nDict();
		R.LoadJson(Stream);
		return R;
	}

	[Doc($$"""
	把嵌套 JSON 攤平成「路徑 → 文案」。
	#Prm[{{nameof(Stream)}}][UTF-8 JSON 流；形狀是嵌套物件。]
	""")]
	public void LoadJson(Stream Stream) {
		var Root = JsonNode.Parse(Stream) as JsonObject
			?? throw new InvalidDataException("語言檔根節點必須是 JSON 物件。");
		Flatten(Root, [], Map);
	}

	[Doc("遞歸攤平：物件繼續往下走，字符串值則以當前路徑為鍵收進字典。")]
	static void Flatten(JsonObject Obj, IList<str> Segs, IDictionary<str, str> Out) {
		foreach (var Pair in Obj) {
			var Cur = new List<str>(Segs) { Pair.Key };
			switch (Pair.Value) {
				case JsonObject Child:
					Flatten(Child, Cur, Out);
					break;
				case JsonValue Value:
					// 數值／布爾型的文案沒有意義，只收字符串。
					if (Value.TryGetValue<str>(out var S)) {
						Out[str.Join(".", Cur)] = S;
					}
					break;
			}
		}
	}
}

[Doc($$"""
全局 I18n 門面：不方便注入 I18n 的地方（視圖、靜態工具）直接用它。

*為何是靜態*：與 `AppLog` 同理——界面首次組裝時就需要文案，那時容器可能還沒交給 `Di`。
容器裡如需注入，註冊 `II18n` 指向 {nameof(Inst)} 即可。
""")]
public static class AppI18n {
	[Doc("默認語言。真實項目應改由配置或系統語言決定（服務端配置鍵 / 客戶端系統 Locale）。")]
	public const str DfltLang = "zh-CN";

	[Doc("當前語言資源；第一次取用時載入默認語言。")]
	public static II18n Inst { get; set; } = I18nDict.FromEmbeddedResource(DfltLang);

	[Doc($$"""
	切換語言。切換後所有透過 {nameof(Inst)} 取的文案都跟著變（已渲染的界面需自行重繪）。
	#Prm[{{nameof(Lang)}}][語言代碼。]
	#Rtn[新的語言資源，便於鏈式取用。]
	""")]
	public static II18n Use(str Lang) {
		Inst = I18nDict.FromEmbeddedResource(Lang);
		return Inst;
	}

	[Doc("異常鍵的文案根節點。異常鍵 `User/PasswordNotMatch` 對應文案鍵 `Error.User.PasswordNotMatch`。")]
	public static II18nKey ErrRoot { get; } = I18nKey.Mk(null, ["Error"]);

	[Doc($$"""
	把異常鍵映射到文案鍵，供界面把 {nameof(TypedErr)} 翻成用戶看得懂的句子。
	#Eg[
		```cs
		catch (TypedErr Err) {
			Show(AppI18n.Inst.Get(Err.Type.ToI18nKey(), Err.Args?.ToArray() ?? []));
		}
		```
	]
	""")]
	public static I18nKey ToI18nKey(this IErrNode ErrNode) {
		return (I18nKey)I18nKey.Mk(ErrRoot, ErrNode.BuildFullPathSegs());
	}
}


