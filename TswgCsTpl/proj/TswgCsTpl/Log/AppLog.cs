namespace TswgCsTpl.Log;

using Tsinswreng.CsLog;

[Doc($$"""
全局日誌門面：不方便依賴注入的地方（視圖、靜態工具、啟動早期）直接調 {nameof(AppLog)}.Inst。

它自己不產生任何輸出，只把呼叫轉發給內層實現 {nameof(IDelegatingLogger.InnerLogger)}；
內層實現由各宿主在入口處指定：Windows 寫檔、Android 進 logcat、Linux／Browser 進主控台。
故換輸出目標時只改入口的一行 {nameof(IServiceCollectionExtn.AddAppLog)}，全項目其餘程式碼不動。

*為何不用依賴注入*：{nameof(AppLog)}.Inst 必須在容器建立之前就能用——啟動失敗往往就發生在建容器途中。
兩者並不衝突：注入用的日誌介面與泛型版也被 {nameof(IServiceCollectionExtn.AddAppLog)} 接到同一個門面上。

#Eg[
	```cs
	AppLog.Info("配置載入完成");
	AppLog.Error(ex, "配置載入失敗");
	```
]
#See[{{nameof(ExtnAppLog)}}][Info／Warn／Error 等便捷寫法。]
""")]
public class AppLog : DelegatingLogger {
	[Doc("全局單例；第一次取用時才建立，故不依賴任何啟動順序。")]
	public static AppLog Inst => field ??= new AppLog();
}
