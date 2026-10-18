namespace TswgCsTpl.Server.Domains.User.Svc;

using System.Security.Cryptography;
using System.Text;
using Tsinswreng.CsCore;
using Tsinswreng.CsErr;
using Tsinswreng.CsSql;
using TswgCsTpl.Infra.Err;
using TswgCsTpl.Server.Db.Migrations;
using TswgCsTpl.Server.Domains.User.Dao;
using TswgCsTpl.Server.Infra;
using TswgCsTpl.Server.Infra.Cfg;
using TswgCsTpl.Shared.User.Models.Po;
using TswgCsTpl.Shared.User.Models.Req;
using TswgCsTpl.Shared.User.Models.Resp;
using TswgCsTpl.Shared.User.Svc;

[Doc($$"""
密碼哈希。

*算法*：PBKDF2-HMAC-SHA256（BCL 內建、AOT 安全、無第三方依賴）。
`Ngan.Dict` 用的是 Argon2id（Konscious 套件）；本模板先取「零依賴」，
要換 Argon2 時只需加一個 {nameof(PoPassword.EAlgo)} 值與對應分支，舊記錄照樣能驗（參數存在行裡）。
""")]
public interface ISvcPasswordHasher {
	[Doc("把明文密碼哈希成可存庫的記錄（未填 `UserId`，由調用方補）。")]
	PoPassword Hash(str Password);

	[Doc("按記錄自帶的算法與參數驗證明文密碼。")]
	bool Verify(str Password, PoPassword Stored);
}

public class SvcPasswordHasher : ISvcPasswordHasher {
	[Doc("鹽長度（位元組）。")]
	public const i32 SaltBytes = 16;

	[Doc("哈希長度（位元組）。")]
	public const i32 HashBytes = 32;

	[Doc("新記錄使用的迭代次數；每次哈希時讀配置，改配置即對新註冊生效。")]
	i32 Iterations => ServerCfg.Inst.GetI32(KeysServerCfg.Auth.PasswordIterations, 210_000);

	public PoPassword Hash(str Password) {
		// 1: 每條記錄一個隨機鹽，防彩虹表與「同密碼同哈希」。
		var Salt = RandomNumberGenerator.GetBytes(SaltBytes);
		// 2: 迭代次數寫進記錄，日後升參數不影響舊記錄驗證。
		var Iters = Iterations;
		var Hash = Rfc2898DeriveBytes.Pbkdf2(
			Encoding.UTF8.GetBytes(Password), Salt, Iters, HashAlgorithmName.SHA256, HashBytes
		);
		return new PoPassword {
			Id = new(),
			Algo = PoPassword.EAlgo.Pbkdf2Sha256,
			Text = Convert.ToBase64String(Hash),
			Salt = Convert.ToBase64String(Salt),
			Iterations = Iters,
		};
	}

	public bool Verify(str Password, PoPassword Stored) {
		// 未知算法一律視為驗證失敗：寧可要求重設密碼，也不要用錯算法放行。
		if (Stored.Algo != PoPassword.EAlgo.Pbkdf2Sha256) {
			return false;
		}
		try {
			var Salt = Convert.FromBase64String(Stored.Salt);
			var Expect = Convert.FromBase64String(Stored.Text);
			var Actual = Rfc2898DeriveBytes.Pbkdf2(
				Encoding.UTF8.GetBytes(Password), Salt, Stored.Iterations, HashAlgorithmName.SHA256, Expect.Length
			);
			// 定時比較：避免以響應時間推測哈希前綴。
			return CryptographicOperations.FixedTimeEquals(Actual, Expect);
		} catch (FormatException) {
			// 記錄被寫壞時當作驗證失敗，讓上層回「賬號或密碼不正確」。
			return false;
		}
	}
}

[Doc("令牌的生成與摘要。令牌本身是隨機 32 位元組；數據庫只存它的 SHA-256。")]
public interface ISvcToken {
	[Doc("生成一個新的不透明令牌（Base64URL、無填充）。")]
	str MkToken();

