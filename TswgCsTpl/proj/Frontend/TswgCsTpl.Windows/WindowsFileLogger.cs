namespace TswgCsTpl.Windows;

using System.Text;

using Microsoft.Extensions.Logging;

using TswgCsTpl.Common;

[Doc($$"""
Windows 桌面端的本機診斷日誌器。

*為何不用主控台*：桌面入口的 OutputType 是 WinExe，雙擊啟動時根本沒有終端，
一切主控台輸出都無處可去；啟動失敗與背景錯誤因此無跡可尋。
故日誌固定追加到應用儲存區下的 logs 目錄，排錯時直接打開那一個檔案。

*寫在哪*：路徑由容器裡的 {nameof(I_GetBaseDir)} 決定，本類不假設自己在哪個安裝位置。
*不寫什麼*：配置憑據、使用者資料與任何第三方密鑰。

#Eg[
	```cs
	// 入口處指定內層實現；路徑由容器提供，故這裡不傳任何參數。
	Svc.AddAppLog(new WindowsFileLogger());
	```
]
""")]
sealed class WindowsFileLogger : ILogger {
	[Doc("日誌檔在應用儲存區下的子目錄名。")]
	const str LogDirName = "logs";

	[Doc("日誌檔名。Init.csx 會全域替換專案名，故換新專案後此名字自動跟上。")]
	const str LogFileName = "TswgCsTpl.Windows.log";

	[Doc("序列化追加寫入：日誌可能來自 UI、背景任務、網絡回調多條線程，不加鎖會寫壞同一行。")]
	readonly Lock WriteLock = new();

	[Doc("解析後的路徑快取。延遲到第一次寫入才解析，因為設置日誌的時間點早於 Di.SvcProvider 賦值。")]
	str? _LogPath;

	[Doc($$"""
	取日誌檔路徑：經 {nameof(Di)} 取得應用儲存區根目錄，再拼上子目錄與檔名。
	只解析一次並快取；首次寫入時順帶建立目錄。
	#Rtn[日誌檔的完整路徑。]
	""")]
	str GetLogPath() {
		if (_LogPath is not null) {
			return _LogPath;
		}
		// 1: 根目錄由容器提供，故本日誌器不必也知道自己在哪個安裝位置。
		var Dir = Di.GetRSvc<I_GetBaseDir>().Combine(LogDirName);
		// 2: 首次寫入前確保目錄存在；之後不再檢查。
		Directory.CreateDirectory(Dir);
		return _LogPath = Path.Combine(Dir, LogFileName);
	}

	[Doc($$"""
	本機診斷保留所有非 {nameof(LogLevel)}.{nameof(LogLevel.None)} 等級，
	避免首次裝置或網絡失敗只留下缺少上下文的錯誤行。
	#Rtn[非 None 即為 true。]
	""")]
	public bool IsEnabled(LogLevel LogLevel) {
		return LogLevel != LogLevel.None;
	}

	[Doc("本日誌器不保存 scope；目前也沒有呼叫端依賴 scope 內的資料。")]
	public IDisposable BeginScope<TState>(TState State) where TState : notnull {
		return NoOpScope.Inst;
	}

	[Doc($$"""
	把格式化訊息與完整例外堆棧追加為一個原子日誌項。

	日誌失敗不得反過來讓應用崩潰，故所有 IO 與路徑解析例外都在此吞掉；
	同理，不能用本日誌器記錄自己的失敗，否則會遞迴。

	#Prm[{{nameof(LogLevel)}}][等級。]
	#Prm[{{nameof(EventId)}}][事件識別，目前原樣忽略。]
	#Prm[{{nameof(State)}}][訊息狀態，交給官方格式化委託。]
	#Prm[{{nameof(Exception)}}][可選例外，其堆棧附在訊息之後另起一段。]
	#Prm[{{nameof(Formatter)}}][把狀態與例外組成文字的官方委託。]
	""")]
	public void Log<TState>(
		LogLevel LogLevel
		, EventId EventId
		, TState State
		, Exception? Exception
		, Func<TState, Exception?, str> Formatter
	) {
		if (!IsEnabled(LogLevel)) {
			return;
		}
		try {
			// 1: 組出「時間 等級 訊息」一行；例外另起，方便直接看堆棧。
			var Entry = new StringBuilder()
				.Append(DateTimeOffset.Now.ToString("O"))
				.Append(' ')
				.Append(LogLevel)
				.Append(' ')
				.Append(Formatter(State, Exception));
			if (Exception is not null) {
				Entry.AppendLine().Append(Exception);
			}
			Entry.AppendLine();
			// 2: 整條在鎖內一次寫完，避免與其他線程的日誌交錯。
			lock (WriteLock) {
				File.AppendAllText(GetLogPath(), Entry.ToString(), Encoding.UTF8);
			}
		} catch {
			// 診斷輸出失敗時保持應用可用。
		}
	}

	[Doc("無狀態 scope 的單例，滿足 ILogger 契約而不為每條日誌多做一次配置。")]
	sealed class NoOpScope : IDisposable {
		[Doc("唯一實例；scope 本體沒有狀態。")]
		public static NoOpScope Inst { get; } = new();

		[Doc("沒有資源需要回收。")]
		public void Dispose() { }
	}
}
