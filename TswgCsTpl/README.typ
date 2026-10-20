使用此模板

複製整個 Cs/ 目錄
```sh
dotnet script Init.csx <NewProjectName>
```

Init.csx 會一次完成：

- 把 TswgCsTpl 換成 <NewProjectName>（命名空間與專案名，含目錄/檔案/sln）
- 刪除模板自帶的 .git
- 重新 git init

維護腳本

proj/TswgCsTpl.Scripts 是模板附帶的維護腳本項目：

- dispatcher：`Program.cs` 依首個引數分派到各命令
- 上下文：dispatcher 統一建立 `ISCtx`（`RootDir`、`Tfm`）傳給子命令
- 命令直接跑 `TswgCsTpl.Test` 專案，不依賴專案名

已內建命令

- `Test`：以 `dotnet run` 直接運行 TswgCsTpl.Test（非 AOT 的快速驗證）
- `TestAotWin`：以 win-x64 NativeAOT 發布 TswgCsTpl.Test 後運行發布物
- `GenAppJsonCtx`：掃描 `IAppSerializable` 的實現者，重寫
  `proj/TswgCsTpl/Tsinswreng.SrcGen/TswgCsTpl.Gen.g.cs`（見「JSON 序列化與源碼生成」章）。
  改了 DTO／實體／強類型 Id 之後手動跑一次。

用法（在模板根目錄）：
```sh
dotnet run --project proj/TswgCsTpl.Scripts/TswgCsTpl.Scripts.csproj -- Test
dotnet run --project proj/TswgCsTpl.Scripts/TswgCsTpl.Scripts.csproj -- TestAotWin
dotnet run --project proj/TswgCsTpl.Scripts/TswgCsTpl.Scripts.csproj -- GenAppJsonCtx
```

在 Scripts 專案內新增命令時：

- 新增 `Xxx.cs`（partial class + `partial Task Main(ISCtx Ctx, CT Ct)` 聲明）與 `Xxx.Impl.cs`（實現）
- 在 `Program.cs` 的 switch 與 PrintUsage 中加入分派



#import "@preview/tsinswreng-auto-heading:0.1.0": auto-heading
#let H = auto-heading
\-\-\-

name: tsinswreng-csharp-proj-setup

description: 如何初始化C\#項目。只有用戶要求你閱讀此skill時纔看

\-\-\-

項目一般分成 庫項目 和 業務項目 兩種。
庫項目沒甚麼好說的。
這裏主要說 業務項目

#H[待辦標記][
	見
	#link("TswgCsTpl/proj/TswgCsTpl/Todo.cs")。
	未I18n的UI文字、異常內容等都用此
]

#H[依賴注入][
	#H[全局依賴注入源][
		必須要有一個全局的類 叫 Di。
		見#link("TswgCsTpl/proj/TswgCsTpl/Di.cs")。
		`Di.SvcProvider`即整個項目最源頭的服務提供源。
		設計的理由:
		- 作爲全局的統一`SvcProvider`源
		- 方便在[不方便構造函數注入]的場合使用`Di.GetRSvc<>()`獲取依賴, 寫法簡便
	]

	#H[依賴注入裝配][
		依賴注入裝配統一寫到一處
		
		例:
```cs
namespace TswgCsTpl.Ui;

public static class DiUi{
	public static IServiceCollection SetupUi(
		this IServiceCollection z
	){
		z.AddTransient<VmAddWord>();
		z.AddTransient<VmLearnWords>();
		z.AddTransient<VmLoginRegister>();
		...
	}
}
```
每個需要配DI的程序集都寫。
如 是`TswgCsTpl.Windows`就定義`DiWindows`

		本模板的對應:
		+ `TswgCsTpl`（共享層）→ `DiCore.SetupCore`（I18n 門面、型別元資料）
		+ `TswgCsTpl.Server` → `DiServer.SetupServer`（ORM、倉儲、用戶領域服務）
		+ `TswgCsTpl.Ui` → `DiUi.SetupUi`（內部先呼叫 `SetupCore`）
		+ 四個宿主 → `DiWindows`／`DiLinux`／`DiAndroid`／`DiBrowser`，各自 `SetupXxx`；
		入口的 `MkServices` 只剩「`new ServiceCollection()` ＋ `Svc.SetupXxx()` ＋ `BuildServiceProvider()`」

		寫法就是傳統的 `this IServiceCollection z` 擴展方法, 不是 `extension(...)` 塊;
		回傳同一個集合以便串接。
	]
]

