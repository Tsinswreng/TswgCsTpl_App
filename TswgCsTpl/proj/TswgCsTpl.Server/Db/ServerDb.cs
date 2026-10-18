namespace TswgCsTpl.Server.Db;

using Npgsql;
using Tsinswreng.CsCore;
using Tsinswreng.CsSql;
using Tsinswreng.CsSql.Postgres;
using TswgCsTpl.Infra;
using TswgCsTpl.Infra.Err;
using TswgCsTpl.Shared.Ids;
using TswgCsTpl.Server.Infra.Cfg;
using TswgCsTpl.Shared.Base;
using TswgCsTpl.Shared.User.Models.Po;

[Doc($$"""
PostgreSQL 數據源與表管理器的持有者。

*為何是單例*：`NpgsqlDataSource` 本身就是連接池，重複建立等於重複開池；
`ITblMgr` 則要在建容器之前就配好（`AddCsSqlPostgres` 需要它），故兩者都不走容器構造。

*初始化順序*：`ServerDb.Inst` → 建 DataSource → 配表（{nameof(ServerTblMgrIniter)}）。
故取 {nameof(Inst)} 就保證表元資料已就緒。
""")]
public class ServerDb {
	static ServerDb? _Inst;
	[Doc("全局唯一實例；第一次取用時完成「建池 ＋ 配表」。")]
	public static ServerDb Inst => _Inst ??= new ServerDb();

	[Doc("連接池。整個進程共用，不要自己 new NpgsqlConnection。")]
	public NpgsqlDataSource DataSource { get; set; } = null!;

	[Doc("表元資料管理器；遷移與倉儲都靠它把實體映射成表。")]
	public ITblMgr TblMgr { get; set; } = null!;

	ServerDb() {
		// 1: 建連接池。連不上時拋出的例外由 Program 統一記錄，不在這裡吞。
		var Builder = new NpgsqlDataSourceBuilder(ServerCfg.Inst.PgConnStr());
		DataSource = Builder.Build();

		// 2: 配表。順序：先註冊實體，再讓 Initer 補主鍵／映射／索引。
		TblMgr = new ServerTblMgr();
		new ServerTblMgrIniter(TblMgr).Init();
	}
}

[Doc("服務端的表管理器：方言固定為 PostgreSQL。")]
public class ServerTblMgr : PostgresTblMgr {
}

[Doc($$"""
把實體註冊成表，並補上主鍵、型別映射、索引。

*為何要手寫*：CsSql 是「構造型」ORM，表的形狀（列型別、唯一索引、軟刪列）全部由代碼聲明，
不靠反射猜測。好處是 AOT 安全、生成的 DDL 可預期；代價是每張表都要寫一段。

#Eg[
	```cs
	var Tbl = Mk<PoUser>("User");
	Mgr.AddTbl(Tbl.Tbl);
	PoBaseTblCfg.CfgPoBase(Tbl);
	Tbl.Tbl.Col(nameof(PoUser.Id)).MapType(IdUser.MkTypeMapFn());
	```
]
""")]
public class ServerTblMgrIniter {
	[Doc("目標表管理器。")]
	public ITblMgr Mgr { get; set; }

	public ServerTblMgrIniter(ITblMgr Mgr) {
		this.Mgr = Mgr;
	}

	[Doc($$"""
	按實體型別與數據表名建出表配置器。
	#Prm[{{nameof(DbTblName)}}][數據庫裡的表名；刻意與 C# 型別名分開，改名時可自行決定。]
	#Rtn[可繼續 `.Col(...)`／`.Idx(...)` 的配置器。]
	""")]
	public static ITblSetter<T> Mk<T>(str DbTblName)
		where T : new()
	{
		return Table.FnSetTbl<T>(AppTypeInfoSrc.Inst)(DbTblName);
	}

	[Doc("配好全部表。可重複呼叫（`Mgr.AddTbl` 覆蓋同名項），但正常只在啟動時跑一次。")]
	public nil Init() {
		// 1: 遷移歷史表由 CsSql 提供，先註冊它，遷移執行器才有地方記版本。
		Mgr.AddTbl(new SchemaHistoryTblMkr().MkTbl());

		// 2: 用戶表。
		var TblUser = Mk<PoUser>("User");
		Mgr.AddTbl(TblUser.Tbl);
		{
			var o = TblUser;
			PoBaseTblCfg.CfgPoBase(o);
			o.Tbl.Col(nameof(PoUser.Id)).MapType(IdUser.MkTypeMapFn());

			// 登錄名：未刪行內唯一，且排除空字符串——否則第二個「沒填」的用戶會撞約束。
			var UxUniqName = new OptMkIdx {
				Unique = true,
				Where = o.Tbl.SqlIsNonDel()
					+ $" AND {o.Tbl.QtCol(nameof(PoUser.UniqName))} <> ''",
			};
			o.Idx(UxUniqName, [nameof(PoUser.UniqName)]);

			// 郵箱：同上。
			var UxEmail = new OptMkIdx {
				Unique = true,
				Where = o.Tbl.SqlIsNonDel()
					+ $" AND {o.Tbl.QtCol(nameof(PoUser.Email))} <> ''",
			};
			o.Idx(UxEmail, [nameof(PoUser.Email)]);
		}

		// 3: 密碼表。查詢路徑是「按用戶取最新一條」，故給 (UserId) 索引。
		var TblPassword = Mk<PoPassword>("Password");
		Mgr.AddTbl(TblPassword.Tbl);
		{
			var o = TblPassword;
			PoBaseTblCfg.CfgPoBase(o);
			o.Tbl.Col(nameof(PoPassword.Id)).MapType(IdPassword.MkTypeMapFn());
			o.Tbl.Col(nameof(PoPassword.UserId)).MapType(IdUser.MkTypeMapFn());
			o.Tbl.Col(nameof(PoPassword.Algo)).MapEnumToInt32<PoPassword.EAlgo>();
			o.IdxExpr(null, x => x.UserId);
		}

		// 4: 會話表。認證的熱路徑是「按令牌摘要查一條」→ 摘要必須唯一且有索引。
		var TblSession = Mk<PoSession>("Session");
		Mgr.AddTbl(TblSession.Tbl);
		{
			var o = TblSession;
			PoBaseTblCfg.CfgPoBase(o);
			o.Tbl.Col(nameof(PoSession.Id)).MapType(IdSession.MkTypeMapFn());
			o.Tbl.Col(nameof(PoSession.UserId)).MapType(IdUser.MkTypeMapFn());
			// 摘要本身是 bytea，不需要額外型別映射。
			o.Tbl.Col(nameof(PoSession.TokenHash));
		}

		return NIL;
	}
}


