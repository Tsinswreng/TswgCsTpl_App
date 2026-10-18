namespace TswgCsTpl.Log;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[Doc($$"""
日誌相關的依賴注入裝配。

四個宿主原本各自貼同一段「設 {nameof(AppLog)}.Inst 的內層實現 ＋ AddSingleton ＋ AddLogging」，
改一處漏三處；收成 {nameof(AddAppLog)} 之後，入口只留一行，宿主之間只剩「傳哪個內層實現」這一處差異。
""")]
public static class IServiceCollectionExtn {
	extension(IServiceCollection z) {
		[Doc($$"""
		把 {nameof(AppLog)} 接到容器，並指定本宿主的日誌去向。

		做完之後三種寫法都落到同一個內層實現：注入日誌介面、注入泛型日誌介面、直接調 {nameof(AppLog)}.Inst。

		#Prm[{{nameof(InnerLogger)}}][本宿主的日誌實現，由各宿主提供：桌面檔案、Android logcat、主控台。]
		#Rtn[同一個 {nameof(IServiceCollection)}，便於串接。]
		#Eg[
			```cs
			// Linux
			Svc.AddAppLog(MkrLogger.Console(App.AppName));

			// Windows
			Svc.AddAppLog(new WindowsFileLogger());

			// Android
			Svc.AddAppLog(new AndroidLogger(App.AppName));
			```
		]
		""")]
		public IServiceCollection AddAppLog(ILogger InnerLogger) {
			// 1: 全局門面先指向本宿主的實現——這一步必須在建任何視圖之前完成，
			//    否則建視圖期間的日誌會落到空的內層實現上而丟失。
			AppLog.Inst.InnerLogger = InnerLogger;

			// 2: 讓注入取得的是同一個門面，而不是另一份實現。
			z.AddSingleton<ILogger>(_ => AppLog.Inst);

			// 3: 泛型日誌介面由 Logging 框架的分類機制產生，
			//    故換掉它的 provider 才能接到門面上；先清空默認 provider，避免多出第二條出口。
			z.AddLogging(B => {
				B.ClearProviders();
				B.AddProvider(AppLogLoggerProvider.Inst);
			});
			return z;
		}
	}
}
