namespace TswgCsTpl;

using Tsinswreng.CsErr;
using K = Tsinswreng.CsErr.IErrNode;
using static Tsinswreng.CsErr.ErrNode;

[Doc($$"""
異常鍵族（CsErr 的 {{nameof(IErrNode)}}）。全項目拋錯一律拋這裡的鍵，不拋裸字符串。

*鍵不是文案*：鍵的用途是「可識別的失敗原因」，它會被：
1. 序列化成 {{nameof(ITypedErrView)}} 回給客戶端（只傳鍵與參數，不傳文案）；
2. 由客戶端用自己的譯文翻譯：`I18n.Get(Err.Type, ...)` 會以 `Error/` ＋ 錯誤路徑查文案（見 `I18nKeys`）。
故鍵的路徑一旦發佈就不該改——改了等於讓舊客戶端翻不出文案。

*命名約定*：成員名結尾的 `__` 是參數佔位符，`ToErr(參數...)` 會按順序填入。
`Mk` 造出的鍵**不帶任何標籤**——這正是默認語義：公開可見的業務異常（可示予用戶、由前端翻文案）。
只有「不可示人」的少數情況才自己加 `AppErrTags.Private`（或 `AppErrTags.SysErr`）。
""")]
public static class KeysErr {
	[Doc($$"""
	根節點。值來自 {{nameof(ErrRoot)}}，此處只是給調用方一個好記的名字。
	""")]
	public static K _Root = ErrRoot.Node;

	[Doc("此組的根節點；子鍵都以它為父。")]
	public static class Common {
		public static K _R = Mk(ErrRoot.Node, [nameof(Common)]);

		[Doc($$"""
		參數不合法。佔位符填字段名。
		#Eg[`throw {nameof(ArgErr__)}.ToErr(nameof(ReqLogin.Identity))`]
		""")]
		public static K ArgErr__ = Mk(_R, [nameof(ArgErr__)]);

		[Doc("未預期的失敗。回應給用戶時只說「請稍後重試」，詳細原因看服務端日誌。")]
		public static K UnknownErr = Mk(_R, [nameof(UnknownErr)]);

		[Doc("網絡不通或服務端不可達。客戶端把 HttpRequestException 之類的底層失敗歸到這裡。")]
		public static K NetWorkErr = Mk(_R, [nameof(NetWorkErr)]);
	}

	[Doc("用戶與認證相關。")]
	public static class User {
		[Doc("本組的根節點。")]
		public static K _R = Mk(ErrRoot.Node, [nameof(User)]);

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

	[Doc($$"""
	全部錯誤鍵。

	*為何要這份清單*：AOT 下不能反射列舉靜態欄位，而覆蓋率自檢需要知道「錯誤文案該有哪些」；
	新增錯誤鍵時記得同步加進來。少了不會編譯錯，但自檢會報「缺少文案」。
	""")]
	public static IList<K> All { get; } = [
		Common.ArgErr__, Common.UnknownErr, Common.NetWorkErr,
		User.UniqNameTaken, User.EmailTaken, User.PasswordNotMatch, User.NotLogin, User.TokenExpired,
	];
}

[Doc($$"""
錯誤鍵樹的根。**刻意獨立成一個類型**，而不是 {{nameof(KeysErr)}} 的成員。

*為何*：分組類（{{nameof(KeysErr)}}.Common 等）的鍵要以根為父。若根長在外層 {{nameof(KeysErr)}} 上，
分組類初始化時讀根就會觸發 {{nameof(KeysErr)}} 初始化；而 {{nameof(KeysErr)}} 初始化又要讀
{{nameof(KeysErr)}}.All（內含分組成員）——此時分組類**正在**初始化中，CLR 不允許重入，
成員仍是 null，All 便永久帶着若干 null。根搬到獨立類型後，兩邊互不牽扯。
""")]
internal static class ErrRoot {
	[Doc("根鍵，路徑 `Error`。")]
	public static K Node = Mk(null, ["Error"]);
}





