namespace TswgCsTpl.Ui.Client;

using System.Net;
using System.Text;
using System.Text.Json;
using Tsinswreng.CsCore;
using Tsinswreng.CsErr;
using Tsinswreng.CsErr.Results;
using TswgCsTpl.Common;
using TswgCsTpl.Infra.Json;
using TswgCsTpl.Infra.Url;
using TswgCsTpl.Shared.User.Models.Req;
using TswgCsTpl.Shared.User.Models.Resp;
using TswgCsTpl.Shared.User.Svc;

[Doc($$"""
極簡 HTTP 調用門面。

*為何要它*：視圖與業務不該各自 new `HttpClient`、各自拼 URL、各自判斷錯誤包絡。
集中在一處之後，「認證頭」「錯誤包絡還原」「超時」只有一份實現。

*錯誤語義*：HTTP 通了但服務端回錯誤包絡時，拋 {nameof(TypedErr)}（鍵＋參數），
讓上層能用同一套 I18n 翻；連不通則拋 `KeysErr.Common.NetWorkErr` 等價的業務錯誤。
""")]
public interface IHttpCaller {
	[Doc($$"""
	POST 並把響應包絡還原成 {nameof(WebAns)} 的 Data。
	#Prm[{{nameof(Path)}}][相對路徑，來自 `KeysUrl`。]
	#Prm[{{nameof(Req)}}][請求體。]
	#Prm[{{nameof(Token)}}][可選訪問令牌；非空時放進 `Authorization: Bearer`。]
	#Throw[{{nameof(TypedErr)}}][服務端回了錯誤包絡時（鍵與參數原樣帶回）。]
	""")]
	Task<TResp?> Post<TReq, TResp>(str Path, TReq Req, str? Token, CT Ct) where TResp : class;

	[Doc("GET 並還原響應包絡。語義同 `Post`。")]
	Task<TResp?> Get<TResp>(str Path, str? Token, CT Ct) where TResp : class;
}

public class HttpCaller : IHttpCaller {
	[Doc("服務端基地址。預設指向本機服務端；換部署時只改這裡。")]
	public I_GetBaseUrl BaseUrl { get; set; }

	[Doc("整個應用共用的 HttpClient。HttpClient 是設計成可重用的，每次 new 會耗盡通訊埠。")]
	readonly HttpClient Http;

	public HttpCaller(I_GetBaseUrl BaseUrl) {
		this.BaseUrl = BaseUrl;
		Http = new HttpClient();
	}

	[Doc("拼絕對位址。")]
	str MkUrl(str Path) {
		var Base = BaseUrl.GetBaseUrl().TrimEnd('/');
		return Base + Path;
	}

	[Doc("建立請求；有令牌就加認證頭。")]
	HttpRequestMessage MkReq(HttpMethod Method, str Path, str? Token) {
		var Req = new HttpRequestMessage(Method, MkUrl(Path));
		if (!string.IsNullOrWhiteSpace(Token)) {
			Req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
		}
		return Req;
	}

	[Doc("把響應體解析成 `WebAns<T>`，錯誤包絡轉成 TypedErr 拋出。")]
	static TResp? ReadAns<TResp>(str Json) where TResp : class {
		// 1: 型別必須登記在 AppJsonCtx（AOT 下源生成器只認登記過的型別）。
		var Ti = AppJsonCtx.Wire.GetTypeInfo(typeof(WebAns<TResp>))
			?? throw new InvalidOperationException($"型別未登記於 AppJsonCtx：WebAns<{typeof(TResp).Name}>");
		var Ans = (WebAns<TResp>?)JsonSerializer.Deserialize(Json, Ti);
		// 2: 錯誤包絡 → 轉成業務異常，讓上層用 I18n 翻譯。
		if (Ans?.Errors is not null && Ans.Errors.Count > 0) {
			throw TypedErr.FromViews(Ans.Errors.Select(x => (ITypedErrView)x).ToList());
		}
		return Ans?.Data;
	}

	[Doc("把網絡層例外轉成業務錯誤，避免各處各自判斷 HttpRequestException。")]
	static TypedErr MkNetErr(Exception Ex) {
		// 底層例外無處可放（Exception.InnerException 只讀），故只回業務鍵；
		// 真正的網絡細節由各宿主自己的診斷日誌記錄。
		_ = Ex;
		return KeysErr.Common.NetWorkErr.ToErr();
	}