#H[日誌][
	在每個項目中定義一個`AppLog`作爲日誌門面
	```cs
	public class AppLog:DelegatingLogger {
		public static AppLog Inst => field??=new AppLog();
	}
	```
	其中`DelegatingLogger`是`ILogger`子類型,
	來自`Tsinswreng.CsLog`
	在程序入口處爲`AppLog.Inst`初始化。
	然後`AddSingleton<ILogger>(AppLog.Inst)`,
	方便依賴注入時就注入ILogger,
	不方便依賴注入時就直接調用全局可用的`AppLog.Inst`。
	優先用便攜的擴展方法 在`AppLogExtn`中,
	不用每次都`.Inst`。

	各宿主的內層實現:
	+ Windows: `MkrLogger.ConsoleEtFile`
	（主控台 ＋ 檔案雙寫。桌面是 WinExe, 雙擊啟動時沒有終端,
	故從終端跑能當場看到、雙擊啟動則留下 `logs/<應用名>.log` 事後查。
	參照 `Ngan.Dict` 的 Console ＋ File 雙 sink; 本模板不引入 Serilog,
	用 `AddConsole()` ＋ 自製 `FileLoggerProvider` 達到同樣效果）
	+ Android: `AndroidLogger`（logcat, 固定 tag 便於單獨過濾）
	+ Linux／Browser: `MkrLogger.Console`（標準輸出; WASM 下 stdout 被導向開發者工具）

	共用基礎依賴（CsSql／CsKeyNode／CsErr／CsI18n／CsRefl／CsU128Id）與強類型 Id 的
	源生成器、模板, 一律寫在 `proj/Directory.Build.props`, 各 csproj 不再重複聲明。
]

#H[異常與標籤約定][
	用 `Tsinswreng.CsErr`庫。
	參見
	- KeysErr
	- AppErrTags

	+ 默認＝公開可見的業務異常：`ErrNode.Mk` 造出的鍵不帶標籤，前端會翻成文案給用戶看
	+ 只有「不可示人」的情況才自己加 `AppErrTags.Private`／`AppErrTags.SysErr`；
	`GlobalErrHandler` 會把帶這兩個標籤的錯換成 `Error/Common/UnknownErr`，詳情只進日誌
	+ 新增錯誤鍵分組時，父節點要用 `ErrRoot.Node`，*不要*用 `KeysErr._Root`：
	分組類初始化時讀外層類的成員會觸發外層類初始化，而外層類初始化又要讀 `KeysErr.All`（內含分組成員），
	CLR 不允許重入，`All` 就會永久帶着 null。根搬到獨立類型 `ErrRoot` 就是為了斷開這個環
]

