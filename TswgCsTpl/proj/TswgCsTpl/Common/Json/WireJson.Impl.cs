namespace TswgCsTpl.Infra.Json;

using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;
using Tsinswreng.CsErr;
using Tsinswreng.CsErr.Results;

[Doc($$"""
{nameof(WireJson)} 的函數實現。
""")]
public static partial class WireJson {
	public static partial str Ans<T>(T? Data) {
		// 包成 WebAns<T> 再交給源生成 context；T 用具體型別，源生成器才知道要按什麼寫。
		var Resp = new WebAns<T> { Data = Data };
		return AppJson.Inst.Serialize(Resp, typeof(WebAns<T>), AppJsonCtx.Default);
	}

	public static partial str Req<TReq>(TReq Req) {
		// 請求體用「Type ＋ context」重載：TReq 必須在生成檔裡登記過，否則這裡直接拋。
		return AppJson.Inst.Serialize(Req, typeof(TReq), AppJsonCtx.Default);
	}

	public static partial str Err(IList<ITypedErrView> Views) {
		// 錯誤包絡固定按 IWebAns<obj> 序列化：Data 為 null 會被 WhenWritingNull 略過，
		// 故實際輸出只有 Errors——這也是唯一允許用 obj 作數據型別的場合。
		return AppJson.Inst.Serialize(WebAns.Mk(null, Views), typeof(IWebAns<obj>), AppJsonCtx.Default);
	}

	public static partial TResp? ReadAns<TResp>(str Json)
		where TResp : class
	{
		// 1: Errors 按具體型別 TypedErrView 解析（接口無法被 STJ 實例化，故不能用 IList<ITypedErrView>）。
		var ErrorsTi = GetTypeInfo<IList<TypedErrView>>();
		// 2: 交給 CsErr.Results 的解析器：它自己拆 Data／Errors，兩邊都用源生成。
		var Ans = WebAns.Deserialize<TResp>(
			Json
			, ErrorsTi
			, new Func<Type, JsonTypeInfo>(GetTypeInfo)
		);
		// 3: 有錯誤就拋 TypedErr，鍵與參數原樣帶回，由上層用同一套 I18n 翻譯。
		if (Ans.Errors is { Count: > 0 } Errors) {
			throw TypedErr.FromViews([.. Errors]);
		}
		return Ans.Data;
	}

	[Doc($$"""
	取登記過的型別元資料；沒登記就直接拋，而不是讓 STJ 稍後給出難懂的訊息。

	#TPrm[{nameof(T)}][要查的型別。]
	#Rtn[對應的 {nameof(JsonTypeInfo)}。]
	#Throw[{{nameof(InvalidOperationException)}}][型別未登記在 {nameof(AppJsonCtx)}。]
	""")]
	static JsonTypeInfo<T> GetTypeInfo<T>() {
		return AppJsonCtx.Default.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>
			?? throw new InvalidOperationException($"型別未登記於 AppJsonCtx：{typeof(T).Name}");
	}

	[Doc($$"""
	{nameof(JsonTypeInfo)} 的非泛型版本（供 {nameof(WebAns.Deserialize)} 的委託參數使用）。

	#Prm[{{nameof(Type)}}][要查的型別。]
	#Rtn[對應的 {nameof(JsonTypeInfo)}。]
	#Throw[{{nameof(InvalidOperationException)}}][型別未登記在 {nameof(AppJsonCtx)}。]
	""")]
	static JsonTypeInfo GetTypeInfo(Type Type) {
		return AppJsonCtx.Default.GetTypeInfo(Type)
			?? throw new InvalidOperationException($"型別未登記於 AppJsonCtx：{Type.Name}");
	}
}
