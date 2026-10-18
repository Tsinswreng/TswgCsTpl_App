namespace TswgCsTpl.Ui.Views.User;

using Avalonia.Controls;
using Tsinswreng.CsCore;
using Tsinswreng.Avln.Grid;
using TswgCsTpl.Ui.Infra;

[Doc(@"賬戶視圖：左邊登錄、右邊註冊，下方一行狀態文字。

*寫法*：純 C# 聲明式（Avalonia.Markup.Declarative），不建 XAML。
文字一律走 I18n 鍵，不在這裡硬編碼中文字串。

*為何兩塊都常駐可見*：模板階段先求「一眼看到全部能力和狀態」；
真要收成標籤頁時只改這裡的排版，不動 Vm。")]
public partial class ViewAccount : AppViewBase<VmAccount> {
	[Doc("唯一的無參構造器：Declarative 的視圖工廠要求它。")]
	public partial ViewAccount();

	[Doc("根容器：逐列往下排。")]
	public GridStack Root = new(IsRow: true);

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

	[Doc("建立控件樹。")]
	protected override partial obj Build(VmAccount Vm);
}

