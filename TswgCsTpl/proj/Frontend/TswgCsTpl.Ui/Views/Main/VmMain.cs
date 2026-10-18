namespace TswgCsTpl.Ui.Views.Main;

using TswgCsTpl.Common;
using TswgCsTpl.Ui.Infra;
using Tsinswreng.CsCore;

[Doc(@"主視圖（`ViewMain`）的 ViewModel。

職責邊界：只有狀態與命令，不碰任何控件、不做視圖跳轉、不耦合 View 層細節。
本檔是骨架專案的示範：按鈕按一次就把計數加一並更新狀態文字，
用來證明「聲明式綁定 ＋ 屬性通知」這條鏈路是通的。
")]
public partial class VmMain : AppVmBase, IMk<VmMain>{

	[Doc(@"供 `Mk()` 使用的無參構造器。
本示範沒有依賴，故 `Mk()` 造出來的實例就是完整可用的。
設為 protected：禁止外部直接 new（一律走依賴注入或 `Mk()`）。
")]
	protected VmMain(){
	}

	[Doc(@"不注入依賴的建立方式。`Di.DiOrMk<VmMain>()` 在容器沒有註冊時會退回它。
不能定義多個 public 構造器，否則依賴注入無法確定該用哪一個。
")]
	public static partial VmMain Mk();

	[Doc(@"按鈕被按下的次數。
綁定屬性的 setter 一律寫成 `SetProperty(ref field, value)`，這樣界面才會收到通知。
")]
	public i32 Counter{
		get;
		set{ SetProperty(ref field, value); }
	} = 0;

	[Doc(@"狀態列文字。UI 顯示的文字一律經過 `Todo.I18n`，不直接硬編碼在視圖裡。
初始值與按下按鈕後的更新都在 ViewModel 這一側，視圖只負責顯示與呼叫命令。
")]
	public str StatusText{
		get;
		set{ SetProperty(ref field, value); }
	} = Todo.I18n("就緒");

	[Doc("計數加一並更新狀態文字。同步、無耗時操作，故可直接給按鈕的點擊事件綁定。")]
	public partial void AddOne();
}
