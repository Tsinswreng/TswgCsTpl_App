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
namespace Ngan.Dict.Ui;

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
在
	]
]

#H[日誌][
	全項目只有一個日誌門面 `AppLog`（命名空間 `TswgCsTpl.Log`）。
	它自己不產生任何輸出,
	只把呼叫轉發給各宿主挑選的內層實現。
	故換輸出目標時只改入口的一行,
	其餘程式碼不動。
	參照 `Ngan.Ime` 的形狀,
	但多了「泛型日誌也走同一條出口」與「四份入口重複碼收斂」兩件事。

	#H[組成][
		+ `Log/AppLog.cs`: 門面本體, 繼承 `DelegatingLogger`（來自 `Tsinswreng.CsLog`）
		+ `Log/ExtnAppLog.cs`: `AppLog.Info/Warn/Error/Debug/Trace` 便捷寫法
		+ `Log/IServiceCollectionExtn.cs`: `AddAppLog` 把門面接到容器
		+ `Log/AppLogLoggerProvider.cs`: 讓泛型日誌介面也走門面, 並把分類名綴在訊息前
		+ `Log/MkrLogger.cs`: 主控台類宿主的內層實現
	]

	#H[三種寫法共用一條出口][
		+ 直接調門面: `AppLog.Info("配置載入完成")`。
		+ 注入 `ILogger`: 拿到的是 `AppLog.Inst` 本身。
		+ 注入泛型日誌介面: 由 `AppLogLoggerProvider` 轉發到同一個門面, 故日誌行帶分類前綴。

		*不要*在入口另外寫 `AddLogging(B => B.AddConsole())`。
		那會造出第二條出口,
		注入的日誌與直接調門面的日誌分岔到兩處, 排錯時看到的是殘缺的兩半。
	]

	#H[各宿主的內層實現][
		+ Windows: `WindowsFileLogger`
		（寫檔; 桌面是 WinExe, 雙擊啟動時沒有終端, 主控台輸出會全部消失）
		+ Android: `AndroidLogger`（logcat, 固定 tag 便於單獨過濾）
		+ Linux: 主控台（`MkrLogger.Console`）
		+ Browser: 主控台（WASM 的 stdout 被導向開發者工具 Console）
	]

	#H[日誌檔位置][
		Windows 的日誌檔路徑由 `I_GetBaseDir` 決定,
		落在應用儲存區根目錄下的 `logs/TswgCsTpl.Windows.log`。
		檔案器本身不拼根目錄也不接參數,
		而是向 `Di` 取 `I_GetBaseDir`,
		故換儲存區實現時日誌自動跟著走。
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

