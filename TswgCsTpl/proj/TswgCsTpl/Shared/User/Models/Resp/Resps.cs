namespace TswgCsTpl.Shared.User.Models.Resp;

using Tsinswreng.CsCore;

[Doc($$"""
登錄成功響應。

*為何 Id 用字符串*：強類型 Id 的 `Value` 是 UInt128，JSON 沒有 UInt128；
對外形態一律用它的 Low64Base 字符串（22 字元、URL 安全），客戶端用 `IdUser.FromLow64Base` 還原。
這樣也避免了給每個 Id 寫一個 JSON 轉換器。
""")]
public class RespLogin {
	[Doc("用戶 Id（Low64Base 字符串）。")]
	public str UserId { get; set; } = "";

	[Doc("登錄名，供界面直接顯示，省一次查詢。")]
	public str UniqName { get; set; } = "";

	[Doc("暱稱，可空。")]
	public str? NickName { get; set; }

	[Doc("訪問令牌。之後所有需要認證的請求放在 `Authorization: Bearer <令牌>` 裡。")]
	public str AccessToken { get; set; } = "";

	[Doc("令牌過期時間（UTC 毫秒）。客戶端可據此提前提示重新登錄。")]
	public i64 AccessTokenExpireAt { get; set; }
}

[Doc("當前用戶信息。用於「我」頁面與登錄狀態校驗。")]
public class RespUser {
	[Doc("用戶 Id（Low64Base 字符串）。")]
	public str UserId { get; set; } = "";

	[Doc("登錄名。")]
	public str UniqName { get; set; } = "";

	[Doc("暱稱，可空。")]
	public str? NickName { get; set; }

	[Doc("郵箱。")]
	public str Email { get; set; } = "";
}
