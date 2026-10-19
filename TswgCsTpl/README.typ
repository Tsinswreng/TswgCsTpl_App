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

用法（在模板根目錄）：
```sh
dotnet run --project proj/TswgCsTpl.Scripts/TswgCsTpl.Scripts.csproj -- Test
dotnet run --project proj/TswgCsTpl.Scripts/TswgCsTpl.Scripts.csproj -- TestAotWin
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
]

#H[界面文案與譯文][
	鍵：`Common/I18n/KeysI18n.cs`，*平鋪一層*、不按畫面分組；同一鍵同一譯文（msg-fmt-key 的理念）。
	譯文：`Common/I18n/MsgZhCn.cs`、`MsgEn.cs`，直接用 C\# 寫，不生成 JSON、不讀檔。
	+ 每條寫成 `new(KeysI18n.Login, "登錄")`——鍵直接引用業務側的鍵物件，*改名就編譯錯*
	+ 錯誤文案同理：`new(KeysErr.User.PasswordNotMatch, "登錄名或密碼不正確")`，鍵規範化成 `Error/` ＋ 錯誤路徑
	+ 模板只用位置參數 `{0}`、`{1}`；C\# 原始字符串可多行、完全不必轉義（JSON 做不到這點）
	+ 啟動自檢：`DiCore.SetupCore` 會跑 `AppI18n.Verify()` 比對「每個語言的條目」與「`KeysI18n.All` 全集」，
	缺譯／重複／未登記的鍵只記日誌、不中斷啟動
	+ 切換語言：`AppI18n.Use("en")`；已渲染的界面需自行重繪
	+ 鍵那側的參數佔位照 msg-fmt-key 規範：一個 `__` 對應一個位置參數（例：`Select__From__`）
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


#H[用戶系統][
	一套可直接用的「註冊／登錄／退出登錄」：
	共享契約在 `proj/TswgCsTpl/Shared/User`、後端在 `proj/TswgCsTpl.Server`、前端在
	`proj/Frontend/TswgCsTpl.Ui/Client` 與 `Views/User`。

	#H[分層][
		+ 共享層：DTO、`ISvcUser`、強類型 Id、`KeysErr`、`KeysI18n`、`KeysUrl`、`AppJsonCtx`
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
		+ 不引入 CsCfg；I18n 自寫 `I18nDict : II18n`（只沿用 CsI18n 的接口與鍵模型），語言檔內嵌為資源
		+ 認證用「不透明令牌＋會話表」，而不是 JWT＋Redis；不需要額外服務，換來的是每次請求一次索引查詢
		+ 密碼用 PBKDF2-HMAC-SHA256（BCL 內建），而不是 Argon2id；算法與迭代次數存在密碼行裡，可逐條升級
		+ 服務接口不吃 `IUserCtx` 參數：身份由傳輸層（令牌）與 `ICurrentUser` 提供
	]

	#H[已做到的驗證][
		+ 共享層、服務端、Ui 全部、以及 Windows／Linux／Browser 三個宿主，都已用 Roslyn（csc）直接編譯通過；
		Ui 的聲明式寫法是在 Markup.Declarative 生成器參與下編譯的
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






