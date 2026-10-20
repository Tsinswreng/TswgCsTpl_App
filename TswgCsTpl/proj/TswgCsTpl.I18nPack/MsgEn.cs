namespace TswgCsTpl.I18nPack;

using Tsinswreng.CsCore;
using Tsinswreng.CsI18n;
using TswgCsTpl.Infra.I18n;

[Doc($$"""
英文譯文。鍵與 {{nameof(MsgZhCn)}} 完全對應；缺一條 {{nameof(I18nSet.Verify)}} 會在構建期報出來。
""")]
public static class MsgEn {
	[Doc("本語言的全部譯文。")]
	public static IList<MsgEntry> Entries { get; } = [
		// ── 界面文案 ──
		new(KeysI18n.AppTitle, "TswgCsTpl"),
		new(KeysI18n.Login, "Log in"),
		new(KeysI18n.Register, "Sign up"),
		new(KeysI18n.Logout, "Log out"),
		new(KeysI18n.UniqName, "User name"),
		new(KeysI18n.Email, "Email"),
		new(KeysI18n.Password, "Password"),
		new(KeysI18n.ConfirmPassword, "Confirm password"),
		new(KeysI18n.NickName, "Nickname"),
		new(KeysI18n.Identity, "User name or email"),
		new(KeysI18n.LoginSucceeded, "Logged in"),
		new(KeysI18n.RegisterSucceeded, "Signed up. Please log in."),
		new(KeysI18n.LogoutSucceeded, "Logged out"),
		new(KeysI18n.PasswordMismatch, "The two passwords do not match"),
		new(KeysI18n.EmailInvalid, "Invalid email address"),
		new(KeysI18n.CannotBeEmpty, "{0} is required"),
		new(KeysI18n.Me, "Current user"),
		new(KeysI18n.NotLogin, "Not logged in"),
		new(KeysI18n.PleaseRetryLater, "Please try again later"),

		// ── 錯誤文案 ──
		new(KeysErr.Common.ArgErr__, "Invalid argument: {0}"),
		new(KeysErr.Common.UnknownErr, "The server could not handle the request. Please try again later."),
		new(KeysErr.Common.NetWorkErr, "Cannot reach the server. Please check your network."),
		new(KeysErr.User.UniqNameTaken, "This user name is already taken"),
		new(KeysErr.User.EmailTaken, "This email is already registered"),
		new(KeysErr.User.PasswordNotMatch, "User name or password is incorrect"),
		new(KeysErr.User.NotLogin, "Please log in first"),
		new(KeysErr.User.TokenExpired, "Your session has expired. Please log in again."),
	];
}
