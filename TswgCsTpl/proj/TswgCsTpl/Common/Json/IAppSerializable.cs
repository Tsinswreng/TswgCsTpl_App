namespace TswgCsTpl.Infra.Json;

using Tsinswreng.CsCore;

[Doc($$"""
標記接口：實現它 ＝ 這個型別要進 JSON 源生成上下文（{{nameof(AppJsonCtx)}}）。

*為何要有這層標記*：NativeAOT 下沒有反射式序列化，凡是要上網線、或被 ORM 按名讀寫成員的型別，
都必須在 {{nameof(AppJsonCtx)}} 上有 `[JsonSerializable]`。手寫那份清單早晚會漏
（漏了不會編譯錯，只會在 AOT 執行期才炸），故改成「型別自己聲明」＋「腳本掃描生成」。

*誰在掃*：`proj/TswgCsTpl.Scripts` 的 `GenAppJsonCtx`——用 Roslyn 載入本專案、
收集所有實現／繼承本接口的型別（含繼承它的接口，例如 `IPoBase`），產出
`proj/TswgCsTpl/Tsinswreng.SrcGen/TswgCsTpl.Gen.g.cs`。

*要打在哪*：打在「會被 JSON 碰到的介面或具體類型」上，而不是每個實作類各打一次——
例如打在 `IPoBase` 上，三個實體就都自動被收集。
外部型別（例如 `WebAns<T>`、`TypedErrView`）無法實作本接口，改由生成器的
`ExtraJsonTypes` 清單顯式列出。

#Eg[
	```cs
	// 實體：打在公共介面上，全部實體一起被收集
	public interface IPoBase : IAppSerializable { /* … */ }

	// 傳輸對象：直接打在具體類上
	public class ReqLogin : IAppSerializable { /* … */ }
	```
]
""")]
public interface IAppSerializable{
}
