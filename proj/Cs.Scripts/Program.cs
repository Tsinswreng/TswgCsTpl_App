namespace Cs.Scripts;

using System.Runtime.CompilerServices;
using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

public interface ISCtx{
	public Pth RootDir{get;set;}
}

public class SCtx:ISCtx{
	public Pth RootDir{get;set;}
}

/// 模板維護腳本的命令列入口。
/// 第一個引數選擇具體腳本；腳本本身負責完整的一次性流程。
internal static partial class Program{
	/// 將命令列入口分派至具名腳本。
	internal static async Task Main(str[] Args){
		var Ct = default(CT);
		// Program.cs 位於 <根>/proj/Cs.Scripts；上推兩級得到倉庫根。
		var Root = FullPath(DirName(OwnPath())/"../..");

		// step 1: 建立統一的子命令上下文，所有子命令都從中取得 RootDir 等共用資料。
		ISCtx Ctx = new SCtx { RootDir = Root };

		if(Args.Length == 0){
			PrintUsage();
			return;
		}

		// step 2: 依首個引數分派；新命令在此與 PrintUsage 各加一行。
		switch(Args[0]){
			case nameof(Test):
				await Test.Main(Ctx, Ct);
				break;
			case nameof(TestAot):
				await TestAot.Main(Ctx, Ct);
				break;
			default:
				throw new ArgumentException($"Unknown Cs script: {Args[0]}.", nameof(Args));
		}
	}

	/// 列出可由 dotnet run -- <entry> 呼叫的腳本名稱。
	private static void PrintUsage(){
		Console.Error.WriteLine("Usage: dotnet run --project proj/Cs.Scripts -- <entry>");
		Console.Error.WriteLine("Entries: Test, TestAot");
	}

	/// 讓編譯器提供腳本源文件路徑，故腳本不依賴啟動時的當前目錄。
	private static str OwnPath([CallerFilePath] str CallerPath = ""){
		return CallerPath;
	}
}
