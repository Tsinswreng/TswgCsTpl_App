namespace TswgCsTpl.Infra;

public interface I_GetBaseDir{
	
	/// 安卓應用之pwd皆根目錄。故叵直ᵈ由相對路徑㕥載入配置等。
	/// 宜皆由此㕥拼接路徑。
	[Doc(@$"
	表示應用儲存區的根目錄。
	適用于 非Browser之客戶端。
	在Windows上 實現 爲 `Directory.GetCurrentDirectory()`
	在Android上 實現 爲 
	`global::Android.App.Application.Context.GetExternalFilesDir(null).AbsolutePath`
	")]
	public str GetBaseDir();
}

public static class I_GetBaseDirExtn{
	public static str Combine(
		this I_GetBaseDir z
		,params IList<str> Segs
	){
		return Path.Combine([z.GetBaseDir(), ..Segs]);
	}
}