#H[界面文案與譯文][
	分四層，職責不混：
	+ 鍵：`proj/TswgCsTpl/Common/I18n/KeysI18n.cs`（界面文案，*平鋪一層*）與根目錄 `KeysErr.cs`（錯誤，按領域分組）
	+ 譯文：`proj/TswgCsTpl.I18nPack`——*不隨主程式發佈*，只被構建期工具引用
	+ 生成：`proj/TswgCsTpl.LangPack`——構建期把譯文導出成每種語言一個 JSON，並做覆蓋率自檢
	+ 執行時：`AppI18n` 只載入*當前*語言的 JSON（`Languages/<語言>.json`）

	#H[為甚麼不在主程式裡寫死所有語言][
		把每種語言的譯文都編進組件，沒用到的語言也佔發佈體積（字串進 metadata，UTF-16）
		與內存（靜態初始化就把每個語言的字典建好）。改成分檔之後：體積只算主程式，
		內存只算真的切過去的語言。
	]

	#H[怎麼運作][
		+ 條目型別由*應用自己*定義：`proj/TswgCsTpl.I18nPack/MsgEntry.cs`（`record MsgEntry(鍵物件, 模板)`）；
		`Tsinswreng.CsI18n` 只認「鍵 → 模板」，不提供條目型別，故由 `Pack.EntriesToMap(lang)` 攤成字典再 `AddLang`
		+ 譯文每條寫成 `new(KeysI18n.Login, "登錄")`——鍵直接引用鍵物件，*改名就編譯錯*
		+ 錯誤文案同理：`new(KeysErr.User.PasswordNotMatch, "登錄名或密碼不正確")`；`KeysErr` 立根於 `Error`，
		故錯誤鍵本身就是 i18n 鍵（`Error/User/PasswordNotMatch`）
		+ 生成器先跑 `Verify(KeysI18n.All, KeysErr.All)`（界面文案一組、錯誤文案一組），缺譯或空譯文就*讓構建失敗*
		+ 生成器還會比對「主程式 `AppI18n.Langes`」與「語言包專案的清單」，不一致就失敗（防「選得到、載不到」）
		+ 四個宿主 csproj 都 `<Import Project="..\..\LangPack.targets" />`：構建時先跑生成器寫到
		`proj/_gen/Languages`，再複製成 `$(OutDir)Languages`
		+ 手動跑：`dotnet run --project proj/TswgCsTpl.LangPack -- <輸出目錄>`
		+ 切換語言：`AppI18n.Use("en")`——該語言還沒載入時才去讀它的 JSON；已渲染的界面需自行重繪
		+ 模板格式：ICU MessageFormat（位置參數 `{0}` 照常，需要時可寫複數）
		+ 鍵那側的參數佔位照 msg-fmt-key 規範：一個 `__` 對應一個位置參數（例：`Select__From__`）
	]
]

#H[基本存儲區路徑][
	對于[不運行在瀏覽器環境中]的程序,
	要爲應用儲存區目錄做抽像:
	#link("TswgCsTpl/proj/TswgCsTpl/Common/I_GetBaseDir.cs")

	實現參考:
	+ 桌面（可執行檔所在目錄）:
	#link("TswgCsTpl/proj/TswgCsTpl/Common/AppDirBaseDirMgr.cs")
	+ Android（應用的外部私有目錄）:
	#link("TswgCsTpl/proj/Frontend/TswgCsTpl.Android/ExternalFilesBaseDirMgr.cs")
	+ Browser（沒有檔案系統, 明確拋出）:
	#link("TswgCsTpl/proj/TswgCsTpl/Common/NoBaseDirMgr.cs")

	四個入口都要在建立容器時把它註冊進容器,
	否則 `Di.GetRSvc<I_GetBaseDir>()` 會直接拋。
]

#H[基 遠端Api Url][
```cs
public partial interface I_GetBaseUrl {
	public str GetBaseUrl();
}
```
與`I_GetBaseDir`近似
]




CsErr2



Avalonia:

App.cs
IMk
App.DiOrMk


