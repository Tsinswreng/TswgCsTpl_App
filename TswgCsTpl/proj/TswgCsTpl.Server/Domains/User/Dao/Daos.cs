namespace TswgCsTpl.Server.Domains.User.Dao;

using Tsinswreng.CsCore;
using Tsinswreng.CsSql;
using TswgCsTpl.Shared.User.Models.Po;

[Doc($$"""
用戶表的查詢（CsSql 構造型拼接）。

*為何用 Dao 而不是全靠 `IRepo`*：倉儲只認主鍵。按「登錄名／郵箱」這種業務鍵查、
或「取該用戶最新一條密碼」這種帶排序的查詢，用拼接器寫更直接，也省得在內存裡過濾。

*參數綁定*：`AndEq(x => x.Col, out var P)` 產出參數名與對應的佔位符，
再把值 `AddT(P, value)` 綁進去——不做字符串拼接，故沒有注入風險。
""")]
public partial class DaoUser {
	ISqlCmdMkr SqlCmdMkr { get; set; }
	ITblMgr TblMgr { get; set; }

	public DaoUser(ISqlCmdMkr SqlCmdMkr, ITblMgr TblMgr) {
		this.SqlCmdMkr = SqlCmdMkr;
		this.TblMgr = TblMgr;
	}

	[Doc("用戶表的元資料。")]
	ITable<PoUser> T => TblMgr.GetTbl<PoUser>();

	[Doc($$"""
	按登錄名查未刪用戶。
	#Prm[{{nameof(UniqName)}}][登錄名，調用方負責去空白。]
	#Rtn[用戶；不存在時 null。]
	""")]
	public async Task<PoUser?> SlctByUniqName(IDbFnCtx Ctx, str UniqName, CT Ct) {
		var Sql = T.SqlSplicer().Select("*").FromT()
			.WhereNonDel()
			.AndEq(x => x.UniqName, out var P)
			.ToSqlStr();
		var Cmd = await Ctx.PrepareToDispose(SqlCmdMkr, Sql, Ct);
		var Args = ArgDict.Mk(T).AddT(P, UniqName);
		return await Ctx.RunCmd(Cmd, Args).FirstOrDefault<PoUser>(T, Ct);
	}

	[Doc($$"""
	按郵箱查未刪用戶。
	#Prm[{{nameof(Email)}}][郵箱，調用方負責統一小寫。]
	#Rtn[用戶；不存在時 null。]
	""")]
	public async Task<PoUser?> SlctByEmail(IDbFnCtx Ctx, str Email, CT Ct) {
		var Sql = T.SqlSplicer().Select("*").FromT()
			.WhereNonDel()
			.AndEq(x => x.Email, out var P)
			.ToSqlStr();
		var Cmd = await Ctx.PrepareToDispose(SqlCmdMkr, Sql, Ct);
		var Args = ArgDict.Mk(T).AddT(P, Email);
		return await Ctx.RunCmd(Cmd, Args).FirstOrDefault<PoUser>(T, Ct);
	}

	[Doc($$"""
	按主鍵查未刪用戶。
	#Prm[{{nameof(Id)}}][用戶 Id。]
	#Rtn[用戶；不存在或已刪時 null。]
	""")]
	public async Task<PoUser?> SlctById(IDbFnCtx Ctx, IdUser Id, CT Ct) {
		var Sql = T.SqlSplicer().Select("*").FromT()
			.WhereNonDel()
			.AndEq(x => x.Id, out var P)
			.ToSqlStr();
		var Cmd = await Ctx.PrepareToDispose(SqlCmdMkr, Sql, Ct);
		var Args = ArgDict.Mk(T).AddT(P, Id);
		return await Ctx.RunCmd(Cmd, Args).FirstOrDefault<PoUser>(T, Ct);
	}

	[Doc($$"""
	取某用戶最新一條未刪密碼記錄。
	#Prm[{{nameof(UserId)}}][用戶 Id。]
	#Rtn[密碼記錄；沒有時 null（歷史上曾允許無密碼賬號）。]
	""")]
	public async Task<PoPassword?> SlctLatestPassword(IDbFnCtx Ctx, IdUser UserId, CT Ct) {
		var TblPwd = TblMgr.GetTbl<PoPassword>();
		var Sql = TblPwd.SqlSplicer().Select("*").FromT()
			.WhereNonDel()
			.AndEq(x => x.UserId, out var P)
			.OrderByDesc(x => x.DbCreatedAt)
			.ToSqlStr();
		var Cmd = await Ctx.PrepareToDispose(SqlCmdMkr, Sql, Ct);
		var Args = ArgDict.Mk(TblPwd).AddT(P, UserId);
		return await Ctx.RunCmd(Cmd, Args).FirstOrDefault<PoPassword>(TblPwd, Ct);
	}
}

[Doc("會話表的查詢。認證的熱路徑只有一條：按令牌摘要取未撤銷的會話。")]
public partial class DaoSession {
	ISqlCmdMkr SqlCmdMkr { get; set; }
	ITblMgr TblMgr { get; set; }

	public DaoSession(ISqlCmdMkr SqlCmdMkr, ITblMgr TblMgr) {
		this.SqlCmdMkr = SqlCmdMkr;
		this.TblMgr = TblMgr;
	}

	[Doc("會話表的元資料。")]
	ITable<PoSession> T => TblMgr.GetTbl<PoSession>();

	[Doc($$"""
	按令牌摘要查未刪會話（是否過期／撤銷由調用方再判）。
	#Prm[{{nameof(TokenHash)}}][令牌的 SHA-256 摘要。]
	#Rtn[會話；查無時 null。]
	""")]
	public async Task<PoSession?> SlctByTokenHash(IDbFnCtx Ctx, u8[] TokenHash, CT Ct) {
		var Sql = T.SqlSplicer().Select("*").FromT()
			.WhereNonDel()
			.AndEq(x => x.TokenHash, out var P)
			.ToSqlStr();
		var Cmd = await Ctx.PrepareToDispose(SqlCmdMkr, Sql, Ct);
		var Args = ArgDict.Mk(T).AddT(P, TokenHash);
		return await Ctx.RunCmd(Cmd, Args).FirstOrDefault<PoSession>(T, Ct);
	}
}
