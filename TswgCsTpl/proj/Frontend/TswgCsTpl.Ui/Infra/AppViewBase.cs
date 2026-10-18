namespace TswgCsTpl.Ui.Infra;

using Avalonia.Markup.Declarative;
using Tsinswreng.CsCore;

[Doc(@"所有視圖的基類：在 Declarative 的 `ViewBase<TVm>` 之上提供強型別的 vm 訪問器。
子類的 `Build` 由基類傳入這個強型別的 Vm，因此不必自己宣告屬性，也不必判空。
#TPrm[本視圖使用的 ViewModel 型別]
")]
public abstract partial class AppViewBase<TVm> : ViewBase<TVm> where TVm : class{

	[Doc(@"本視圖的 ViewModel。由子類的建構器設好之後就不再變，故不判空。
不能寫成 public get; set;，否則源生成器生成的代碼會出錯（實測 CS0246）。")]
	protected TVm vm{
		get{
			return ViewModel!;
		}
		set{
			ViewModel = value;
		}
	}

	[Doc(@"本視圖的 ViewModel（對外唯讀）。
開放給父視圖是為了讓它能拿到「子視圖自己解析出的那一份 Vm」。

*一個視圖只配一份 Vm，而且是在視圖建立時就綁定的*：控件樹與各控件的綁定都在
建立的當下寫死，故「先 new 視圖、再換一份 Vm 進去」不會生效。
需要共用同一份 Vm 的多個視圖，一律採「子視圖先造、父層收養它造出來的那一份」。
")]
	public TVm Vm{
		get{
			return vm;
		}
	}
}
