namespace TswgCsTpl.Shared.User.Svc;

using Tsinswreng.CsCore;
using TswgCsTpl.Shared.User.Models.Req;
using TswgCsTpl.Shared.User.Models.Resp;

[Doc($$"""
用戶服務契約。**服務端與客戶端共用同一個接口**：
- 服務端 `SvcUser` 直接讀寫數據庫；
- 客戶端 `ClientUser` 逐個方法轉發成 HTTP 請求。

*為何共用接口*：Ui 層只認這個接口，故「跑在本機服務端」與「跑在遠程服務端」對界面完全一樣，
寫測試時換一個假實現也不需要改視圖。代價是兩邊的方法簽名必須一模一樣（好處正是如此）。

*關於認證*：不在參數裡傳用戶身份。令牌由傳輸層攜帶（HTTP 頭），
服務端從令牌還原身份、客戶端自己保管令牌——這樣接口形狀不會因為認證方式改變而變。
""")]
public interface ISvcUser {
	[Doc($$"""
	註冊新用戶。成功後不自動登錄（需再調 {nameof(Login)}）。
	#Prm[{{nameof(Req)}}][註冊請求。]
	#Throw[{{nameof(KeysErr.User.UniqNameTaken)}}][登錄名已被佔用時。]
	#Throw[{{nameof(KeysErr.User.EmailTaken)}}][郵箱已被佔用時。]
	#Throw[{{nameof(KeysErr.Common.ArgErr__)}}][必填項缺失或格式不合法時。]
	""")]
	public Task<nil> Register(ReqRegister Req, CT Ct);

	[Doc($$"""
	登錄，成功則簽發訪問令牌。
	#Prm[{{nameof(Req)}}][登錄請求。]
	#Rtn[令牌與用戶摘要。]
	#Throw[{{nameof(KeysErr.User.PasswordNotMatch)}}][身份不存在或密碼錯誤時（刻意不區分，防用戶枚舉）。]
	""")]
	public Task<RespLogin> Login(ReqLogin Req, CT Ct);

	[Doc($$"""
	退出登錄：撤銷當前令牌。重複調用是安全的（令牌已失效時仍然成功返回）。
	#Throw[{{nameof(KeysErr.User.NotLogin)}}][請求未攜帶有效令牌時。]
	""")]
	public Task<nil> Logout(CT Ct);

	[Doc($$"""
	取當前登錄用戶的信息。
	#Rtn[用戶摘要。]
	#Throw[{{nameof(KeysErr.User.NotLogin)}}][未攜帶令牌時。]
	#Throw[{{nameof(KeysErr.User.TokenExpired)}}][令牌過期或已被撤銷時。]
	""")]
	public Task<RespUser> Me(CT Ct);
}



