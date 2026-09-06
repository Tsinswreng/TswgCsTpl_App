using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

namespace Cs.Scripts;

/// TestAot 命令的流程實現；NativeAOT 開關由模板的 Directory.Build.props 統一提供。
internal static partial class TestAot{
	/// 依作業系統選擇 RID，允許 AOT_TEST_RID 覆蓋預設值以便 CI 或交叉編譯。
	internal static async partial Task Main(ISCtx Ctx, CT Ct){
		var TestProject = TestProjects.Find(Ctx.RootDir);
		var TestDir = DirName(TestProject);
		// csproj 檔名即 AssemblyName，去掉 .csproj 後綴就是發布物 exe 的名稱。
		var ProjectName = BaseName(TestProject).Value;
		ProjectName = ProjectName[..^".csproj".Length];

		var Rid = GetEnv("AOT_TEST_RID");
		if(String.IsNullOrWhiteSpace(Rid)){
			Rid = OperatingSystem.IsWindows() ? "win-x64" : "linux-x64";
		}

		// step 1: 以 Release 配置發布 NativeAOT 測試 executable。
		await Exe("dotnet", ["publish", TestProject, "-c", "Release", "-r", Rid], Ct);

		// step 2: 定位 publish 目錄中的原生測試程式並運行；TFM 不硬編碼，用 Glob 動態匹配。
		var ExeName = OperatingSystem.IsWindows() ? ProjectName + ".exe" : ProjectName;
		var PublishMatches = Glob(TestDir/"bin/Release/*"/Rid/"publish"/ExeName).ToList();
		if(PublishMatches.Count != 1){
			throw new InvalidOperationException($"未找到唯一的 AOT 發布產物: {TestDir}/bin/Release/*/{Rid}/publish/{ExeName}，實際匹配 {PublishMatches.Count} 個。");
		}
		await Exe(PublishMatches[0], [], Ct);
	}
}