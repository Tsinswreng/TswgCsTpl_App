namespace TswgCsTpl.Common;

[Doc($$"""
桌面宿主的應用儲存區根目錄：可執行檔所在目錄。

*為何不是當前工作目錄*：從 cmd 或捷徑啟動時當前目錄可能是任何地方，
日誌與配置會跟著跑；可執行檔目錄才是這份應用真正「屬於自己」的位置。

故入口在建立容器時就把它註冊成 {nameof(I_GetBaseDir)}，
需要寫檔的元件（例如 Windows 的檔案日誌器）向容器取，而不是自己拼路徑。

#Eg[
	```cs
	var Dir = Di.GetRSvc<I_GetBaseDir>().GetBaseDir();
	var LogPath = Di.GetRSvc<I_GetBaseDir>().Combine("logs", "app.log");
	```
]
""")]
public class AppDirBaseDirMgr : I_GetBaseDir {
	[Doc("全局單例；入口註冊進容器時用的就是它。")]
	public static AppDirBaseDirMgr Inst => field ??= new AppDirBaseDirMgr();

	[Doc("根目錄本體。默認是可執行檔所在目錄；測試或特殊宿主可以改寫。")]
	public str _BaseDir { get; set; } = AppContext.BaseDirectory;

	[Doc("取根目錄。要拼子路徑時用 I_GetBaseDirExtn.Combine，不要自己接字串。")]
	public str GetBaseDir() {
		return _BaseDir;
	}
}
