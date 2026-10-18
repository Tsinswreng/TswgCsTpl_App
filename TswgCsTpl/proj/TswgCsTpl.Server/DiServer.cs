namespace TswgCsTpl.Server;

using Microsoft.Extensions.DependencyInjection;
using Tsinswreng.CsCore;
using Tsinswreng.CsRefl;
using Tsinswreng.CsSql;
using Tsinswreng.CsSql.Postgres;
using Tsinswreng.CsSql.Postgres.Di;
using TswgCsTpl.Infra;
using TswgCsTpl.Server.Db;
using TswgCsTpl.Server.Db.Migrations;
using TswgCsTpl.Server.Domains.User;
using TswgCsTpl.Server.Domains.User.Dao;
using TswgCsTpl.Server.Domains.User.Svc;
using TswgCsTpl.Shared.User.Models.Po;
using TswgCsTpl.Shared.User.Svc;

[Doc($$"""
服務端的依賴注入裝配（程序集 `TswgCsTpl.Server` → {nameof(DiServer)}）。

*寫法照文檔約定*：`public static class DiXxx` ＋ `SetupXxx(this IServiceCollection z)`，回傳同一個集合便於串接。

*一處註冊、可預期*：CsSql 的固定配方（連接池／命令／事務／表管理器）由
{{nameof(CsSqlPostgresDiExtn.AddCsSqlPostgres)}} 一次註冊；其餘是「本服務端有什麼」——
每個實體一個倉儲、每個領域一個 Dao／Svc。
""")]
public static class DiServer {
	[Doc($$"""
	註冊服務端全部服務。
	#Rtn[同一個 {nameof(IServiceCollection)}，便於串接。]
	#Note[配置不當參數傳入：`ServerCfg.Inst.Configuration` 由 `Program` 在建容器前設好，
	`ServerDb.Inst` 因此能自行讀出連接串。]
	""")]
	public static IServiceCollection SetupServer(
		this IServiceCollection z
	) {
		// 0: 共享層（I18n、型別元資料）。
		z.SetupCore();

		// 1: CsSql 固定配方。DataSource 與 TblMgr 來自 ServerDb 單例（首次取用時已配好表）。
		z.AddCsSqlPostgres(new PostgresCfg {
			DataSource = ServerDb.Inst.DataSource,
			TblMgr = ServerDb.Inst.TblMgr,
		});

		// 2: 遷移設施。
		z.AddScoped<IRepo<SchemaHistory, i64>, SqlRepo<SchemaHistory, i64>>();
		z.AddScoped<IMigrationMgr>(Sp => new MigrationMgr(
			Sp.GetRequiredService<ITblMgr>(),
			Sp.GetRequiredService<ISqlCmdMkr>()
		).UseServerMigrations());
		z.AddTransient<MigrationRunner>();
		z.AddTransient<FullInit>();

		// 3: 用戶領域：每個實體一個倉儲（AOT 下不能泛型反射，只能逐個註冊）。
		z.AddScoped<IRepo<PoUser, IdUser>, SqlRepo<PoUser, IdUser>>();
		z.AddScoped<IRepo<PoPassword, IdPassword>, SqlRepo<PoPassword, IdPassword>>();
		z.AddScoped<IRepo<PoSession, IdSession>, SqlRepo<PoSession, IdSession>>();
		z.AddScoped<DaoUser>();
		z.AddScoped<DaoSession>();

		// 4: 用戶領域服務。
		z.AddScoped<ISvcPasswordHasher, SvcPasswordHasher>();
		z.AddScoped<ISvcToken, SvcToken>();
		z.AddScoped<ISvcSession, SvcSession>();
		z.AddScoped<ISvcUser, SvcUser>();
		z.AddScoped<ICurrentUser, CurrentUser>();

		return z;
	}
}
