namespace TswgCsTpl.Log;

using Microsoft.Extensions.Logging;

[Doc($$"""
建立「寫到標準輸出」的內層日誌器，供 Linux／Browser 這類本來就有輸出的宿主共用。

*為何要有這個類*：`LoggerFactory.Create` 每次呼叫都會造一整套 provider 與背景佇列，
四個入口各寫一遍就是各漏一個；這裡只造一個，且把「主控台」這個選擇集中在一處，
日後要加檔案、結構化等 sink 時也只改這裡。
""")]
public static class MkrLogger {
	[Doc("進程內共用的主控台工廠；進程結束即隨之回收，故不提供 Dispose。")]
	static readonly Lazy<ILoggerFactory> ConsoleFactory = new(MkConsoleFactory);

	[Doc("建立主控台工廠：Linux 寫 stdout，Browser 寫開發者工具的 Console（WASM 下 stdout 被導向那裡）。")]
	static ILoggerFactory MkConsoleFactory() {
		return LoggerFactory.Create(B => B.AddConsole());
	}

	[Doc($$"""
	取一個寫到主控台的日誌器。
	#Prm[{{nameof(Category)}}][分類名。宿主通常傳應用名稱，故同一應用的日誌行有共同前綴。]
	#Rtn[可直接當 {nameof(AppLog)}.Inst 內層實現的日誌器。]
	""")]
	public static ILogger Console(str Category) {
		return ConsoleFactory.Value.CreateLogger(Category);
	}
}
