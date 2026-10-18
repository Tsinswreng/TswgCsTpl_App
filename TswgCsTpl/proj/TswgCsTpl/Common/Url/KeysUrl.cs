namespace TswgCsTpl.Infra.Url;

using Tsinswreng.CsCore;

[Doc($$"""
HTTP 路由路徑常量。服務端與客戶端都引用它，故路徑只寫一次、改一處兩邊同步。

*為何不用 CsKeyNode*：這裡是「對外契約的字面路徑」，不是配置鍵；
配置鍵用 CsKeyNode（見服務端 `KeysServerCfg`），路由用常量，兩者語義不同。
""")]
public static class KeysUrl {
	[Doc("所有 API 的共同前綴。前端靜態資源與 API 都在同一個應用下，故用 `/api` 區分。")]
	public const str ApiRoot = "/api";

	[Doc("用戶相關端點。")]
	public static class User {
		[Doc("用戶端點的共同前綴。")]
		public const str Root = ApiRoot + "/user";

		[Doc("註冊。POST，匿名可調。")]
		public const str Register = Root + "/register";

		[Doc("登錄。POST，匿名可調；成功回傳令牌。")]
		public const str Login = Root + "/login";

		[Doc("退出登錄。POST，需認證；撤銷當前令牌。")]
		public const str Logout = Root + "/logout";

		[Doc("取當前用戶。GET，需認證。")]
		public const str Me = Root + "/me";
	}
}
