namespace TswgCsTpl.Ui.Views.Account;

using Avalonia.Controls;
using Tsinswreng.CsCore;
using Tsinswreng.Avln.Grid;
using TswgCsTpl.Ui.Infra;

[Doc(@"賬戶視圖：上方登錄、下方註冊，最底一行狀態文字。

*寫法*：純 C# 聲明式（Avalonia.Markup.Declarative），不建 XAML、不寫字串路徑綁定。
文字一律走 I18n 鍵，不在這裡硬編碼。

*版面*：根節點是 `StackGrid`（Tsinswreng.Avln.Grid 的自動落號 Grid），
子項依加入順序自動落號，故不必自己算行號；需要兩維時就嵌套。
")]
public partial class ViewAccount : AppViewBase<VmAccount> {
	[Doc("唯一的無參構造器：Declarative 的視圖工廠與依賴注入都依賴它。")]
	public partial ViewAccount();

	[Doc("根容器：全為行的佈局，子項依序往下排。")]
	public StackGrid Root = new(IsRow: true);

	[Doc("登錄身份輸入框。")]
	public TextBox? _TxtIdentity;

	[Doc("登錄密碼輸入框。")]
	public TextBox? _TxtPassword;

	[Doc("註冊登錄名輸入框。")]
	public TextBox? _TxtRegUniqName;

	[Doc("註冊郵箱輸入框。")]
	public TextBox? _TxtRegEmail;

	[Doc("註冊密碼輸入框。")]
	public TextBox? _TxtRegPassword;

	[Doc("註冊確認密碼輸入框。")]
	public TextBox? _TxtRegConfirm;

	[Doc("狀態文字（成功提示／錯誤文案／當前用戶）。")]
	public TextBlock? _TxtStatus;

	[Doc(@"建立控件樹。
#Prm[本視圖的 ViewModel（由基底類別傳入，型別已確定，故不判空）]
#Rtn[控件樹的根控件]
")]
	protected override partial obj Build(VmAccount Vm);
}

