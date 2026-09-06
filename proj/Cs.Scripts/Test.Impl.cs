using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

namespace Cs.Scripts;

/// Test 命令的流程實現；所有外部命令均透過 CsSh 執行。
internal static partial class Test{
	/// 定位測試專案並以 dotnet run 直接運行，驗證普通（非 AOT）環境下測試通過。
	internal static async partial Task Main(ISCtx Ctx, CT Ct){
		var TestProject = TestProjects.Find(Ctx.RootDir);
		// dotnet run 會自動還原並建置；輸出必須轉送終端，否則看不到測試結果。
		await Exe("dotnet", ["run", "--project", TestProject], Ct);
	}
}