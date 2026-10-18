namespace TswgCsTpl.Server.Infra.Cfg;

using Microsoft.Extensions.Configuration;
using Tsinswreng.CsCore;
using Tsinswreng.CsKeyNode;

[Doc($$"""
服務端配置鍵族（CsKeyNode）。

*鍵與值分離*：鍵在這裡聲明（有型別、有默認值、可跳轉），值來自 `appsettings.json` 或環境變量。
路徑分隔符用 `:`，因為 ASP.NET 的 `IConfiguration` 就用 `:` 分層——
於是 `Pg:Host` 這種鍵在 `appsettings.json` 裡就是嵌套物件，在環境變量裡就是 `Pg__Host`，兩邊都通。

*為何不用 CsCfg*：這一階段不引入 CsCfg，配置值直接由 `IConfiguration` 提供；
但鍵的形狀與 CsCfg 的鍵一致（都是 KeyNode 路徑），日後換成 CsCfg 時鍵聲明不用改。
""")]
public class KeysServerCfg : KeysDeclBase<KeysServerCfg>, IStaticKeysDecl {
	[Doc("本族共用的節點製造器；`:` 與 ASP.NET 配置的分層符一致。")]
	public static MkrKeyNode _MkrKeyNode { get; } = new() { PathSep = ":" };

	[Doc("PostgreSQL 連線配置。刻意拆成五項而不是一條連接串：環境變量更好覆寫，也不必轉義分號。")]
	public static class Pg {
		[Doc("本組根節點。")]
		public static KeyNode<obj?> _R = KeysServerCfg.Mk(null, [nameof(Pg)]);

		[Doc("主機名或 IP。默認本機。")]
		public static KeyNode<str> Host = KeysServerCfg.Mk(_R, [nameof(Host)], "localhost");

		[Doc("端口。默認 5432。")]
		public static KeyNode<i32> Port = KeysServerCfg.Mk(_R, [nameof(Port)], 5432);

		[Doc("資料庫名。")]
		public static KeyNode<str> Database = KeysServerCfg.Mk(_R, [nameof(Database)], "tswgcs_tpl");

		[Doc("資料庫用戶。")]
		public static KeyNode<str> UserId = KeysServerCfg.Mk(_R, [nameof(UserId)], "postgres");

		[Doc("資料庫密碼。生產環境應由環境變量或密鑰管理注入，不要寫進 appsettings.json。")]
		public static KeyNode<str> Password = KeysServerCfg.Mk(_R, [nameof(Password)], "");
	}

	[Doc("HTTP 服務配置。")]
	public static class Http {
		[Doc("本組根節點。")]
		public static KeyNode<obj?> _R = KeysServerCfg.Mk(null, [nameof(Http)]);

		[Doc("監聽端口。默認 5080（避開常見的 5000／8080 佔用）。")]
		public static KeyNode<i32> Port = KeysServerCfg.Mk(_R, [nameof(Port)], 5080);
	}

	[Doc("認證配置。")]
	public static class Auth {
		[Doc("本組根節點。")]
		public static KeyNode<obj?> _R = KeysServerCfg.Mk(null, [nameof(Auth)]);

		[Doc("登錄令牌有效期（天）。默認 14 天。")]
		public static KeyNode<i32> TokenExpireDays = KeysServerCfg.Mk(_R, [nameof(TokenExpireDays)], 14);

		[Doc("PBKDF2 迭代次數。只影響新註冊／改密；舊記錄用它自己存的迭代次數驗證。")]
		public static KeyNode<i32> PasswordIterations = KeysServerCfg.Mk(_R, [nameof(PasswordIterations)], 210_000);
	}
}

[Doc($$"""
配置讀取門面：把 {nameof(IConfiguration)} 與 CsKeyNode 的鍵接起來。

*為何要這層*：`IConfiguration["Pg:Host"]` 這種字符串鍵散落在各處就等於硬編碼；
統一經這裡，鍵的唯一出處是 {nameof(KeysServerCfg)}，且取值失敗有明確默認。
""")]
public class ServerCfg {
	[Doc("全局唯一實例。由 `Program` 在建容器時設定；設定前取用會拋例外。")]
	public static ServerCfg Inst { get; set; } = new();

	[Doc("ASP.NET 的配置來源（appsettings.json ＋ 環境變量 ＋ 命令列）。")]
	public IConfiguration Configuration { get; set; } = null!;

	[Doc($$"""
	讀字符串。鍵不存在時用 {nameof(Dflt)}。
	#Prm[{{nameof(Key)}}][配置鍵，來自 {nameof(KeysServerCfg)}。]
	#Prm[{{nameof(Dflt)}}][默認值。]
	""")]
	public str GetStr(IKeyNode Key, str Dflt = "") {
		return Configuration[Key.BuildFullPath(":")] ?? Dflt;
	}

	[Doc($$"""
	讀整數。缺失或格式不對時用 {nameof(Dflt)}——配置寫錯不該讓服務起不來，
	但會在日誌裡留一行警告（見 `Program`）。
	#Prm[{{nameof(Key)}}][配置鍵。]
	#Prm[{{nameof(Dflt)}}][默認值。]
	""")]
	public i32 GetI32(IKeyNode Key, i32 Dflt) {
		var Raw = Configuration[Key.BuildFullPath(":")];
		if (i32.TryParse(Raw, out var V)) {
			return V;
		}
		return Dflt;
	}

	[Doc($$"""
	組 PostgreSQL 連接串。集中在一處，免得連接串字符串散落各處。
	#Rtn[Npgsql 可用的連接串。]
	""")]
	public str PgConnStr() {
		return $"""
			Host={GetStr(KeysServerCfg.Pg.Host)}
			;Port={GetI32(KeysServerCfg.Pg.Port, 5432)}
			;Database={GetStr(KeysServerCfg.Pg.Database)}
			;Username={GetStr(KeysServerCfg.Pg.UserId)}
			;Password={GetStr(KeysServerCfg.Pg.Password)}
			""";
	}
}


