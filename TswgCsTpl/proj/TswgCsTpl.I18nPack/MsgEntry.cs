namespace TswgCsTpl.I18nPack;

using Tsinswreng.CsCore;
using Tsinswreng.CsI18n;
using Tsinswreng.CsKeyNode;

[Doc($$"""
一條譯文：鍵物件 ＋ 模板。

*為何由應用自己定義、而不是由 {{nameof(Tsinswreng.CsI18n)}} 提供*：
庫只認最終形態「鍵 → 模板」（`I18nDict.FromMap`）；譯文要怎麼宣告是應用的事。
寫在應用這一側，宣告時仍能引用鍵物件——鍵改名、刪除時這裏編譯不過。

*鍵字符串只在要交出去時才算*：{nameof(KeyStr)} 是計算屬性，
故它與 {nameof(Key)} 不可能不同步。
""")]
public record MsgEntry(IKeyNode Key, str Text) {
	[Doc($"字典鍵：父鏈路徑段用 {nameof(I18nDict.KeySep)} 連接。")]
	public str KeyStr => Key.BuildFullPath(I18nDict.KeySep);
}
