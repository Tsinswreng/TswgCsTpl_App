namespace TswgCsTpl.Shared.User.Models.Po;

using Tsinswreng.CsSql;
using TswgCsTpl.Shared.Base;

[Doc($$"""
用戶實體。

*唯一性*：`{nameof(UniqName)}` 與 `{nameof(Email)}` 各有一個「未刪行內唯一」的索引
（建表見服務端的表管理器）。兩者都允許為空字符串，故索引條件裡排除空值，
否則第二個「沒填郵箱」的用戶會被唯一約束擋住。
""")]
public partial class PoUser : PoBase, I_Id<IdUser> {
	[Doc("主鍵。新建實例即生成新 Id（ULID），故插入前不必另外取號。")]
	public IdUser Id { get; set; } = new();

	[Doc("登錄名。全局唯一（未刪行內），也是默認顯示名。")]
	public str UniqName { get; set; } = "";

	[Doc("暱稱。可空；空時介面顯示 `UniqName`。")]
	public str? NickName { get; set; }

	[Doc("郵箱。可空；用於登錄與找回密碼。")]
	public str Email { get; set; } = "";
}

[Doc($$"""
密碼實體：一個用戶可以有多條，取最新一條作為當前密碼。

*為何分表而不是直接把哈希放用戶表*：
改密只需追加一條並把舊的軟刪，不需要改用戶行；日後要加「密碼歷史」「強制改密」也不用遷移表結構。

*為何存算法與迭代次數*：哈希參數會隨硬件升級而上調，
舊記錄必須能用它自己的參數驗證，否則升參數等於讓所有人登不進來。
""")]
public partial class PoPassword : PoBase, I_Id<IdPassword> {
	[Doc("主鍵。")]
	public IdPassword Id { get; set; } = new();

	[Doc("所屬用戶。")]
	public IdUser UserId { get; set; } = new();

	[Doc("哈希算法。目前只有 `Pbkdf2Sha256`；加新算法時只需追加枚舉值與實現分支。")]
	public EAlgo Algo { get; set; } = EAlgo.Pbkdf2Sha256;

	[Doc("哈希結果（Base64）。*不是* 明文密碼，也不是可逆加密。")]
	public str Text { get; set; } = "";

	[Doc("PBKDF2 迭代次數。驗證時用它，而不是用當前配置值。")]
	public i32 Iterations { get; set; }

	[Doc("鹽（Base64）。每條記錄各自一個，防彩虹表。")]
	public str Salt { get; set; } = "";

	[Doc("哈希算法枚舉。值寫進數據庫，故一旦發佈不可改號。")]
	public enum EAlgo : i32 {
		[Doc("PBKDF2-HMAC-SHA256。BCL 內建、AOT 友好、無第三方依賴。")]
		Pbkdf2Sha256 = 1,
	}
}

[Doc($$"""
登錄會話實體：一次登錄簽發一個不透明令牌。

*令牌怎麼存*：只存 SHA-256 摘要，不存原文——
數據庫洩漏時攻擊者拿到的摘要不能直接當令牌用。
驗證時對傳入的令牌算同樣的摘要再查，故仍然是 O(1) 的一次索引查詢。

*退出登錄*：把 `{nameof(RevokeAt)}` 寫成當前時刻；驗證時要求它為 0。
不刪行是為了保留「這個令牌何時被撤銷」這一資訊。
""")]
public partial class PoSession : PoBase, I_Id<IdSession> {
	[Doc("主鍵。")]
	public IdSession Id { get; set; } = new();

	[Doc("所屬用戶。")]
	public IdUser UserId { get; set; } = new();

	[Doc("令牌的 SHA-256 摘要（32 位元組）。原文只出現在登錄響應的那一次。")]
	public u8[] TokenHash { get; set; } = [];

	[Doc("過期時間（UTC 毫秒）。驗證時要求 > 現在。")]
	public i64 ExpireAt { get; set; }

	[Doc("撤銷時間（UTC 毫秒）；0 表示仍有效。退出登錄時寫入。")]
	public i64 RevokeAt { get; set; }

	[Doc("設備描述。可空，僅用於排錯與「登錄設備」列表。")]
	public str? DeviceInfo { get; set; }
}
