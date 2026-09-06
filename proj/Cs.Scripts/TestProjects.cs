using Tsinswreng.CsSh;

namespace Cs.Scripts;

/// 定位模板測試專案的共用工具，供 Test 與 TestAot 使用。
internal static partial class TestProjects{
	/// 在 RootDir 的 proj/ 下找到唯一的 *.Test 測試專案 csproj；找不到或超過一個時拋異常。
	internal static partial Pth Find(Pth RootDir);
}