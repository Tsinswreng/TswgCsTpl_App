namespace TswgCsTpl.Infra.Json;

using System.Text.Json;
using System.Text.Json.Serialization;
using Tsinswreng.CsErr;
using Tsinswreng.CsErr.Results;
using Tsinswreng.CsCore;
using TswgCsTpl.Shared.User.Models.Req;
using TswgCsTpl.Shared.User.Models.Resp;

[Doc($$"""
本應用的 JSON 源生成上下文（`System.Text.Json` 源生成）。

*為何一定要有它*：專案是 NativeAOT，運行時反射式序列化會被裁剪掉、直接在執行期拋例外。
凡是「上網線」的型別都必須在此登記，否則症狀是「本機 debug 正常、AOT 發佈後 500」。

*為何成功響應用 `WebAns<T>`（泛型具體型別）而不是 `WebAns<obj>`*：
`Data` 宣告成 `object` 時源生成器不知道要按什麼型別寫，AOT 下會失敗。
故每個端點用自己的 `WebAns<具體DTO>`；只有「空數據 + 錯誤」這種場合才用 `WebAns<obj>`（Data 為 null，不涉及多態）。
""")]
[JsonSerializable(typeof(ReqRegister))]
[JsonSerializable(typeof(ReqLogin))]
[JsonSerializable(typeof(ReqLogout))]
[JsonSerializable(typeof(RespLogin))]
[JsonSerializable(typeof(RespUser))]
[JsonSerializable(typeof(WebAns<RespLogin>))]
[JsonSerializable(typeof(WebAns<RespUser>))]
[JsonSerializable(typeof(WebAns<obj>))]
[JsonSerializable(typeof(TypedErrView))]
[JsonSerializable(typeof(IList<TypedErrView>))]
public partial class AppJsonCtx : JsonSerializerContext {
	[Doc($$"""
	線上一律使用的選項：不轉命名風格（DTO 已是 PascalCase）、不寫縮排、null 不輸出。

	*為何 `PropertyNamingPolicy = null`*：前後端都認同一份 `AppJsonCtx`，
	統一是最省心的；改策略等於改變對外協議，要兩端同時發版。
	""")]
	public static JsonSerializerOptions WireOptions { get; } = new() {
		PropertyNamingPolicy = null,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	};

	[Doc("線上一律使用的上下文實例。")]
	public static AppJsonCtx Wire { get; } = new(WireOptions);
}