#H[JSON 序列化與源碼生成][
	一條鏈路、三層職責，全部圍着「NativeAOT 下沒有反射式序列化」這件事：

	+ 標記：`proj/TswgCsTpl/Common/Json/IAppSerializable.cs`——
	實現它 ＝ 這個型別要能被 JSON 源生成器認識。打在公共介面上（`IPoBase`、`IIdUInt128`）比逐個類打更省。
	+ 生成：`proj/TswgCsTpl.Scripts` 的 `GenAppJsonCtx`——用 Roslyn（MSBuildWorkspace）載入
	`proj/TswgCsTpl/TswgCsTpl.csproj`，掃標記接口的實現者，產出
	`proj/TswgCsTpl/Tsinswreng.SrcGen/TswgCsTpl.Gen.g.cs`（`[JsonSerializable]` ＋ 轉換器清單）。
	生成物*要入庫*，否則模板第一次複製出來編不過。
	+ 執行期：`AppJsonCtx`（選項 ＋ 轉換器掛載）、`AppJson`（靜態門面，`AppJson.Inst`）、
	`WireJson`（響應包絡的編解碼）。業務代碼不直接碰 `System.Text.Json` 的靜態類。

	#H[怎麼用][
		+ 需要序列化／反序列化：注入 `IJsonSerializer`（`DiCore.SetupCore` 已註冊），
		拿不到注入的場合用 `AppJson.Inst`；兩者是*同一個實例*。
		+ 需要寫端點：`HttpRtn.Ok(data)`；要回錯誤包絡：`WireJson.Err([view])`；
		前端還原：`WireJson.ReadAns<TResp>(body)`（包絡有錯就拋 `TypedErr`）。
		+ 加了新的 DTO／實體／強類型 Id：先讓它實現 `IAppSerializable`（或掛到已打標記的介面上），
		再跑一次生成器：
		`dotnet run --project proj/TswgCsTpl.Scripts -- GenAppJsonCtx`
		+ 外部型別（`WebAns<T>` 這種泛型閉包）無法打標記，改在生成器的 `ExtraJsonTypes` 手動列出。
	]

	#H[為甚麼實體（Po）也要進 JSON 上下文][
		ORM 那條路徑（建表、DbDict⇄實體映射、按名讀寫）走 `ITypeInfoSrc`，
		而 `AppTypeInfoSrc` 的第一條來源是 `JsonTypeInfoSrc`——只有*登記過的型別*才答得出來，
		沒登記就落到反射來源。反射在 NativeAOT 下成員元資料被裁掉，等於 ORM 直接崩。
		故 `IPoBase : IAppSerializable`，實體與 Id 都必須登記。
	]

	#H[強類型 Id 為甚麼不會變成 {"Value": …}][
		`proj/UInt128Template.typedid` 讓每個 Id 實作 `IRawValueSerializable`，
		由 `Tsinswreng.CsJson` 的 `JsonConvtr<T>` 把它寫成裸 Low64Base 字串，反序列化再還原。
		轉換器清單同樣由生成器列出（掃 `IRawValueSerializable` 的實作者），不手寫。
	]

	#H[兩個踩過的坑][
		+ *靜態初始化順序*：`AppJsonCtx.WireOptions` 不能寫成靜態字段初始化器去讀
		`AppJsonCtx.Default`——生成檔裡的 `Default` 可能還沒初始化，拿到 null 後
		`TypeInfoResolver` 就靜默變 null，JIT 下一切正常（退回反射）、AOT 下才炸。
		現寫法 `=> field ??=` 延遲到第一次使用，並在建構期檢查 resolver 非 null、否則直接拋。
		+ *STJ 有兩份選項表面*：生成期的 `[JsonSourceGenerationOptions]`（在生成檔裡）與
		運行期的 `WireOptions` 是兩份獨立配置，兩邊不一致時同一個型別走不同路徑會寫出不同結果。
		改一處就要同改另一處，生成檔裡那段註釋也寫了同樣的提醒。
	]

	#H[驗證方式][
		`pwsh -File _verify/Run.ps1` 的第 8 組是行為探針（`_verify/ProbeJson.cs`），
		真的跑起來檢查：resolver 非 null、不轉命名、null 省略、中文不轉義、
		成功包絡往返、錯誤包絡拋 `TypedErr`、Id 寫成裸字串且可還原、`ITypeInfoSrc` 能列出實體成員。
		編譯過不等於行為對，這幾條只有跑起來才看得見。
	]
]

