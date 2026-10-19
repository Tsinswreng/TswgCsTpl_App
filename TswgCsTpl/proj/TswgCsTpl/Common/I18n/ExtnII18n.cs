namespace TswgCsTpl.Infra.I18n;

using Tsinswreng.CsCore;
using Tsinswreng.CsErr;
using Tsinswreng.CsI18n;

[Doc($$"""
{nameof(II18n)} 的錯誤鍵便捷取法。

*為何要這一層*：`II18n.Get` 只認界面文案鍵（`II18nKey`）；錯誤鍵是 KeyNode 家族的 `IErrNode`，
兩者沒有共同介面（見 {nameof(I18nEntry)} 的說明）。
故在此把錯誤鍵包成「單段文案鍵」再轉發——字典裡的鍵是同一個字符串，故查得到。
""")]
public static class ExtnII18n {
	extension(II18n z) {
		[Doc($$"""
		按錯誤鍵取文案。
		#Prm[{{nameof(Node)}}][錯誤鍵。]
		#Prm[{{nameof(Args)}}][填進模板的位置參數。]
		#Rtn[填好的文案；沒有譯文時回鍵路徑（方便看出漏翻的是哪條）。]
		""")]
		public str Get(IErrNode Node, params obj[] Args) {
			// 錯誤鍵的規範化字符串本身不含 `.`（路徑用 `/`），故包成單段鍵後查到的還是同一條目。
			var Key = (I18nKey)I18nKey.Mk(null, [I18nKeys.Of(Node)]);
			return z.Get(Key, Args);
		}
	}
}
