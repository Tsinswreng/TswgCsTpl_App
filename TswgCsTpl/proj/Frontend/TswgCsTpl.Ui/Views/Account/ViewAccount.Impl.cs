namespace TswgCsTpl.Ui.Views.Account;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Tsinswreng.CsCore;
using Tsinswreng.CsI18n;
using TswgCsTpl.Infra.I18n;
using TswgCsTpl.Ui.Infra;

public partial class ViewAccount {
	[Doc("界面文案門面。")]
	II18n I18n => AppI18n.Inst;

	public partial ViewAccount() {
		// Vm 由業務層的 Di 門面解析：容器有註冊就用容器提供的，否則退回 VmAccount.Mk()。
		// 上一層（各入口層）必須在建立視圖之前設好 Di.SvcProvider。
		vm = Di.DiOrMk<VmAccount>();
	}

	[Doc($$"""
	一列「標籤 ＋ 輸入框」。
	#Prm[{{nameof(Label)}}][標籤文字。]
	#Prm[{{nameof(Box)}}][輸入框。]
	#Rtn[可加入控件樹的一列。]
	""")]
	static Control Field(str Label, TextBox Box) {
		Box.Width = 220;
		return new StackPanel()
			.Orientation(Orientation.Horizontal)
			.Margin(new Thickness(12, 4))
			.Children(
				new TextBlock {
					Text = Label,
					Width = 96,
					VerticalAlignment = VAlign.Center,
				},
				Box
			);
	}

	protected override partial obj Build(VmAccount Vm) {
		// 十一列，全為 Auto：標題／輸入列／按鈕列都不需要撐開。
		Root.Rows("Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto")
		.Children(
			// ── 登錄區 ──
			new TextBlock()
				.Text(I18n[KeysI18n.Login])
				.FontSize(18)
				.Margin(new Thickness(12, 12, 12, 0)),

			Field(I18n[KeysI18n.Identity], new TextBox()
				.With(t => { _TxtIdentity = t; })
				.Text(Vm, x => x.Identity)),

			Field(I18n[KeysI18n.Password], new TextBox()
				.With(t => { _TxtPassword = t; t.PasswordChar = '●'; })
				.Text(Vm, x => x.Password)),

			new StackPanel()
				.Orientation(Orientation.Horizontal)
				.Margin(new Thickness(12, 4))
				.Children(
					new Button()
						.With(b => {
							b.Classes.Add(Cls.AccountBtn);
							b.OnClick(_ => Vm.FireLogin());
						})
						.Content(I18n[KeysI18n.Login]),
					new Button()
						.With(b => {
							b.Classes.Add(Cls.AccountBtn);
							b.OnClick(_ => Vm.FireLogout());
						})
						.Content(I18n[KeysI18n.Logout])
				),

			// ── 註冊區 ──
			new TextBlock()
				.Text(I18n[KeysI18n.Register])
				.FontSize(18)
				.Margin(new Thickness(12, 12, 12, 0)),

			Field(I18n[KeysI18n.UniqName], new TextBox()
				.With(t => { _TxtRegUniqName = t; })
				.Text(Vm, x => x.RegUniqName)),

			Field(I18n[KeysI18n.Email], new TextBox()
				.With(t => { _TxtRegEmail = t; })
				.Text(Vm, x => x.RegEmail)),

			Field(I18n[KeysI18n.Password], new TextBox()
				.With(t => { _TxtRegPassword = t; t.PasswordChar = '●'; })
				.Text(Vm, x => x.RegPassword)),

			Field(I18n[KeysI18n.ConfirmPassword], new TextBox()
				.With(t => { _TxtRegConfirm = t; t.PasswordChar = '●'; })
				.Text(Vm, x => x.RegConfirmPassword)),

			new Button()
				.With(b => {
					b.Classes.Add(Cls.AccountBtn);
					b.Margin = new Thickness(12, 4);
					b.OnClick(_ => Vm.FireRegister());
				})
				.Content(I18n[KeysI18n.Register]),

			// ── 狀態列 ──
			new TextBlock()
				.With(t => { _TxtStatus = t; })
				.Text(Vm, x => x.StatusText)
				.Margin(new Thickness(12, 12))
		);

		return Root;
	}

	[Doc("樣式類名常量。禁止在別處硬編碼類名。")]
	public static class Cls {
		[Doc("賬戶區按鈕的類名。")]
		public const str AccountBtn = nameof(AccountBtn);
	}
}