#H[用戶系統][
	一套可直接用的「註冊／登錄／退出登錄」：
	共享契約在 `proj/TswgCsTpl/Shared/User`、後端在 `proj/TswgCsTpl.Server`、前端在
	`proj/Frontend/TswgCsTpl.Ui/Client` 與 `Views/User`。

	#H[分層][
		+ 共享層：DTO、`ISvcUser`、強類型 Id、`KeysErr`、`KeysI18n`、`KeysUrl`、
		`AppJsonCtx`／`AppJson`／`WireJson`（見「JSON 序列化與源碼生成」章）
		+ 服務端：`SvcUser`（事務內註冊／登錄）、`SvcSession`（令牌簽發與撤銷）、
		`DaoUser`／`DaoSession`（CsSql 拼接器查詢）、`FullInit`（由表配置生成 DDL）
		+ 前端：`ClientUser`（`ISvcUser` 的 HTTP 實現）、`UserSession`（令牌狀態）、`ViewAccount`／`VmAccount`
	]

	#H[認證模型][
		+ 登錄成功簽發*不透明令牌*（隨機 32 位元組），庫裡只存它的 SHA-256
		+ 之後的請求帶 `Authorization: Bearer <令牌>`；中間件解出用戶身份放進 `ICurrentUser`
		+ 退出登錄＝把會話的 `RevokeAt` 寫成當前時刻；重複退出是冪等的
		+ 刻意不做 refresh token：過期即重新登錄。要加時擴 `PoSession`（加 RefreshHash／RefreshExpireAt）與一個端點
	]

	#H[端點][
		+ `POST /api/user/register`、`POST /api/user/login`（匿名）
		+ `POST /api/user/logout`、`GET /api/user/me`（需令牌）
		+ 響應一律是 `WebAns<T>`：`Data` 放數據、`Errors` 放「錯誤鍵＋參數」；
		文案由前端用同一套 I18n 翻，服務端不傳中文
	]

	#H[配置與遷移][
		+ 配置鍵用 CsKeyNode 聲明（`KeysServerCfg`），值來自 `appsettings.json` 或環境變量（`Pg__Host` 這種）
		+ 首次部署：`dotnet run --project proj/TswgCsTpl.Server -- --migrate`
		+ 不引入 CsCfg：鍵仍走 KeyNode，取值直接讀 `IConfiguration`
	]

	#H[與 Ngan.Dict 的差異（刻意）][
		+ 不引入 CsCfg；譯文直接寫在 C\# 裡（不生成 JSON、運行時不讀檔、也不內嵌語言資源），
		鍵與譯文一起編譯檢查，改鍵名就編譯錯
		+ 認證用「不透明令牌＋會話表」，而不是 JWT＋Redis；不需要額外服務，換來的是每次請求一次索引查詢
		+ 密碼用 PBKDF2-HMAC-SHA256（BCL 內建），而不是 Argon2id；算法與迭代次數存在密碼行裡，可逐條升級
		+ 服務接口不吃 `IUserCtx` 參數：身份由傳輸層（令牌）與 `ICurrentUser` 提供
	]

	#H[已做到的驗證][
		+ 共享層、服務端、Ui 全部、以及 Windows／Linux／Browser 三個宿主，都已用 Roslyn（csc）直接編譯通過；
		Ui 的聲明式寫法是在 Markup.Declarative 生成器參與下編譯的
		+ I18n 跑過實測：`Verify(KeysI18n.All, KeysErr.All)` 回 0 個問題；
		`zh-CN` 與 `en` 下取界面文案、取錯誤文案（含位置參數）都正確
		+ *尚未做*：`dotnet build`（本沙箱裡凡涉及專案引用圖的還原會在 `_GetAllRestoreProjectPathItems` 靜默失敗）、
		Android 宿主編譯、以及起 PostgreSQL 跑一次註冊→登錄→`/me`→退出的實測
		+ 強類型 Id 的模板（`UInt128Template.typedid`）在 csc 下沒被 `-additionalfile` 餵進去，
		故 Id 的生成體是用等價 stub 驗的；真正的模板生成要在 `dotnet build` 時確認
	]
]


#H[][
	確保以下符號不需要額外引用其他命名空間就能直接使用:
	- `Di`
	- `AppLog`
	- `Todo`
	- `KeysErr`
	要麼就置于根命名空間的直接子層(在本項目中即`TswgCsTpl`),
	要麼就用`global using`實現。
	目前 `Tsinswreng.CsErr` 走全域 using（`proj/TypeAlias.cs` 與 `Frontend/GlobalUsing.cs`），故拋錯處只寫 `KeysErr.X.ToErr(...)` 即可。
]










