using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

namespace Cs.Scripts;

/// TestProjects 的流程實現。
internal static partial class TestProjects{
	/// 依 Glob 模式 proj/*.Test/*.csproj 定位；模板用 RenameProj 改名後專案名改變也能自動命中。
	internal static partial Pth Find(Pth RootDir){
		var Matches = Glob(RootDir/"proj/*.Test/*.csproj").ToList();
		if(Matches.Count == 0){
			throw new InvalidOperationException($"在 {RootDir}/proj 下找不到任何 *.Test 測試專案。");
		}
		if(Matches.Count > 1){
			throw new InvalidOperationException($"在 {RootDir}/proj 下找到多個 *.Test 測試專案，無法確定唯一目標。");
		}
		return Matches[0];
	}
}