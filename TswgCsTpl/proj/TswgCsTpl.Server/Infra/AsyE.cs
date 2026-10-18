namespace TswgCsTpl.Server.Infra;

using Tsinswreng.CsCore;

[Doc("把單個元素包成 `IAsyncEnumerable`。CsSql 的倉儲接口一律吃流，單筆場合用它適配。")]
public static class AsyE {
	[Doc($$"""
	單元素流。
	#Prm[{{nameof(Item)}}][要輸出的唯一元素。]
	#Rtn[只含該元素的流。]
	""")]
	public static async IAsyncEnumerable<T> One<T>(T Item) {
		// 逐個 yield；此處只有一個，故不涉及延遲求值的複雜性。
		yield return Item;
		await Task.CompletedTask;
	}
}
