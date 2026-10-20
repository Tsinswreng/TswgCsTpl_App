namespace TswgCsTpl.Server;

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tsinswreng.CsCore;
using TswgCsTpl.Infra.Json;
using TswgCsTpl.Log;
using TswgCsTpl.Server.Db.Migrations;
using TswgCsTpl.Server.Http;
using TswgCsTpl.Server.Infra.Cfg;

[Doc($$"""
服務端進程入口。

*兩個子命令*：
- 無參數：正常啟動 HTTP 服務；
- `--migrate`：只跑遷移然後退出（部署腳本用，避免「服務起來了但表沒建」）。

*順序很要緊*：配置 → 數據源與表 → 容器 → 中間件 → 路由。
`ServerDb` 在容器建立之前取用是刻意的：表元資料必須在 `AddCsSqlPostgres` 之前配好。
""")]
public static class Program {
	[Doc("進程入口。")]
	public static async Task<nil> Main(str[] Args) {
		// 1: 日誌。宿主有終端，故用主控台。
		AppLog.Inst.InnerLogger = MkrLogger.Console(nameof(TswgCsTpl.Server));

		var Builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = Args });

		// 2: 配置門面先接上，之後 ServerDb 才能讀 Pg 鍵。
		ServerCfg.Inst.Configuration = Builder.Configuration;
		Builder.WebHost.UseKestrel(o => {
			o.ListenAnyIP(ServerCfg.Inst.GetI32(KeysServerCfg.Http.Port, 5080));
		});

		// 3: JSON：把源生成上下文放進解析器鏈首位（AOT 必需）。
		//    這裡只影響「框架自己會序列化的場合」（minimal API 的請求體綁定、ProblemDetails）；
		//    響應體一律走 HttpRtn/WireJson，用同一份 AppJsonCtx。
		Builder.Services.ConfigureHttpJsonOptions(o => {
			o.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonCtx.Default);
			o.SerializerOptions.PropertyNamingPolicy = null;
			o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		});

		// 4: 異常與業務服務。
		Builder.Services.AddExceptionHandler<GlobalErrHandler>();
		Builder.Services.AddProblemDetails();
		Builder.Services.SetupServer();

		var App = Builder.Build();
		App.UseExceptionHandler();

		// 5: 遷移子命令。
		if (Args.Contains("--migrate", StringComparer.OrdinalIgnoreCase)) {
			await App.Services.GetRequiredService<FullInit>().Up(default);
			AppLog.Info("遷移完成。");
			return NIL;
		}

		// 6: 認證 → 路由。認證只識別身份，不攔匿名請求。
		App.UseMiddleware<TokenAuthMidware>();
		CtrlrUser.Map(App.MapGroup("/"));

		AppLog.Info($"服務啟動：http://0.0.0.0:{ServerCfg.Inst.GetI32(KeysServerCfg.Http.Port, 5080)}");
		await App.RunAsync();
		return NIL;
	}
}


