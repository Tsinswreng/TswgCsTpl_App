namespace Cs.Scripts;

/// 以 NativeAOT 發布測試專案並運行發布物，驗證 AOT 剪裁後測試仍全部通過。
internal static partial class TestAot{
	/// 執行：publish -c Release -r <Rid> → 運行 publish 目錄下之測試 exe。
	internal static partial Task Main(ISCtx Ctx, CT Ct);
}