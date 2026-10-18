namespace TswgCsTpl.Ui.Infra;

using CommunityToolkit.Mvvm.ComponentModel;
using Tsinswreng.CsCore;

[Doc(@"所有 ViewModel 的基類。

生命週期約定：
依賴注入建立的 Vm，其唯一的 public 構造器最後必須呼叫 `Init`；
任何需要依賴的 public 方法開頭應呼叫 `CheckInit`，及早暴露
「用 Mk() 造出來、卻當成注入實例使用」這種錯誤。

屬性通知用 `SetProperty(ref field, value)`（來自 `ObservableObject`）＋ C# 的 `field` 關鍵字。

非同步工作一律經 `Fire` 啟動：UI 事件處理器與生命週期回調都是 void、不能 await，
直接在視圖裡裸呼叫非同步方法會讓例外變成「未被觀察的例外」。
")]
public partial class AppVmBase : ObservableObject{

	[Doc("供子類以「不注入依賴」方式建立時使用（見 Mk()）。設為 protected：禁止外部直接 new 出一個沒有依賴的 Vm。")]
	protected AppVmBase(){
	}

	[Doc("是否已完成初始化。由 `Init()` 設為 true。")]
	public bool IsInited{get;set;} = false;

	[Doc("標記初始化完成。由依賴注入構造器在設置完所有依賴之後調用。")]
	public partial void Init();

	[Doc("檢查是否已初始化；未初始化則拋出異常。")]
	public partial void CheckInit();

	[Doc(@"啟動非同步工作。
UI 事件處理器與生命週期回調都是 void、不能 await，故一律由此啟動。
#Prm[已啟動的非同步操作]
")]
	protected partial void Fire(Task<nil> Op);

	[Doc(@"`Fire` 的實際執行段：等它、接住例外、轉給 `HandleErr`。

*為何抽成另一個方法*：`Fire` 自己不是 async（它是同步的啟動點），
若把 await 寫在裡面就得宣告成 `async void`，那會讓例外逃出 try 區塊。
#Prm[已啟動的非同步操作]
#Rtn[空值]
")]
	private partial Task<nil> RunFire(Task<nil> Op);

	[Doc(@"非同步工作的失敗出口。
子類覆寫它以決定錯誤顯示在哪裡；它自己不得拋例外，因為呼叫它的是 `Fire` 的 catch 區塊。
#Prm[失敗的例外]
")]
	protected virtual partial void HandleErr(Exception Ex);
}
