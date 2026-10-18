namespace TswgCsTpl.Shared.User.Models.Req;

using Tsinswreng.CsCore;

[Doc($$"""
註冊請求。

*校驗放在服務端*：客戶端也會先做一遍同樣的校驗以省一次往返，
但唯一性、密碼強度這類必須以服務端為準——客戶端的檢查只是體驗，不是安全邊界。
""")]
public class ReqRegister {
	[Doc("登錄名。必填、2–32 字元、全局唯一（未刪行內）。")]
	public str UniqName { get; set; } = "";

	[Doc("郵箱。必填、需符合郵箱格式、全局唯一（未刪行內）。")]
	public str Email { get; set; } = "";

	[Doc("明文密碼。只在此請求體中出現一次，服務端立即哈希，不落庫原文。")]
	public str Password { get; set; } = "";

	[Doc("暱稱。可空。")]
	public str? NickName { get; set; }
}

[Doc($$"""
登錄請求。

*身份用一個字段表達*：`{nameof(Identity)}` 可以是登錄名或郵箱。
不分兩個字段，是為了讓用戶不必記「我當初是用哪個註冊的」，
同時避免「登錄名與郵箱都能登錄」時出現兩條不同的認證路徑。
""")]
public class ReqLogin {
	[Doc("登錄名或郵箱。")]
	public str Identity { get; set; } = "";

	[Doc("明文密碼。")]
	public str Password { get; set; } = "";
}

[Doc("退出登錄請求。目前不需要任何參數：要撤銷的會話由請求頭裡的令牌決定。")]
public class ReqLogout {
}
