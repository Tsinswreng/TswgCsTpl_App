namespace TswgCsTpl.Infra.I18n;

using Tsinswreng.CsCore;
using Tsinswreng.CsI18n;

[Doc($$"""
界面文案鍵：**平鋪**一層，不按畫面／模塊分組。

*為何平鋪*：同一個鍵到哪裡都該是同一句譯文（msg-fmt-key 的理念）。
分組會誘使人為「登錄頁的取消」和「刪除確認的取消」各立一個鍵，最後同一句話翻出多份、改一處漏三處。
真的需要不同措辭時，正解是加參數或另立一個語義不同的鍵，而不是加前綴。

*命名*：識別符即鍵字符串，照 msg-fmt-key 規範——
顯示名稱的含義與英文譯文一致；`__` 表示參數佔位，一個 `__` 對應一個位置參數。

*鍵在哪查*：與譯文一起被 `Msg<語言>` 引用，故鍵改名會讓翻譯檔編譯錯。
""")]
public static class KeysI18n {
	[Doc("造一個鍵。目前全部是單段，故父節點傳 null。")]
	static I18nKey Mk1(str Name) {
		return I18nKey.Mk(null, [Name]);
	}

	[Doc("應用標題。")]
	public static I18nKey AppTitle = Mk1(nameof(AppTitle));

	[Doc("「登錄」。")]
	public static I18nKey Login = Mk1(nameof(Login));

	[Doc("「註冊」。")]
	public static I18nKey Register = Mk1(nameof(Register));

	[Doc("「退出登錄」。")]
	public static I18nKey Logout = Mk1(nameof(Logout));

	[Doc("「登錄名」。")]
	public static I18nKey UniqName = Mk1(nameof(UniqName));

	[Doc("「郵箱」。")]
	public static I18nKey Email = Mk1(nameof(Email));

	[Doc("「密碼」。")]
	public static I18nKey Password = Mk1(nameof(Password));

	[Doc("「確認密碼」。")]
	public static I18nKey ConfirmPassword = Mk1(nameof(ConfirmPassword));

	[Doc("「暱稱」。")]
	public static I18nKey NickName = Mk1(nameof(NickName));

	[Doc("「登錄名或郵箱」。")]
	public static I18nKey Identity = Mk1(nameof(Identity));

	[Doc("登錄成功提示。")]
	public static I18nKey LoginSucceeded = Mk1(nameof(LoginSucceeded));

	[Doc("註冊成功提示。")]
	public static I18nKey RegisterSucceeded = Mk1(nameof(RegisterSucceeded));

	[Doc("退出登錄成功提示。")]
	public static I18nKey LogoutSucceeded = Mk1(nameof(LogoutSucceeded));

	[Doc("兩次密碼不一致。")]
	public static I18nKey PasswordMismatch = Mk1(nameof(PasswordMismatch));

	[Doc("郵箱格式不合法。")]
	public static I18nKey EmailInvalid = Mk1(nameof(EmailInvalid));

	[Doc("「{0} 不可為空」。")]
	public static I18nKey CannotBeEmpty = Mk1(nameof(CannotBeEmpty));

	[Doc("「當前用戶」標題。")]
	public static I18nKey Me = Mk1(nameof(Me));

	[Doc("尚未登錄。")]
	public static I18nKey NotLogin = Mk1(nameof(NotLogin));

	[Doc("「請稍後重試」。")]
	public static I18nKey PleaseRetryLater = Mk1(nameof(PleaseRetryLater));

	[Doc($$"""
	全部界面文案鍵。

	*為何要這份清單*：NativeAOT 下不能靠反射列舉靜態欄位，
	而運行時的覆蓋率檢查（每個語言是否都翻了每個鍵）需要知道全集。
	新增鍵時記得同步加進來——漏了不會編譯錯，但檢查會報「翻譯檔裡有未知鍵」。
	""")]
	public static IList<II18nKey> All { get; } = [
		AppTitle, Login, Register, Logout, UniqName, Email, Password, ConfirmPassword,
		NickName, Identity, LoginSucceeded, RegisterSucceeded, LogoutSucceeded,
		PasswordMismatch, EmailInvalid, CannotBeEmpty, Me, NotLogin, PleaseRetryLater,
	];
}

