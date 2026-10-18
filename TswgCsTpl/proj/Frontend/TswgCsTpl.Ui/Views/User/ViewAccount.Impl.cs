namespace TswgCsTpl.Ui.Views.User;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Tsinswreng.CsCore;
using Tsinswreng.CsI18n;
using TswgCsTpl.Common;
using TswgCsTpl.Infra.I18n;
using TswgCsTpl.Ui.Infra;

public partial class ViewAccount {
	II18n I18n => AppI18n.Inst;

	public partial ViewAccount() {
		// Vm 由業務層的 Di 門面解析：容器有註冊就用容器的，否則退回一個沒有依賴的實例。
		vm = Di.GetRSvc<VmAccount>();
	}

	/// 標籤與輸入框成對排一列，避免每個字段各寫一遍排版。
	static Control Field(str Label, TextBox Box) {
		var Row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(12, 4) };
		Row.Children.Add(new TextBlock {
			Text = Label,
			Width = 96,
			VerticalAlignment = VAlign.Center,
		});
		Box.Width = 220;
		Row.Children.Add(Box);
		return Row;
	}

	protected override partial obj Build(VmAccount Vm) {
		// 1: 登錄區。
		Root.Add(new TextBlock { Text = I18n[KeysI18n.Ui.Login], FontSize = 18, Margin = new Thickness(12, 12, 12, 0) });

		Root.Add(Field(I18n[KeysI18n.Ui.Identity], new TextBox()
			.With(t => { _TxtIdentity = t; })
			.Text(Vm, x => x.Identity)));
		Root.Add(Field(I18n[KeysI18n.Ui.Password], new TextBox()
			.With(t => { _TxtPassword = t; t.PasswordChar = '●'; })
			.Text(Vm, x => x.Password)));

		Root.Add(new StackPanel {
			Orientation = Orientation.Horizontal,
			Margin = new Thickness(12, 4),
			Children = {
				new Button()
					.With(b => {
						b.Classes.Add(Cls.AccountBtn);
						b.OnClick(_ => Vm.FireLogin());
					})
					.Content(I18n[KeysI18n.Ui.Login]),
				new Button()
					.With(b => {
						b.Classes.Add(Cls.AccountBtn);
						b.OnClick(_ => Vm.FireLogout());
					})
					.Content(I18n[KeysI18n.Ui.Logout]),
			},
		});

		// 2: 註冊區。
		Root.Add(new TextBlock { Text = I18n[KeysI18n.Ui.Register], FontSize = 18, Margin = new Thickness(12, 12, 12, 0) });

		Root.Add(Field(I18n[KeysI18n.Ui.UniqName], new TextBox()
			.With(t => { _TxtRegUniqName = t; })
			.Text(Vm, x => x.RegUniqName)));
		Root.Add(Field(I18n[KeysI18n.Ui.Email], new TextBox()
			.With(t => { _TxtRegEmail = t; })
			.Text(Vm, x => x.RegEmail)));
		Root.Add(Field(I18n[KeysI18n.Ui.Password], new TextBox()
			.With(t => { _TxtRegPassword = t; t.PasswordChar = '●'; })
			.Text(Vm, x => x.RegPassword)));
		Root.Add(Field(I18n[KeysI18n.Ui.ConfirmPassword], new TextBox()
			.With(t => { _TxtRegConfirm = t; t.PasswordChar = '●'; })
			.Text(Vm, x => x.RegConfirmPassword)));

		Root.Add(new Button()
			.With(b => {
				b.Classes.Add(Cls.AccountBtn);
				b.Margin = new Thickness(12, 4);
				b.OnClick(_ => Vm.FireRegister());
			})
			.Content(I18n[KeysI18n.Ui.Register]));

		// 3: 狀態列。
		Root.Add(new TextBlock()
			.With(t => { _TxtStatus = t; })
			.Text(Vm, x => x.StatusText)
			.Margin(new Thickness(12, 12)));

		return Root.Grid;
	}

	[Doc("樣式類名常量。禁止在別處硬編碼類名。")]
	public static class Cls {
		[Doc("賬戶區按鈕的類名。")]
		public const str AccountBtn = nameof(AccountBtn);
	}
}

