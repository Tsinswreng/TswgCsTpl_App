namespace TswgCsTpl.LangPack;

using Tsinswreng.CsCore;
using Tsinswreng.CsI18n;
using TswgCsTpl.Infra.I18n;
using TswgCsTpl.I18nPack;

[Doc($$"""
構建期工具：把 C\# 裏的譯文導出成「每種語言一個 JSON」。

*為何要有這一步*：譯文寫在 C\# 裡，鍵與譯文一起受編譯檢查，寫起來也方便（多行不必轉義）；
但主程式不該把用不到的語言也編進組件、也不該在啟動時建好所有語言。
故：
+ 譯文留在 {{nameof(Pack)}} 那個專案（只有本工具引用它，主程式不引用）；
+ 本工具在構建時導出 `Languages/<語言>.json`；
+ 主程式執行時只讀當前語言的 JSON（見 `AppI18n`）。

*順便當翻譯檢查*：導出前先跑 {{nameof(I18nSet.Verify)}}（界面文案一組、錯誤文案一組），
有問題就讓構建失敗——漏翻、翻成空白都會在這一步被擋住，而不是等到用戶看見鍵名。
""")]
public static class Program {
	[Doc($$"""
	#Sum[導出全部語言包。]
	#Descr[
	用法：`dotnet run --project proj/TswgCsTpl.LangPack -- <輸出目錄>`
	#Prm[
		[輸出目錄；省略時寫到 `./Languages`。]
	]
	#Rtn[0 表示成功；1 表示翻譯有問題或寫檔失敗（構建腳本看退出碼）。]
	]
	""")]
	public static i32 Main(str[] Args) {
		var OutDir = Args is { Length: > 0 } ? Args[0] : "Languages";
		try {
			return Mk(OutDir);
		}
		catch (Exception e) {
			Console.Error.WriteLine("語言包生成失敗：" + e.Message);
			Console.Error.WriteLine(e);
			return 1;
		}
	}

	[Doc($$"""
	導出的主體：自檢 → 逐語言寫檔。
	#Prm[{{nameof(OutDir)}}][輸出目錄。]
	#Rtn[進程退出碼。]
	""")]
	static i32 Mk(str OutDir) {
		// 1: 先把全部語言裝進集合（此時才需要譯文，主程式不會做這件事）。
		var Set = Pack.MkSet();

		// 2: 語言清單要與主程式那份一致——主程式選語言時靠它，
		//    不一致會變成「選得到、載不到」的執行時錯誤。
		CheckLanges();

		// 3: 覆蓋率自檢。界面文案一組、錯誤文案一組——漏翻或翻成空白都在這裏擋住。
		var Problems = Set.Verify(KeysI18n.All, KeysErr.All);
		if (Problems.Count > 0) {
			Console.Error.WriteLine($"翻譯有 {Problems.Count} 個問題：");
			foreach (var Problem in Problems) {
				Console.Error.WriteLine("  " + Problem);
			}
			return 1;
		}

		// 4: 每種語言一個檔案；鍵→模板平鋪寫出（讀取端兩種形狀都吃得下）。
		foreach (var Lang in Pack.Langes) {
			var Path = System.IO.Path.Combine(OutDir, Lang + ".json");
			I18nJson.SaveFile(Path, Set.Dicts[Lang].Map);
			Console.WriteLine($"已生成 {Path}（{Set.Dicts[Lang].Map.Count} 條）");
		}
		return 0;
	}

	[Doc($$"""
	比對「主程式認為有的語言」與「語言包專案真的有的語言」。
	#Throw[{{nameof(InvalidOperationException)}}][兩份清單不一致時。]
	""")]
	static void CheckLanges() {
		var Missing = AppI18n.Langes.Except(Pack.Langes).ToList();
		var Extra = Pack.Langes.Except(AppI18n.Langes).ToList();
		if (Missing.Count == 0 && Extra.Count == 0) {
			return;
		}
		throw new InvalidOperationException(
			$"主程式與語言包專案的語言清單不一致。主程式多出：[{str.Join(",", Missing)}]；語言包多出：[{str.Join(",", Extra)}]"
		);
	}
}
