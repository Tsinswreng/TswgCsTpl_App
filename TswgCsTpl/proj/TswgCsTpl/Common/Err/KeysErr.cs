namespace TswgCsTpl.Infra.Err;

using Tsinswreng.CsErr;
using K = Tsinswreng.CsErr.IErrNode;
using static Tsinswreng.CsErr.ErrNode;

[Doc($$"""
異常鍵族（CsErr 的 {{nameof(IErrNode)}}）。全項目拋錯一律拋這裡的鍵，不拋裸字符串。

*鍵不是文案*：鍵的用途是「可識別的失敗原因」，它會被：
1. 序列化成 {{nameof(ITypedErrView)}} 回給客戶端（只傳鍵與參數，不傳文案）；
2. 由客戶端用自己的語言資源翻譯（見 `AppI18n.ToI18nKey`）。
故鍵的路徑一旦發佈就不該改——改了等於讓舊客戶端翻不出文案。

*命名約定*：成員名結尾的 `__` 是參數佔位符，`ToErr(參數...)` 會按順序填入。
`Mk` 造出的鍵**不帶任何標籤**——這正是默認語義：公開可見的業務異常（可示予用戶、由前端翻文案）。
只有「不可示人」的少數情況才自己加 `AppErrTags.Private`（或 `AppErrTags.SysErr`）。
""")]
public static class KeysErr {
	public static class Common {
		[Doc("本組的根節點；子鍵都以它為父。")]
		public static K _R = Mk(null, [nameof(Common)]);

		[Doc($$"""
		參數不合法。佔位符填字段名。
		#Eg[`throw {nameof(ArgErr)}.ToErr(nameof(ReqLogin.Identity))`]
		""")]
		public static K ArgErr = Mk(_R, [nameof(ArgErr)]);

		[Doc("未預期的失敗。回應給用戶時只說「請稍後重試」，詳細原因看服務端日誌。")]
		public static K UnknownErr = Mk(_R, [nameof(UnknownErr)]);

		[Doc("網絡不通或服務端不可達。客戶端把 HttpRequestException 之類的底層失敗歸到這裡。")]
		public static K NetWorkErr = Mk(_R, [nameof(NetWorkErr)]);
	}

	[Doc("用戶與認證相關。")]
	public static class User {
		[Doc("本組的根節點。")]
		public static K _R = Mk(null, [nameof(User)]);

		[Doc("登錄名已被佔用。")]
		public static K UniqNameTaken = Mk(_R, [nameof(UniqNameTaken)]);

		[Doc("郵箱已被佔用。")]
		public static K EmailTaken = Mk(_R, [nameof(EmailTaken)]);

		[Doc($$"""
		身份或密碼不正確。

		*刻意不區分「用戶不存在」與「密碼錯誤」*：分開會讓攻擊者能枚舉出哪些賬號存在。
		""")]
		public static K PasswordNotMatch = Mk(_R, [nameof(PasswordNotMatch)]);

		[Doc("請求未攜帶令牌，或令牌格式不合法。")]
		public static K NotLogin = Mk(_R, [nameof(NotLogin)]);

		[Doc("令牌已過期或被撤銷。客戶端應引導重新登錄。")]
		public static K TokenExpired = Mk(_R, [nameof(TokenExpired)]);
	}
}
