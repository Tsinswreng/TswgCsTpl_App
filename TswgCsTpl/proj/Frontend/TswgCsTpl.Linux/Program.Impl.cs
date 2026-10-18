namespace TswgCsTpl.Linux;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

using Microsoft.Extensions.DependencyInjection;

using TswgCsTpl.Common;
using TswgCsTpl.Log;
using TswgCsTpl.Ui;

public static partial class Program{

	[STAThread]
	public static partial void Main(str[] Args){
		// step 1: 先建容器並交給業務層的 Di 門面（理由見 Windows 入口層的同一步）。
		Di.SvcProvider = MkServices();

		// step 2: 桌面生命週期。與 Windows 相同：只有一個視窗，關掉即退出。
		var lifetime = new ClassicDesktopStyleApplicationLifetime{
			Args = Args,
			ShutdownMode = ShutdownMode.OnMainWindowClose,
		};

		// step 3: 配置 Avalonia 並啟動。
		//         UsePlatformDetect 在 Linux 上選 X11 後端；主視窗由 App 依生命週期掛載。
		AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace()
			.SetupWithLifetime(lifetime);

		lifetime.Start(Args);
	}

	private static partial IServiceProvider MkServices(){
				var Svc = new ServiceCollection();

		// 本平臺的全部註冊都集中在 DiLinux。
		Svc.SetupLinux();

		return Svc.BuildServiceProvider();
	}
}


