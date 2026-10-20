namespace TswgCsTpl.Infra;

using Tsinswreng.CsCore;
using Tsinswreng.CsRefl;
using TswgCsTpl.Infra.Json;

[Doc($$"""
全應用的型別元資料來源（CsSql 建表、倉儲映射、按名讀寫屬性都靠它）。

*兩條來源串起來*：先問 {nameof(JsonTypeInfoSrc)}（登記在 {{nameof(AppJsonCtx)}} 的型別走 STJ 源生成，
NativeAOT 下可用），沒登記的再落到 {nameof(ReflTypeInfoSrc)} 兜底。

*為何要這條順序、而不是讓實體走反射*：CsSql 建表時要枚舉實體成員；在 AOT 下反射成員會被裁剪，
故凡是「要當表用的實體」都必須能被來源靜態列舉。實體靠 `IPoBase : IAppSerializable` 進
{{nameof(AppJsonCtx)}}，於是它們走第一條來源，不需要反射。

*為何傳 {nameof(AppJsonCtx.WireOptions)} 而不是 {{nameof(AppJsonCtx)}}.Default*：
前者已經顯式掛好 `TypeInfoResolver`（{nameof(JsonTypeInfoSrc)} 的建構期檢查要求非空），
且與線上序列化用的是同一份選項與轉換器，兩邊解析出的成員元資料不會有第二種口徑。
""")]
public static class AppTypeInfoSrc {
	static ITypeInfoSrc? _Inst;

	[Doc("全局唯一實例；兩條來源各自有快取，故只建一次。")]
	public static ITypeInfoSrc Inst => _Inst ??= new MergedTypeInfoSrc(
		new JsonTypeInfoSrc(AppJsonCtx.WireOptions)
		, new ReflTypeInfoSrc()
	);
}
