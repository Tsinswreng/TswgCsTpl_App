namespace TswgCsTpl.Ui.Views.Main;

using TswgCsTpl.Common;

public partial class VmMain{

	public static partial VmMain Mk(){
		return new VmMain();
	}

	public partial void AddOne(){
		// step 1: 改狀態。屬性的 setter 會發屬性通知，界面跟著更新。
		Counter = Counter + 1;

		// step 2: 更新狀態文字。
		//         使用者可見的文字走 Todo.I18n（尚未接入真正的 I18n，暫時原樣返回）。
		StatusText = Todo.I18n($"已加一，累計 {Counter} 次");
	}
}
