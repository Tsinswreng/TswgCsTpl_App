namespace TswgCsTpl.Infra.Json;

using Tsinswreng.CsCore;
using Tsinswreng.CsErr;
using Tsinswreng.CsErr.Results;

[Doc($$"""
線上的「響應包絡」編解碼：把 {{nameof(WebAns<>)}} 這個約定收在唯一一處。

*為何要集中*：包絡的形狀（`Data` ＋ `Errors`）、錯誤怎麼還原成
{{nameof(TypedErr)}}、用哪份源生成 context——這三件事若在服務端與前端各寫一遍，
早晚一邊改了另一邊沒跟上，症狀是「本機正常、另一端口徑不同」。故只留這一組函數：
服務端只用 {nameof(Ans)}／{nameof(Err)}，前端只用 {nameof(Req)}／{nameof(ReadAns)}。

*為何全部走 {nameof(JsonSerializerContext)} 重載*：這是 STJ 的 AOT 安全路徑——
型別資訊由源生成提供，不經過反射。用 `Type` ＋ 源生成 context 的寫法在 NativeAOT 下不會觸發 IL2026／IL3050。

*錯誤包絡為何不能直接 `Deserialize<WebAns<T>>`*：{{nameof(WebAns<>)}}.Errors 的宣告型別是
`IList<{{nameof(ITypedErrView)}}>`，元素是接口；STJ 無法實例化接口，直接反序列化整包會在
「錯誤響應」這一路上拋 `NotSupportedException`——而那正是最需要正確處理的一路。
故 {nameof(ReadAns)} 走 `WebAns.Deserialize<T>`（`Tsinswreng.CsErr.Results` 提供）：
它自己拆 `Data`／`Errors`，`Errors` 按具體型別 `{{nameof(TypedErrView)}}` 解析。

#Eg[
	```cs
	// 服務端：成功帶數據 / 只回錯誤
	Results.Text(WireJson.Ans(new RespUser{ /* … */ }), "application/json; charset=utf-8");
	var Body = WireJson.Err([TeView]);

	// 前端：把響應體還原成 Data，或把錯誤包絡拋成 TypedErr
	var User = WireJson.ReadAns<RespUser>(Body);
	```
]
""")]
public static partial class WireJson {
	[Doc($$"""
	把成功響應序列化成 JSON：`{ "Data": … }`。

	#TPrm[T][數據的具體型別；每個端點用自己的 DTO，不要用 {nameof(obj)}（源生成器不知道按什麼寫）。]
	#Prm[{{nameof(Data)}}][要回傳的數據；`null` 時 `Data` 不會出現在 JSON 裡。]
	#Rtn[可直接寫進 HTTP 響應體的 JSON 字串。]
	""")]
	public static partial str Ans<T>(T? Data);

	[Doc($$"""
	把請求體序列化成 JSON。

	#TPrm[{nameof(TReq)}][請求 DTO 型別；必須登記在 {nameof(AppJsonCtx)}（生成器會掃 {nameof(IAppSerializable)}）。]
	#Prm[{{nameof(Req)}}][請求物件。]
	#Rtn[JSON 字串。]
	""")]
	public static partial str Req<TReq>(TReq Req);

	[Doc($$"""
	把錯誤包絡序列化成 JSON：`{ "Errors": [ … ] }`（`Data` 為 null，故被自動略過）。

	#Prm[{{nameof(Views)}}][異常視圖；一般只有一項。]
	#Rtn[JSON 字串。]
	""")]
	public static partial str Err(IList<ITypedErrView> Views);

	[Doc($$"""
	把響應體還原成 {nameof(WebAns<T>.Data)}：包絡裡有錯誤時拋 {nameof(TypedErr)}。

	#TPrm[{nameof(TResp)}][期望的數據型別，對應服務端那個端點的 `WebAns<TResp>`。]
	#Prm[{{nameof(Json)}}][響應體全文。]
	#Rtn[數據；包絡裡 `Data` 為 null 時返回 null。]
	#Throw[{{nameof(TypedErr)}}][包絡 `Errors` 非空時，鍵與參數原樣帶回，供上層用同一套 I18n 翻譯。]
	#Throw[{{nameof(InvalidOperationException)}}][型別沒登記在 {nameof(AppJsonCtx)}（生成器漏掃或漏列）。]
	""")]
	public static partial TResp? ReadAns<TResp>(str Json)
		where TResp : class
	;
}
