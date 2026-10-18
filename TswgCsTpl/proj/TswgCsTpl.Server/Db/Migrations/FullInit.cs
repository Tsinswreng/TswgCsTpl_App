namespace TswgCsTpl.Server.Db.Migrations;

using Tsinswreng.CsCore;
using Tsinswreng.CsSql;

[Doc($$"""
全量初始化遷移：在空庫上一次建出全部結構，並寫一條 {nameof(SchemaHistory)} 記錄。

*為何「全量」而不是逐步遷移*：模板剛起步，結構還在動；用 `TblMgr.SqlMkSchema()` 由表配置直接生成 DDL，
可以保證「代碼裡配的」與「庫裡建的」永遠一致——手寫 DDL 遲早與配置漂移。
等結構穩定、開始有線上數據時，再追加 `M2026_xxxx_Xxx` 形式的增量遷移。

*為何整段包一個事務*：建表途中失敗時要麼全成、要麼全不成，
否則會留下半套結構，下一次啟動的判斷（有無歷史表）就失真了。
""")]
public class FullInit : IMigration {
	[Doc("SQL 命令製造器；由容器注入。")]
	public ISqlCmdMkr SqlCmdMkr { get; set; }

	[Doc("表管理器；DDL 由它的 `SqlMkSchema` 生成。")]
	public ITblMgr TblMgr { get; set; }

	[Doc("遷移歷史倉儲；用來記錄「已跑過這次初始化」。")]
	public IRepo<SchemaHistory, i64> RepoSchemaHistory { get; set; }

	[Doc("執行順序號（毫秒時間戳）。此值一旦發佈不可改。")]
	public i64 CreatedMs { get; set; } = 1792000000000;

	[Doc("本次遷移的全部 DDL（由表配置生成，可能包含多條語句）。")]
	public str SqlUp { get; set; } = "";

	[Doc("回滾用的 DDL。全量初始化不做回滾（空庫回滾＝刪庫），故為空。")]
	public IList<str> SqlsDown { get; } = new List<str>();

	[Doc("執行順序號；與 `IMigration` 的約定一致。")]
	public IList<str> SqlsUp { get; } = new List<str>();

	public FullInit(
		ISqlCmdMkr SqlCmdMkr
		, ITblMgr TblMgr
		, IRepo<SchemaHistory, i64> RepoSchemaHistory
	) {
		this.RepoSchemaHistory = RepoSchemaHistory.UseCsSqlTypeInfo();
		this.SqlCmdMkr = SqlCmdMkr;

		this.TblMgr = TblMgr;
		// 由表配置生成建表語句：一條 `CREATE TABLE ...;` 序列。
		SqlUp = TblMgr.SqlMkSchema();
	}

	[Doc($$"""
	把建表語句包成「在給定事務裡執行」的延遲動作。
	#Prm[{{nameof(Ctx)}}][數據庫函數上下文（已有事務）。]
	#Rtn[接受取消令牌、執行建表的函數。]
	""")]
	public async Task<Func<CT, Task<nil>>> FnMkSchema(IDbFnCtx Ctx, CT Ct) {
		var SqlCmd = await SqlCmdMkr.MkCmd(Ctx, SqlUp, Ct);
		return async (Ct) => {
			try {
				await SqlCmd.WithCtx(Ctx).All1d(Ct);
				return NIL;
			} catch (Exception e) {
				// 建表失敗時把 DDL 一起帶出來，否則只看到「關係已存在」這種無從下手的訊息。
				throw new Exception("建表失敗。SQL：\n" + SqlUp + "\n", e);
			}
		};
	}

	[Doc("初始化動作：先建表，再寫一條歷史記錄。")]
	public async Task<Func<CT, Task<nil>>> FnInit(IDbFnCtx Ctx, CT Ct) {
		var MkSchema = await FnMkSchema(Ctx, Ct);
		var InsertSchemaHistory = await RepoSchemaHistory.FnInsertManyNoPrepare(Ctx, Ct);
		return async (Ct) => {
			var History = new SchemaHistory {
				CreatedMs = CreatedMs,
				Name = nameof(FullInit),
			};
			// 順序不能顛倒：先有表才有地方寫歷史。
			await MkSchema(Ct);
			await InsertSchemaHistory([History], Ct);
			return NIL;
		};
	}

	[Doc("執行初始化（自開事務）。")]
	public async Task<nil> Up(CT Ct) {
		// 自開事務執行：不用已過時的 TxnWrapper，也讓「整段成敗」的邊界就在這裡看得見。
		return await SqlCmdMkr.RunInTxn(Ct, async (Ctx) => {
			var Fn = await FnInit(Ctx, Ct);
			return await Fn(Ct);
		});
	}

	[Doc("全量初始化不支持回滾。")]
	public Task<nil> Down(CT Ct) {
		throw new NotSupportedException("全量初始化不支持回滾：空庫回滾等於刪庫。");
	}

	[Doc("本遷移只提供整段執行的版本；增量遷移才需要逐步的 FnUpAsy。")]
	public Task<Func<CT, Task<obj>>> FnUpAsy(IDbFnCtx Ctx, CT Ct) {
		throw new NotSupportedException();
	}

	[Doc("本遷移不提供逐步回滾。")]
	public Task<Func<CT, Task<obj>>> FnDownAsy(IDbFnCtx Ctx, CT Ct) {
		throw new NotSupportedException();
	}
}

[Doc($$"""
本服務端的遷移清單。

*約定*：日後新增結構變更時，在 {nameof(UseServerMigrations)} 裡追加遷移類即可；
真正「按順序跑、跳過已跑的」由 CsSql 的 `MigrationRunner` 負責。
""")]
public static class ServerMigrations {
	extension(IMigrationMgr z) {
		[Doc("註冊全部遷移。目前只有全量初始化；增量遷移按時間順序追加在後面。")]
		public IMigrationMgr UseServerMigrations() {
			z.AddMigrationsIfAbsent([
				// 目前沒有增量遷移。新結構變更請在此追加 `new M2026_xxxx_Xxx()`。
			]);
			return z;
		}
	}
}

