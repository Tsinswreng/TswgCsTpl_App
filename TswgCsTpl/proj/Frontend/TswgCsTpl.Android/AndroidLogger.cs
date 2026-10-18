namespace TswgCsTpl.Android;

using Microsoft.Extensions.Logging;
using AndroidLog = global::Android.Util.Log;

[Doc($$"""
Android 端的日誌實現：把日誌轉發到 logcat。

*為何不用主控台*：Android 上 Console 輸出雖然也會進 logcat，但沒有固定 tag、
等級映射也不受控，`adb logcat` 過濾時分不清是本應用還是別人的輸出。
本類固定 tag 與等級映射，Release 版也能直接看。

#Eg[
	```cs
	// 入口處指定內層實現；tag 取應用名稱，與其他宿主的日誌分類一致。
	Svc.AddAppLog(new AndroidLogger(TswgCsTpl.Ui.App.AppName));
	```
]
""")]
public class AndroidLogger : ILogger {
	[Doc("logcat 的 tag：同一應用的日誌可用 `adb logcat -s 應用名稱` 單獨看。")]
	readonly str _Tag;

	[Doc("低於此等級的日誌直接丟棄，避免 Verbose 級把 logcat 刷滿。")]
	readonly LogLevel _MinLevel;

	[Doc($$"""
	建立 Android 日誌器。
	#Prm[{{nameof(Tag)}}][logcat 的 tag，通常傳應用名稱。]
	#Prm[{{nameof(MinLevel)}}][最低保留等級，默認 {nameof(LogLevel)}.{nameof(LogLevel.Debug)}。]
	""")]
	public AndroidLogger(str Tag, LogLevel MinLevel = LogLevel.Debug) {
		_Tag = Tag;
		_MinLevel = MinLevel;
	}

	[Doc("本日誌器不保存 scope；logcat 沒有對應的 scope 概念。")]
	public IDisposable BeginScope<TState>(TState State) where TState : notnull {
		return NoOpDisposable.Inst;
	}

	[Doc($$"""
	是否啟用某等級：低於下限或為 {nameof(LogLevel)}.{nameof(LogLevel.None)} 都不啟用。
	#Rtn[等級在下限之上且不是 None 時為 true。]
	""")]
	public bool IsEnabled(LogLevel LogLevel) {
		return LogLevel >= _MinLevel && LogLevel != LogLevel.None;
	}

	[Doc($$"""
	寫一條日誌到 logcat：等級按 Android 的五檔映射，例外附在訊息之後。
	#Prm[{{nameof(LogLevel)}}][等級。]
	#Prm[{{nameof(EventId)}}][事件識別，logcat 沒有對應欄位，故忽略。]
	#Prm[{{nameof(State)}}][訊息狀態，交給官方格式化委託。]
	#Prm[{{nameof(Exception)}}][可選例外，其堆棧接在訊息之後。]
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
		// 1: 先組訊息；空訊息又沒有例外時不必要地佔一行，故直接返回。
		var Message = Formatter(State, Exception);
		if (string.IsNullOrWhiteSpace(Message) && Exception is null) {
			return;
		}
		if (Exception is not null) {
			Message = string.IsNullOrWhiteSpace(Message)
				? Exception.ToString()
				: Message + "\n" + Exception;
		}
		// 2: 映射到 Android 的等級；logcat 只有五檔，故 Trace 併入 Debug、Critical 併入 Error。
		switch (LogLevel) {
			case LogLevel.Trace:
			case LogLevel.Debug:
				AndroidLog.Debug(_Tag, Message);
				break;
			case LogLevel.Information:
				AndroidLog.Info(_Tag, Message);
				break;
			case LogLevel.Warning:
				AndroidLog.Warn(_Tag, Message);
				break;
			case LogLevel.Error:
			case LogLevel.Critical:
				AndroidLog.Error(_Tag, Message);
				break;
		}
	}

	[Doc("無狀態 scope 的單例，滿足 ILogger 契約而不產生額外配置。")]
	sealed class NoOpDisposable : IDisposable {
		[Doc("唯一實例；scope 本體沒有狀態。")]
		public static NoOpDisposable Inst { get; } = new();

		[Doc("沒有資源需要回收。")]
		public void Dispose() { }
	}
}
