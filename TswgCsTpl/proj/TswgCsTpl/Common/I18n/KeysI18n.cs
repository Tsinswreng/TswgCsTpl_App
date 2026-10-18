namespace TswgCsTpl.Infra.I18n;

using Tsinswreng.CsCore;
using Tsinswreng.CsI18n;

[Doc($$"""
界面文案鍵族（CsI18n 的 {{nameof(I18nKey)}} 路徑）。

*鍵與文案分離*：鍵只描述「這裡要顯示什麼」，具體文字在 `Languages/<語言>.json`。
故加一種語言只需加一個 JSON 檔，不必改任何代碼。

*路徑風格用 `.`*：語言檔是嵌套 JSON，用 `.` 拼成 `Ui.Login` 這種可讀路徑；
而異常鍵（見 `KeysErr`）用 `/` —— 兩者是不同的鍵族，刻意分開避免混淆。

*為何葉子用 {nameof(I18nKey)} 而不是裸 {nameof(Tsinswreng.CsKeyNode.KeyNode)}*：
`II18n.Get` 收的是 `II18nKey`；{nameof(I18nKey)} 是 CsI18n 提供的實現，
它本身仍以 KeyNode 的分段路徑為基礎，故鍵的形狀與配置鍵一族一致。
""")]
public static class KeysI18n {
	[Doc("造一個文案鍵。`Parent` 為 null 表示鍵樹的根。")]
	static I18nKey Mk(II18nKey? Parent, IList<str> Path) {
		return (I18nKey)I18nKey.Mk(Parent, Path);
	}

	[Doc("界面通用文案。")]
	public static class Ui {
		[Doc("本組根節點。")]
		public static I18nKey _R = Mk(null, [nameof(Ui)]);

		[Doc("應用標題。")]
		public static I18nKey AppTitle = Mk(_R, [nameof(AppTitle)]);

		[Doc("「登錄」：標籤頁標題與按鈕文字。")]
		public static I18nKey Login = Mk(_R, [nameof(Login)]);

		[Doc("「註冊」。")]
		public static I18nKey Register = Mk(_R, [nameof(Register)]);

		[Doc("「退出登錄」。")]
		public static I18nKey Logout = Mk(_R, [nameof(Logout)]);

		[Doc("「登錄名」字段標籤。")]
		public static I18nKey UniqName = Mk(_R, [nameof(UniqName)]);

		[Doc("「郵箱」字段標籤。")]
		public static I18nKey Email = Mk(_R, [nameof(Email)]);

		[Doc("「密碼」字段標籤。")]
		public static I18nKey Password = Mk(_R, [nameof(Password)]);

		[Doc("「確認密碼」字段標籤。")]
		public static I18nKey ConfirmPassword = Mk(_R, [nameof(ConfirmPassword)]);

		[Doc("「暱稱」字段標籤。")]
		public static I18nKey NickName = Mk(_R, [nameof(NickName)]);

		[Doc("「身份（登錄名或郵箱）」字段標籤。")]
		public static I18nKey Identity = Mk(_R, [nameof(Identity)]);

		[Doc("登錄成功提示。")]
		public static I18nKey LoginSucceeded = Mk(_R, [nameof(LoginSucceeded)]);

		[Doc("註冊成功提示（提示用戶接著登錄）。")]
		public static I18nKey RegisterSucceeded = Mk(_R, [nameof(RegisterSucceeded)]);

		[Doc("退出登錄成功提示。")]
		public static I18nKey LogoutSucceeded = Mk(_R, [nameof(LogoutSucceeded)]);

		[Doc("兩次輸入的密碼不一致。")]
		public static I18nKey PasswordMismatch = Mk(_R, [nameof(PasswordMismatch)]);

		[Doc("郵箱格式不合法。")]
		public static I18nKey EmailInvalid = Mk(_R, [nameof(EmailInvalid)]);

		[Doc("「某字段不可為空」。帶一個 `{0}` 佔位符，填字段名。")]
		public static I18nKey CannotBeEmpty = Mk(_R, [nameof(CannotBeEmpty)]);

		[Doc("「當前用戶」區塊標題。")]
		public static I18nKey Me = Mk(_R, [nameof(Me)]);

		[Doc("尚未登錄時的提示文字。")]
		public static I18nKey NotLogin = Mk(_R, [nameof(NotLogin)]);

		[Doc("提示「請稍後重試」這類不帶細節的錯誤文案。")]
		public static I18nKey PleaseRetryLater = Mk(_R, [nameof(PleaseRetryLater)]);
	}
}
