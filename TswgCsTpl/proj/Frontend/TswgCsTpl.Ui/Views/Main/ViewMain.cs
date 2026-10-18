namespace TswgCsTpl.Ui.Views.Main;

using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using Avalonia.Styling;

using TswgCsTpl.Ui.Infra;
using Tsinswreng.Avln.Grid;
using Tsinswreng.CsCore;

[Doc(@"主視圖：一個標題、一行狀態文字、一顆「加一」按鈕。

*寫法*：純 C# 聲明式（`Avalonia.Markup.Declarative`），不建 `.axaml`、不寫 XAML。
控件樹由 `Build` 的回傳值描述，父子關係用 `.Children(...)`；
事件用生成的 `.OnClick(...)`，綁定用生成的 `.Text(Vm, x => x.Xxx)`——全程沒有字串路徑。

*版面*：根節點是 `GridStack`（Avln.Grid 的順序容器，子項一加進來就自動落號），
三列＝標題（Auto）｜狀態（Auto）｜按鈕（*）。故不必自己算 Grid 的行號。
")]
public partial class ViewMain : AppViewBase<VmMain>{

	[Doc(@"唯一的無參構造器。
View 只能有無參構造器：Declarative 的視圖工廠與依賴注入都依賴它。
")]
	public partial ViewMain();

	// ── 佈局容器 ──

	[Doc(@"根容器。`IsRow: true` 表示全為行的佈局（子項依加入順序往下排）。
一個 GridStack 只能全為行或全為列；需要兩維時就嵌套，不要同時設行與列。
")]
	public GridStack Root = new(IsRow: true);

	// ── 關鍵控件 ──
	// 頁面中涉及輸入操作、資訊展示的控件都提為 public 成員，方便測試與後續規劃。

	[Doc("標題文字。")]
	public TextBlock? _TxtTitle;

	[Doc("狀態文字。綁定 `VmMain.StatusText`。")]
	public TextBlock? _TxtStatus;

	[Doc("「加一」按鈕。點擊即呼叫 `VmMain.AddOne`。")]
	public Button? _BtnAdd;

	[Doc(@"建立本視圖的樣式。
樣式只有一條（按鈕的統一間距與最小寬度），故直接在此建立，不另抽方法。
#Rtn[樣式組；Declarative 會把它併進本視圖的樣式表。]
")]
	protected override partial StyleGroup? BuildStyles();

	[Doc(@"建立控件樹。根控件由回傳值描述，父子關係用 `.Children(...)`。
#Prm[本視圖的 ViewModel（由基底類別傳入，型別已確定，故不判空）]
#Rtn[控件樹的根控件]
")]
	protected override partial obj Build(VmMain Vm);

	[Doc("樣式類名常量。禁止在別處用字串硬編碼類名。")]
	public static partial class Cls{

		[Doc("主視圖按鈕的類名。")]
		public const str MainBtn = nameof(MainBtn);
	}
}
