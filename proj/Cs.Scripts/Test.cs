namespace Cs.Scripts;

/// 以普通（非 AOT）方式運行測試專案，驗證測試通過。
internal static partial class Test{
	/// 執行 dotnet run 運行測試專案；失敗時以非零退出碼結束。
	internal static partial Task Main(ISCtx Ctx, CT Ct);
}