	[Doc("算令牌摘要；驗證與入庫都用它，故同一個令牌永遠得到同一個摘要。")]
	u8[] HashToken(str Token);
}

public class SvcToken : ISvcToken {
	[Doc("令牌熵（位元組）。32 位元組＝256 位，暴力猜測不可行。")]
	public const i32 TokenBytes = 32;

	public str MkToken() {
		var Bytes = RandomNumberGenerator.GetBytes(TokenBytes);
		// Base64URL：放進 HTTP 頭與 JSON 都不需要轉義。
		return Convert.ToBase64String(Bytes)
			.Replace('+', '-')
			.Replace('/', '_')
			.TrimEnd('=');
	}

	public u8[] HashToken(str Token) {
		return SHA256.HashData(Encoding.UTF8.GetBytes(Token));
	}
}

[Doc("已驗證的會話摘要：認證中間件把令牌換成它，再放進請求作用域。")]
public class ValidSession {
	[Doc("會話 Id（撤銷時用）。")]
	public IdSession SessionId { get; set; }

	[Doc("會話所屬用戶。")]
	public IdUser UserId { get; set; }
}

[Doc($$"""
會話（登錄令牌）的簽發、驗證與撤銷。

*職責邊界*：本類只管會話；「密碼對不對」在 {nameof(SvcUser)}。故登錄失敗與令牌無效是兩類錯誤。
""")]
public interface ISvcSession {
	[Doc("簽發一個新會話並回傳登錄響應（含令牌原文，僅此一次）。")]
	Task<RespLogin> Issue(IDbFnCtx Ctx, PoUser User, CT Ct);

	[Doc("驗證令牌；無效／過期／已撤銷時回 null（中間件據此判定匿名）。")]
	Task<ValidSession?> Validate(str Token, CT Ct);

	[Doc("撤銷指定會話（退出登錄）。重複撤銷是安全的。")]
	Task<nil> Revoke(IDbFnCtx Ctx, IdSession SessionId, CT Ct);
}

public class SvcSession : ISvcSession {
	ISqlCmdMkr SqlCmdMkr { get; set; }
	IRepo<PoSession, IdSession> RepoSession { get; set; }
	DaoSession DaoSession { get; set; }
	ISvcToken SvcToken { get; set; }

	public SvcSession(
		ISqlCmdMkr SqlCmdMkr
		, IRepo<PoSession, IdSession> RepoSession
		, DaoSession DaoSession
		, ISvcToken SvcToken
	) {
		this.SqlCmdMkr = SqlCmdMkr;
		this.RepoSession = RepoSession;
		this.DaoSession = DaoSession;
		this.SvcToken = SvcToken;
	}

	[Doc("取當前 UTC 毫秒。")]
	static i64 NowMs() {
		return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
	}

	public async Task<RespLogin> Issue(IDbFnCtx Ctx, PoUser User, CT Ct) {
		// 1: 令牌原文只在響應裡出現，庫裡只落摘要。
		var Token = SvcToken.MkToken();
		var Days = ServerCfg.Inst.GetI32(KeysServerCfg.Auth.TokenExpireDays, 14);
		var ExpireAt = DateTimeOffset.UtcNow.AddDays(Days).ToUnixTimeMilliseconds();
		var Row = new PoSession {
			Id = new(),
			UserId = User.Id,
			TokenHash = SvcToken.HashToken(Token),
			ExpireAt = ExpireAt,
			RevokeAt = 0,
		};
		await RepoSession.OrdAdd(Ctx, AsyE.One(Row), Ct);
		// 2: 回傳的摘要信息供界面直接顯示，省一次查詢。
		return new RespLogin {
			UserId = User.Id.ToString(),
			UniqName = User.UniqName,
			NickName = User.NickName,
			AccessToken = Token,
			AccessTokenExpireAt = ExpireAt,
		};
	}

