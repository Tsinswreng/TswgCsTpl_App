namespace TswgCsTpl.Linux;

using Tsinswreng.CsCore;

[Doc(@"Linux 入口層的可執行入口。

與 `TswgCsTpl.Windows.Program` 的差異只有「本平臺註冊哪個服務實現」這一件，
其餘步驟（建容器、桌面生命週期、配置 AppBuilder）兩者相同。
")]
public static partial class Program{

	[Doc(@"進程入口。

Java 風格的 GUI 線程約定在 Linux 上不存在，但保留 `[STAThread]`，
讓桌面入口層的入口簽名一致（在類 Unix 上該屬性沒有作用）。
#Prm[命令列參數。]
")]
	public static partial void Main(str[] Args);

	[Doc(@"組裝依賴注入容器：註冊本平臺的服務實現。
*必須在建立任何視圖之前*完成並交給業務層的 `Di` 門面。
#Rtn[建好的服務提供源。]
")]
	private static partial IServiceProvider MkServices();
}
