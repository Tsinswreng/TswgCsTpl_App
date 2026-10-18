namespace TswgCsTpl.Ui;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;
using TswgCsTpl.Ui.Views.Main;

public partial class App{

	public static partial Window MkMainWindow(){
		// 根視圖由 Declarative 的 ViewBase 組出來，它本身就是一個 Control，
		// 故桌面的 Window.Content 與單視圖的 MainView 掛的是同一個東西。
		return new Window{
			Title = AppName,
			Width = 1000,
			Height = 640,
			WindowStartupLocation = WindowStartupLocation.CenterScreen,
			Content = new ViewMain(),
		};
	}

	public override partial void Initialize(){
		// 純 C# 掛主題，取代 Avalonia 模板的 App.axaml（本專案不建任何 .axaml）。
		Styles.Add(new FluentTheme());
	}

	public override partial void OnFrameworkInitializationCompleted(){
		switch(ApplicationLifetime){
			case IClassicDesktopStyleApplicationLifetime Desktop:
				// 桌面（Windows／Linux）：關掉主視窗即退出。
				Desktop.MainWindow = MkMainWindow();
				break;

			case ISingleViewApplicationLifetime Single:
				// Android／Browser：沒有視窗，直接把根控件掛到唯一的視圖上。
				Single.MainView = new ViewMain();
				break;
		}

		base.OnFrameworkInitializationCompleted();
	}
}
