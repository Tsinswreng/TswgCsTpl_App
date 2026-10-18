namespace TswgCsTpl.Ui.Views.Main;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using Avalonia.Styling;

using TswgCsTpl.Common;
using TswgCsTpl.Ui.Infra;

public partial class ViewMain{

	public partial ViewMain(){
		// Vm 由業務層的 Di 門面解析：容器有註冊就用容器提供的，
		// 否則退回 VmMain.Mk()（本示範沒有依賴，兩條路等價）。
		// 上一層（各入口層）必須在建立視圖之前設好 Di.SvcProvider。
		vm = Di.DiOrMk<VmMain>();
	}

	protected override partial StyleGroup? BuildStyles(){
		return [
			// 主視圖按鈕：統一最小寬度與左邊距，避免各處各寫一份。
			new Style<Button>(Sel => Sel.Class(Cls.MainBtn))
				.MinWidth(96)
				.Margin(new Thickness(12, 0, 12, 12))
				.HorizontalAlignment(HAlign.Left),
		];
	}

	protected override partial obj Build(VmMain Vm){
		// GridStack 是「Grid ＋ 自動落號」的容器：它自己不是 Control，
		// 真正進控件樹的是它包住的那個 Grid（故最後回傳 Root.Grid）。
		Root.Grid.RowDefinitions = RowDefinitions.Parse("Auto,Auto,*");

		Root.Add(
			new TextBlock()
				.With(t => {
					_TxtTitle = t;
				})
				.Text(Todo.I18n("TswgCsTpl 前端骨架"))
				.FontSize(20)
				.Margin(new Thickness(12, 12, 12, 0))
		);

		Root.Add(
			new TextBlock()
				.With(t => {
					_TxtStatus = t;
				})
				// 編譯期綁定：第二個參數是 Vm 實例、第三個是成員選擇器。
				// 禁止 new Binding("StatusText") 那類字串路徑（不兼容 AOT）。
				.Text(Vm, x => x.StatusText)
				.Margin(new Thickness(12, 8))
		);

		Root.Add(
			new Button()
				.With(b => {
					_BtnAdd = b;
					b.Classes.Add(Cls.MainBtn);
					// 同步、無耗時的事件直接在這裡處理；耗時工作要交給 Vm 裡的 Fire。
					b.OnClick(_ => Vm.AddOne());
				})
				.Content(Todo.I18n("加一"))
				.VerticalAlignment(VAlign.Top)
		);

		return Root.Grid;
	}
}
