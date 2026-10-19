namespace TswgCsTpl.Server.Http;

using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Routing;
using Tsinswreng.CsCore;
using Tsinswreng.CsErr;
using Tsinswreng.CsErr.Results;
using TswgCsTpl.Infra.Err;
using TswgCsTpl.Infra.Json;
using TswgCsTpl.Infra.Url;
using TswgCsTpl.Shared.User.Models.Req;
using TswgCsTpl.Shared.User.Svc;

[Doc("HTTP 響應統一包成 `WebAns<T>`（Data＋Errors），故前端只需要一種解析路徑。")]
public static class HttpRtn {
	[Doc("成功帶數據。用源生成上下文序列化，AOT 安全。")]
	public static IResult Ok<T>(T Data) {
		var Json = JsonSerializer.Serialize(
			new WebAns<T> { Data = Data }, typeof(WebAns<T>), AppJsonCtx.Wire.Options
		);
		return Results.Text(Json, "application/json; charset=utf-8");
	}

	[Doc("成功且無數據。")]
	public static IResult OkEmpty() {
		return Ok<obj>(null!);
	}
}

[Doc("用戶端點：路徑來自共享的 `KeysUrl`，故前後端不可能各記一套。")]
public static class CtrlrUser {
	[Doc("把四個端點掛上去。")]
	public static nil Map(RouteGroupBuilder R) {
		R.MapPost(KeysUrl.User.Register, Register);
		R.MapPost(KeysUrl.User.Login, Login);
		R.MapPost(KeysUrl.User.Logout, Logout);
		R.MapGet(KeysUrl.User.Me, Me);
		return NIL;
	}

	static async Task<IResult> Register(ReqRegister Req, ISvcUser Svc, CT Ct) {
		await Svc.Register(Req, Ct);
		return HttpRtn.OkEmpty();
	}

	static async Task<IResult> Login(ReqLogin Req, ISvcUser Svc, CT Ct) {
		return HttpRtn.Ok(await Svc.Login(Req, Ct));
	}

	static async Task<IResult> Logout(ISvcUser Svc, CT Ct) {
		await Svc.Logout(Ct);
		return HttpRtn.OkEmpty();
	}

	static async Task<IResult> Me(ISvcUser Svc, CT Ct) {
		return HttpRtn.Ok(await Svc.Me(Ct));
	}
}

[Doc($$"""
認證中間件：把 `Authorization: Bearer <token>` 換成請求作用域裡的 {nameof(ICurrentUser)}。

*為何不直接擋掉匿名請求*：註冊與登錄本身是匿名的；
「哪些端點需要登錄」由業務方法自己判斷（未登錄時拋 `KeysErr.User.NotLogin`），
這樣中間件只有一個職責：識別身份。
""")]
public class TokenAuthMidware {
	[Doc("請求管線的下一站。")]
	readonly RequestDelegate Next;

	public TokenAuthMidware(RequestDelegate Next) {
		this.Next = Next;
	}

	[Doc("解析令牌並繼續管線。令牌無效時不報錯，只是保持匿名。")]
	public async Task InvokeAsync(HttpContext Ctx, Domains.User.Svc.ISvcSession SvcSession, Domains.User.ICurrentUser Current) {
		const str Prefix = "Bearer ";
		var Header = Ctx.Request.Headers.Authorization.ToString();
		if (Header.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)) {
			var Token = Header[Prefix.Length..].Trim();
			var Session = await SvcSession.Validate(Token, Ctx.RequestAborted);
			if (Session is not null) {
				// 寫進作用域物件，業務服務之後只讀它。
				Current.UserId = Session.UserId;
				Current.SessionId = Session.SessionId;
			}
		}
		await Next(Ctx);
	}
}

[Doc($$"""
全局異常處理：把 {nameof(TypedErr)} 轉成「鍵＋參數」的 JSON，其餘一律 500 且不外洩內部訊息。

*為何回鍵而不是回文案*：文案屬於前端語言環境，服務端不知道用戶用什麼語言；
回鍵＋參數讓前端用同一套 I18n 翻。
""")]
public class GlobalErrHandler : IExceptionHandler {
	readonly ILogger<GlobalErrHandler> Log;

	public GlobalErrHandler(ILogger<GlobalErrHandler> Log) {
		this.Log = Log;
	}

	[Doc("處理異常。業務異常回 400，其餘 500。")]
	public async ValueTask<bool> TryHandleAsync(HttpContext Ctx, Exception Err, CT Ct) {
		Log.LogError(Err, "未處理的例外。Path={Path}; Method={Method}", Ctx.Request.Path, Ctx.Request.Method);
		// 約定：沒標 Private／SysErr 的業務異常即「可示予用戶」，原樣回 400 ＋ 錯誤鍵。
		// 非 TypedErr（真正未預期的例外）一律 500，細節只進日誌。
		if (Err is TypedErr Te
			&& !Te.Tags.Contains(AppErrTags.Private)
			&& !Te.Tags.Contains(AppErrTags.SysErr)) {
			Ctx.Response.StatusCode = 400;
			Ctx.Response.ContentType = "application/json; charset=utf-8";
			var View = (ITypedErrView)Te;
			var Json = JsonSerializer.Serialize(
				WebAns.Mk(null, [View]), typeof(IWebAns<obj>), AppJsonCtx.Wire.Options
			);
			await Ctx.Response.WriteAsync(Json, Ct);
			return true;
		}
		Ctx.Response.StatusCode = 500;
		return true;
	}
}