	public async Task<TResp?> Post<TReq, TResp>(str Path, TReq Req, str? Token, CT Ct)
		where TResp : class
	{
		try {
			using var Msg = MkReq(HttpMethod.Post, Path, Token);
			// 用源生成上下文序列化請求體；未登記的型別會在此拋出，而不是在 AOT 執行期神秘失敗。
			Msg.Content = new StringContent(
				JsonSerializer.Serialize(Req, typeof(TReq), AppJsonCtx.Wire.Options)
				, Encoding.UTF8
				, "application/json"
			);
			using var Resp = await Http.SendAsync(Msg, Ct);
			var Body = await Resp.Content.ReadAsStringAsync(Ct);
			if (Resp.StatusCode == HttpStatusCode.OK || (i32)Resp.StatusCode == 400) {
				return ReadAns<TResp>(Body);
			}
			throw MkNetErr(new HttpRequestException($"HTTP {(i32)Resp.StatusCode}"));
		} catch (TypedErr) {
			throw;
		} catch (Exception Ex) when (Ex is not OperationCanceledException) {
			throw MkNetErr(Ex);
		}
	}

	public async Task<TResp?> Get<TResp>(str Path, str? Token, CT Ct) where TResp : class {
		try {
			using var Msg = MkReq(HttpMethod.Get, Path, Token);
			using var Resp = await Http.SendAsync(Msg, Ct);
			var Body = await Resp.Content.ReadAsStringAsync(Ct);
			if (Resp.StatusCode == HttpStatusCode.OK || (i32)Resp.StatusCode == 400) {
				return ReadAns<TResp>(Body);
			}
			throw MkNetErr(new HttpRequestException($"HTTP {(i32)Resp.StatusCode}"));
		} catch (TypedErr) {
			throw;
		} catch (Exception Ex) when (Ex is not OperationCanceledException) {
			throw MkNetErr(Ex);
		}
	}
}

[Doc($$"""
前端會話狀態：當前令牌與用戶摘要。

*為何是獨立物件*：界面各頁都要讀「現在是誰」，把它從 Vm 裡抽出來，
換頁、重建視圖都不會丟登錄狀態；也方便日後換成持久化存儲（只改這個類）。
""")]
public class UserSession {
	[Doc("當前訪問令牌；null 表示未登錄。")]
	public str? Token { get; set; }

	[Doc("當前用戶 Id（Low64Base 字符串）。")]
	public str? UserId { get; set; }

	[Doc("當前登錄名。")]
	public str? UniqName { get; set; }

	[Doc("是否已登錄。")]
	public bool IsLogin => !string.IsNullOrWhiteSpace(Token);

	[Doc("清空登錄狀態。退出登錄時用。")]
	public nil Clear() {
		Token = null;
		UserId = null;
		UniqName = null;
		return NIL;
	}
}

[Doc("客戶端用戶服務：把共享契約 `ISvcUser` 逐個方法轉發成 HTTP 請求，並維護會話狀態。")]
public class ClientUser : ISvcUser {
	IHttpCaller Http { get; set; }
	UserSession Session { get; set; }

	public ClientUser(IHttpCaller Http, UserSession Session) {
		this.Http = Http;
		this.Session = Session;
	}

	public async Task<nil> Register(ReqRegister Req, CT Ct) {
		await Http.Post<ReqRegister, obj>(KeysUrl.User.Register, Req, null, Ct);
		return NIL;
	}

	public async Task<RespLogin> Login(ReqLogin Req, CT Ct) {
		var R = await Http.Post<ReqLogin, RespLogin>(KeysUrl.User.Login, Req, null, Ct)
			?? throw KeysErr.User.PasswordNotMatch.ToErr();
		// 登錄成功即寫入會話，之後所有請求自動帶上令牌。
		Session.Token = R.AccessToken;
		Session.UserId = R.UserId;
		Session.UniqName = R.UniqName;
		return R;
	}

	public async Task<nil> Logout(CT Ct) {
		try {
			await Http.Post<ReqLogout, obj>(KeysUrl.User.Logout, new ReqLogout(), Session.Token, Ct);
		} finally {
			// 無論服務端是否撤銷成功，本地狀態必須清掉：否則界面會顯示「已登錄」卻什麼都做不了。
			Session.Clear();
		}
		return NIL;
	}

	public async Task<RespUser> Me(CT Ct) {
		var R = await Http.Get<RespUser>(KeysUrl.User.Me, Session.Token, Ct)
			?? throw KeysErr.User.NotLogin.ToErr();
		return R;
	}
}