	public async Task<ValidSession?> Validate(str Token, CT Ct) {
		if (string.IsNullOrWhiteSpace(Token)) {
			return null;
		}
		// 每次驗證用獨立上下文：認證發生在請求處理之前，不該與業務事務糾纏。
		var Ctx = new DbFnCtx();
		try {
			var Row = await DaoSession.SlctByTokenHash(Ctx, SvcToken.HashToken(Token), Ct);
			if (Row is null) {
				return null;
			}
			if (Row.RevokeAt != 0 || Row.ExpireAt <= NowMs()) {
				return null;
			}
			return new ValidSession { SessionId = Row.Id, UserId = Row.UserId };
		} finally {
			await ((IAsyncDisposable)Ctx).DisposeAsync();
		}
	}

	public async Task<nil> Revoke(IDbFnCtx Ctx, IdSession SessionId, CT Ct) {
		// 逐對更新：把 RevokeAt 從 0 改成當前時刻；已經非 0 的行保持原值（不覆蓋首次撤銷時間）。
		var Dict = new Dictionary<str, obj?> { [nameof(PoSession.RevokeAt)] = NowMs() };
		await RepoSession.OrdUpdByDbDict(Ctx, [SessionId], [Dict], Ct);
		return NIL;
	}
}

[Doc($$"""
用戶服務：註冊、登錄、退出登錄、取當前用戶。**實現**共享契約 `ISvcUser`。

*事務邊界*：每個寫操作各自一個事務（{nameof(ISqlCmdMkrExtn.RunInTxn)}）；
註冊會寫兩張表（用戶＋密碼），必須同生同死，故兩條 INSERT 在同一事務內。
""")]
public class SvcUser : ISvcUser {
	ISqlCmdMkr SqlCmdMkr { get; set; }
	IRepo<PoUser, IdUser> RepoUser { get; set; }
	IRepo<PoPassword, IdPassword> RepoPassword { get; set; }
	DaoUser DaoUser { get; set; }
	ISvcPasswordHasher Hasher { get; set; }
	ISvcSession SvcSession { get; set; }
	ICurrentUser Current { get; set; }

	public SvcUser(
		ISqlCmdMkr SqlCmdMkr
		, IRepo<PoUser, IdUser> RepoUser
		, IRepo<PoPassword, IdPassword> RepoPassword
		, DaoUser DaoUser
		, ISvcPasswordHasher Hasher
		, ISvcSession SvcSession
		, ICurrentUser Current
	) {
		this.SqlCmdMkr = SqlCmdMkr;
		this.RepoUser = RepoUser;
		this.RepoPassword = RepoPassword;
		this.DaoUser = DaoUser;
		this.Hasher = Hasher;
		this.SvcSession = SvcSession;
		this.Current = Current;
	}

	[Doc("登錄名規則：去空白後 2–32 字元。太短容易撞名，太長不利顯示。")]
	public const i32 UniqNameMin = 2;
	[Doc("登錄名上限。")]
	public const i32 UniqNameMax = 32;
	[Doc("密碼最短長度。長度限制比「大小寫數字符號」更有效，也更不擾民。")]
	public const i32 PasswordMin = 8;

	[Doc("郵箱格式校驗：只做「有一個 @、兩側非空、域名含點」這種務實檢查。")]
	public static bool IsEmailValid(str Email) {
		var At = Email.IndexOf('@');
		return At > 0 && At < Email.Length - 1 && Email.IndexOf('.', At) > At + 1;
	}

	[Doc($$"""
	校驗註冊請求的基本形狀（唯一性在事務內查庫）。
	#Throw[{{nameof(KeysErr.Common.ArgErr)}}][任一條規則不滿足時，參數為字段名。]
	""")]
	public static void ValidateRegister(ReqRegister Req) {
		var Name = Req.UniqName.Trim();
		if (Name.Length < UniqNameMin || Name.Length > UniqNameMax) {
			throw KeysErr.Common.ArgErr.ToErr(nameof(ReqRegister.UniqName));
		}
		if (!IsEmailValid(Req.Email.Trim())) {
			throw KeysErr.Common.ArgErr.ToErr(nameof(ReqRegister.Email));
		}
		if (Req.Password.Length < PasswordMin) {
			throw KeysErr.Common.ArgErr.ToErr(nameof(ReqRegister.Password));
		}
	}

