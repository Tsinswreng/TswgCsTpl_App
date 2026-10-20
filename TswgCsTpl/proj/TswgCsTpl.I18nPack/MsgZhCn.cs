namespace TswgCsTpl.I18nPack;

using Tsinswreng.CsCore;
using Tsinswreng.CsI18n;
using TswgCsTpl.Infra.I18n;

[Doc($$"""
簡體中文譯文。

*原文就寫在這裏*：C# 的原始字符串可以多行、完全不必轉義，比 JSON 好寫；
鍵一律引用 {{nameof(KeysI18n)}}／{{nameof(KeysErr)}} 的鍵物件，故鍵改名時這裏會編譯錯。

*這個專案不會隨主程式發佈*：它只被 `TswgCsTpl.LangPack`（構建期工具）引用，
工具把譯文導出成每種語言一個 JSON，執行時只載入真的用到的語言。
故「沒用到的語言佔體積、佔內存」這件事不會發生在主程式上。

*格式*：位置參數 `{0}`；默認引擎是 ICU MessageFormat，需要複數時照 ICU 語法寫。
""")]
public static class MsgZhCn {
	[Doc("本語言的全部譯文（界面文案 ＋ 錯誤文案）。")]
	public static IList<MsgEntry> Entries { get; } = [
		// ── 界面文案 ──
		new(KeysI18n.AppTitle, "TswgCsTpl"),
		new(KeysI18n.Login, "登錄"),
		new(KeysI18n.Register, "註冊"),
		new(KeysI18n.Logout, "退出登錄"),
		new(KeysI18n.UniqName, "登錄名"),
		new(KeysI18n.Email, "郵箱"),
		new(KeysI18n.Password, "密碼"),
		new(KeysI18n.ConfirmPassword, "確認密碼"),
		new(KeysI18n.NickName, "暱稱"),
		new(KeysI18n.Identity, "登錄名或郵箱"),
		new(KeysI18n.LoginSucceeded, "登錄成功"),
		new(KeysI18n.RegisterSucceeded, "註冊成功，請登錄"),
		new(KeysI18n.LogoutSucceeded, "已退出登錄"),
		new(KeysI18n.PasswordMismatch, "兩次輸入的密碼不一致"),
		new(KeysI18n.EmailInvalid, "郵箱格式不正確"),
		new(KeysI18n.CannotBeEmpty, "{0} 不可為空"),
		new(KeysI18n.Me, "當前用戶"),
		new(KeysI18n.NotLogin, "尚未登錄"),
		new(KeysI18n.PleaseRetryLater, "請稍後重試"),

		// ── 錯誤文案（鍵直接引用 KeysErr，故錯誤鍵改名這裏會編譯錯）──
		new(KeysErr.Common.ArgErr__, "參數不合法：{0}"),
		new(KeysErr.Common.UnknownErr, "伺服器暫時無法處理，請稍後重試"),
		new(KeysErr.Common.NetWorkErr, "無法連線到伺服器，請檢查網路"),
		new(KeysErr.User.UniqNameTaken, "此登錄名已被佔用"),
		new(KeysErr.User.EmailTaken, "此郵箱已被佔用"),
		new(KeysErr.User.PasswordNotMatch, "登錄名或密碼不正確"),
		new(KeysErr.User.NotLogin, "請先登錄"),
		new(KeysErr.User.TokenExpired, "登錄已過期，請重新登錄"),
	];
}
