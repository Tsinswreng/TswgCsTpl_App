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
```cs
public interface I_GetBaseDir{
	
	/// 安卓應用之pwd皆根目錄。故叵直ᵈ由相對路徑㕥載入配置等。
	/// 宜皆由此㕥拼接路徑。
	[Doc(@$"
	表示應用儲存區的根目錄。
	適用于 非Browser之客戶端。
	在Windows上 實現 爲 `Directory.GetCurrentDirectory()`
	在Android上 實現 爲 
	`global::Android.App.Application.Context.GetExternalFilesDir(null).AbsolutePath`
	")]
	public str GetBaseDir();
}

public static class I_GetBaseDirExtn{
	public static str Combine(
		this I_GetBaseDir z
		,params IList<str> Segs
	){
		return Path.Combine([z.GetBaseDir(), ..Segs]);
	}
}


public class CwdBaseDirMgr:I_GetBaseDir{
	public static CwdBaseDirMgr Inst =>field??= new CwdBaseDirMgr();
	public str _BaseDir{get;set;} = Directory.GetCurrentDirectory();
	public str GetBaseDir(){
		return _BaseDir;
	}
}

```
]

#H[基 遠端Api Url][
```cs
public partial interface I_GetBaseUrl {
	public str GetBaseUrl();
}
```
與`I_GetBaseDir`近似
]


#H[Avalonia配置][
```cs


```

]

CsErr2



Avalonia:

App.cs
IMk
App.DiOrMk