	public async Task<nil> Register(ReqRegister Req, CT Ct) {
		ValidateRegister(Req);
		var UniqName = Req.UniqName.Trim();
		var Email = Req.Email.Trim().ToLowerInvariant();
		return await SqlCmdMkr.RunInTxn(Ct, async (Ctx) => {
			// 1: 唯一性在事務內查，配合唯一索引兜住併發註冊。
			if (await DaoUser.SlctByUniqName(Ctx, UniqName, Ct) is not null) {
				throw KeysErr.User.UniqNameTaken.ToErr();
			}
			if (await DaoUser.SlctByEmail(Ctx, Email, Ct) is not null) {
				throw KeysErr.User.EmailTaken.ToErr();
			}
			// 2: 用戶與密碼兩條記錄在同一事務內寫入。
			var User = new PoUser {
				Id = new(),
				UniqName = UniqName,
				NickName = string.IsNullOrWhiteSpace(Req.NickName) ? null : Req.NickName.Trim(),
				Email = Email,
			};
			var Pwd = Hasher.Hash(Req.Password);
			Pwd.UserId = User.Id;
			await RepoUser.OrdAdd(Ctx, AsyE.One(User), Ct);
			await RepoPassword.OrdAdd(Ctx, AsyE.One(Pwd), Ct);
			return NIL;
		});
	}

	public async Task<RespLogin> Login(ReqLogin Req, CT Ct) {
		if (string.IsNullOrWhiteSpace(Req.Identity) || string.IsNullOrEmpty(Req.Password)) {
			throw KeysErr.Common.ArgErr.ToErr(nameof(ReqLogin.Identity));
		}
		var Identity = Req.Identity.Trim();
		return await SqlCmdMkr.RunInTxn(Ct, async (Ctx) => {
			// 1: 身份可以是登錄名或郵箱；兩者都查不到時一律回「賬號或密碼不正確」（防枚舉）。
			var User = await DaoUser.SlctByUniqName(Ctx, Identity, Ct)
				?? await DaoUser.SlctByEmail(Ctx, Identity.ToLowerInvariant(), Ct);
			if (User is null) {
				throw KeysErr.User.PasswordNotMatch.ToErr();
			}
			// 2: 驗證密碼。
			var Pwd = await DaoUser.SlctLatestPassword(Ctx, User.Id, Ct);
			if (Pwd is null || !Hasher.Verify(Req.Password, Pwd)) {
				throw KeysErr.User.PasswordNotMatch.ToErr();
			}
			// 3: 簽發會話。
			return await SvcSession.Issue(Ctx, User, Ct);
		});
	}

	public async Task<nil> Logout(CT Ct) {
		// 未登錄時也沒有「當前會話」可撤銷，直接成功——退出登錄是幂等的。
		if (Current.SessionId == default(IdSession)) {
			return NIL;
		}
		return await SqlCmdMkr.RunInTxn(Ct, async (Ctx) => {
			await SvcSession.Revoke(Ctx, Current.SessionId, Ct);
			return NIL;
		});
	}

	public async Task<RespUser> Me(CT Ct) {
		if (Current.UserId == default(IdUser)) {
			throw KeysErr.User.NotLogin.ToErr();
		}
		var Ctx = new DbFnCtx();
		try {
			var User = await DaoUser.SlctById(Ctx, Current.UserId, Ct);
			if (User is null) {
				throw KeysErr.User.NotLogin.ToErr();
			}
			return new RespUser {
				UserId = User.Id.ToString(),
				UniqName = User.UniqName,
				NickName = User.NickName,
				Email = User.Email,
			};
		} finally {
			await ((IAsyncDisposable)Ctx).DisposeAsync();
		}
	}
}

