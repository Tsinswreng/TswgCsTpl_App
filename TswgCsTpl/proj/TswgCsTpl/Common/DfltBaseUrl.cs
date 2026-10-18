namespace TswgCsTpl.Common;

using Tsinswreng.CsCore;

[Doc($$"""
{I_GetBaseUrl} 的默認實現：本機服務端。

*為何不是寫死在調用處*：換部署（本機／局域網／雲端）時只改這一個欄位；
瀏覽器宿主日後可換成「同源相對位址」的實現（WASM 下與頁面同源即可），而不必動調用方。
""")]
public class DfltBaseUrl : I_GetBaseUrl {
	[Doc("全局單例。")]
	public static DfltBaseUrl Inst => field ??= new DfltBaseUrl();

	[Doc("基地址本體；不含結尾斜線。啟動時可由宿主覆寫（例如從配置讀）。")]
	public str _BaseUrl { get; set; } = "http://localhost:5080";

	public str GetBaseUrl() {
		return _BaseUrl;
	}
}
