namespace TswgCsTpl.Scripts;

using Tsinswreng.CsCore;

[Doc($$"""
用手寫源碼生成取代「手寫 JSON 登記清單」的腳本。

*要解決的問題*：{nameof(AppJsonCtx)} 需要把所有「要上網線」與「要被 ORM 按名讀寫」的型別登記成
`[JsonSerializable]`。手寫那份清單有三個毛病：容易漏（漏了編譯不會錯、AOT 執行期才炸）、
每個型別還要多寫一條 `IList<T>` 配對、強類型 Id 的轉換器清單也得手動同步。

*怎麼做*：用 Roslyn（MSBuildWorkspace）載入 `proj/TswgCsTpl/TswgCsTpl.csproj` 取得完整編譯，
掃描兩個標記接口的實現者——
`TswgCsTpl.Infra.Json.IAppSerializable`（要登記的型別）與
`Tsinswreng.CsCore.IRawValueSerializable`（要轉換器的值類型）——
然後產出單一檔案 `proj/TswgCsTpl/Tsinswreng.SrcGen/TswgCsTpl.Gen.g.cs`。

*為何是手動跑、不是構建時自動跑*：生成物要入庫（否則首次複製模板會編不過），
而入庫的檔案一旦由構建改寫，diff 會變得不可讀。故照 `Ngan.Dict` 的做法：改完型別自己跑一次。

*怎麼跑*：`dotnet run --project proj/TswgCsTpl.Scripts -- GenAppJsonCtx`
""")]
internal static partial class GenAppJsonCtx {
	[Doc($$"""
	執行一次生成：載入專案 → 收集標記接口的實現者 → 組裝單檔 → 覆寫輸出檔。

	#Prm[{{nameof(Ctx)}}][腳本上下文；用 {nameof(ISCtx.RootDir)} 定位專案與輸出，不依賴當前目錄。]
	#Throw[{{nameof(InvalidOperationException)}}][標記接口找不到，或編譯取不到。]
	""")]
	internal static partial Task Main(ISCtx Ctx, CT Ct);
}
