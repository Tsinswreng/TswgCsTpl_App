namespace TswgCsTpl.Infra.I18n;

using Tsinswreng.CsCore;
using Tsinswreng.CsCfg;
using Tsinswreng.CsI18n;
using Tsinswreng.CsKeyNode;
using Tsinswreng.CsErr;

[Doc($$"""
一條譯文：鍵 ＋ 模板。

*鍵為什麼收鍵物件而不是裸字符串*：這樣條目可以直接引用業務側宣告的鍵
（界面文案鍵或錯誤鍵）——鍵改名、刪除時**編譯就會錯**，不必另建一套只存在於翻譯檔的字符串鍵。

*為何是兩個多載而不是收一個共同父介面*：`II18nKey` 來自 CsCfg（配置鍵家族），
`IErrNode` 來自 CsErr（KeyNode 家族），兩者沒有共同的鍵介面；分開收反而讓型別檢查更精確。
""")]
public class I18nEntry {
	[Doc("規範化後的鍵字符串（見 {nameof(I18nKeys)}）。")]
	public str Key { get; set; } = "";

	[Doc("模板；`{0}`、`{1}` 為位置參數（僅數字參數，見 msg-fmt-key 規範）。")]
	public str Text { get; set; } = "";

	[Doc($$"""
	用界面文案鍵造一條譯文。
	#Prm[{{nameof(Node)}}][界面文案鍵。]
	#Prm[{{nameof(Text)}}][模板。]
	""")]
	public I18nEntry(II18nKey Node, str Text) {
		this.Key = I18nKeys.Of(Node);
		this.Text = Text;
	}

	[Doc($$"""
	用錯誤鍵造一條錯誤文案。
	#Prm[{{nameof(Node)}}][錯誤鍵。]
	#Prm[{{nameof(Text)}}][模板。]
	""")]
	public I18nEntry(IErrNode Node, str Text) {
		this.Key = I18nKeys.Of(Node);
		this.Text = Text;
	}

	[Doc("用裸字符串鍵造一條譯文（僅限鍵不在業務側宣告的場合）。")]
	public I18nEntry(str Key, str Text) {
		this.Key = Key;
		this.Text = Text;
	}
}

[Doc($$"""
把鍵物件規範化成字典鍵字符串。

兩類鍵用不同前綴區分，故不會撞在一起：
- 界面文案鍵（{nameof(II18nKey)}）→ 全路徑，例如 `Login`。
- 錯誤鍵（{nameof(IErrNode)}）→ `Error/` ＋ 錯誤鍵路徑，例如 `Error/User/PasswordNotMatch`。

*為何錯誤鍵要加前綴*：錯誤鍵族是樹狀的（按領域分組），界面文案鍵是平鋪的；
前綴讓同一個字典能同時容納兩者，也一眼看得出哪條是錯誤文案。
""")]
public static class I18nKeys {
	[Doc("錯誤文案鍵的共同前綴。")]
	public const str ErrPrefix = "Error/";

	[Doc($$"""
	取界面文案鍵的規範化字符串。
	#Prm[{{nameof(Node)}}][界面文案鍵。]
	#Rtn[字典鍵字符串。]
	""")]
	public static str Of(II18nKey Node) {
		return Node.GetFullPath(".");
	}

	[Doc($$"""
	取錯誤鍵的規範化字符串。
	#Prm[{{nameof(Node)}}][錯誤鍵。]
	#Rtn[`Error/` ＋ 錯誤路徑。]
	""")]
	public static str Of(IErrNode Node) {
		return ErrPrefix + Node.BuildFullPath();
	}
}
