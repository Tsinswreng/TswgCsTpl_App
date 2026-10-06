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
	應在每個項目中定義一個`AppLog`作爲日誌門面
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
	不方便依賴注入時就直接調用全局可用的`AppLog.Inst`
]

#H[基本存儲區路徑][
	對于[不運行在瀏覽器環境中]的程序,
	要爲應用存儲區目錄做抽像:
	#link("TswgCsTpl/proj/TswgCsTpl/Infra/I_GetBaseDir.cs")

實現參考:
#link("TswgCsTpl/proj/TswgCsTpl/Infra/CwdBaseDirMgr.cs")

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

