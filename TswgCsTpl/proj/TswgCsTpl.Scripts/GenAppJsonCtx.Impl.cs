using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Tsinswreng.CsCore;
using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

namespace TswgCsTpl.Scripts;

[Doc($$"""
{nameof(GenAppJsonCtx)} 的流程實現。
""")]
internal static partial class GenAppJsonCtx {
	/// 要分析的專案；相對於倉庫根（{{nameof(ISCtx.RootDir)}}）。
	const str CsprojRelPath = "proj/TswgCsTpl/TswgCsTpl.csproj";

	/// 生成物路徑；相對於倉庫根。必須入庫，否則模板第一次複製出來會編不過。
	const str OutRelPath = "proj/TswgCsTpl/Tsinswreng.SrcGen/TswgCsTpl.Gen.g.cs";

	/// 標記接口：實現／繼承它者，登記 `[JsonSerializable]`。
	const str MarkerAppSerializable = "TswgCsTpl.Infra.Json.IAppSerializable";

	/// 標記接口：實現它者，登記一個 `JsonConvtr<T>`（強類型值扁平化成裸值）。
	const str MarkerRawValueSerializable = "Tsinswreng.CsCore.IRawValueSerializable";

	/// 生成檔的命名空間；必須與 `AppJsonCtx.cs` 一致，否則 partial 接不起來。
	const str Ns = "TswgCsTpl.Infra.Json";

	/// 手動額外型別：外部型別無法實作本專案的標記接口，只能在此列出。
	/// 這裡都是傳輸包絡相關的型別（`WebAns` 是泛型閉包，不能打標記）。
	static readonly str[] ExtraJsonTypes = [
		// 成功響應：每個端點一個具體閉包，源生成器才知道 Data 要按什麼寫。
		"global::Tsinswreng.CsErr.Results.WebAns<global::TswgCsTpl.Shared.User.Models.Resp.RespLogin>",
		"global::Tsinswreng.CsErr.Results.WebAns<global::TswgCsTpl.Shared.User.Models.Resp.RespUser>",
		// 「空數據 + 錯誤」這種場合用 obj，Data 為 null，不涉及多態。
		"global::Tsinswreng.CsErr.Results.WebAns<global::System.Object>",
		// 錯誤包絡按介面型別序列化（GlobalErrHandler）。
		"global::Tsinswreng.CsErr.Results.IWebAns<global::System.Object>",
		// 錯誤視圖：具體型別供反序列化，介面型別供按介面序列化。
		"global::Tsinswreng.CsErr.TypedErrView",
		"global::Tsinswreng.CsErr.ITypedErrView",
		"global::System.Collections.Generic.IList<global::Tsinswreng.CsErr.TypedErrView>",
		"global::System.Collections.Generic.IList<global::Tsinswreng.CsErr.ITypedErrView>",
	];

	internal static async partial Task Main(ISCtx Ctx, CT Ct) {
		// step 1: 定位本機 MSBuild，並以 MSBuildWorkspace 載入目標專案
		//（解析邏輯與 dotnet build 相同：ProjectReference、NuGet、框架引用都照它）。
		MSBuildLocator.RegisterDefaults();
		using var Workspace = MSBuildWorkspace.Create();
		Workspace.WorkspaceFailed += (_, E) => Console.Error.WriteLine($"[workspace] {E.Diagnostic.Message}");

		var Project = await Workspace.OpenProjectAsync(Ctx.RootDir/CsprojRelPath, null, Ct);
		var Compilation = await Project.GetCompilationAsync(Ct)
			?? throw new InvalidOperationException("GetCompilationAsync 返回 null：專案載入失敗。");

		// step 2: 收集兩個標記接口的實現者。轉換器只能用於 struct（JsonConvtr<T> 的泛型約束）。
		var AppTypes = FindImplementers(Compilation, MarkerAppSerializable).ToArray();
		var RawTypes = FindImplementers(Compilation, MarkerRawValueSerializable)
			.Where(T => T.IsValueType && !T.IsAbstract)
			.ToArray();

		// step 3: 組裝單檔並覆寫。統一換行為 LF，避免生成檔混用 CRLF／LF 造成整檔 diff。
		var Code = BuildCode(AppTypes, RawTypes).Replace("\r\n", "\n");
		var OutPath = Ctx.RootDir/OutRelPath;
		Write(OutPath, Code);
		Echo($"GenAppJsonCtx: {AppTypes.Length} app types, {RawTypes.Length} raw types -> {OutPath}");
	}

	[Doc($$"""
	依全限定名找標記接口，收集編譯單元裡所有實現／繼承它的命名型別。

	*為何也要收介面*：標記打在 `IPoBase`／`IIdUInt128` 這類公共介面上，
	介面本身也該有 metadata（按介面型別序列化時要用）。
	*為何要排序*：生成檔會入庫，成員順序一變就是整檔 diff；按全限定名排序可保證輸出穩定。
	""")]
	static IEnumerable<INamedTypeSymbol> FindImplementers(Compilation Compilation, str MarkerFullName) {
		var Marker = Compilation.GetTypeByMetadataName(MarkerFullName)
			?? throw new InvalidOperationException($"標記接口找不到：{MarkerFullName}");
		return Compilation.SourceModule.GlobalNamespace
			.GetAllNamedTypes()
			.Where(T => !SymbolEqualityComparer.Default.Equals(T, Marker) && IsSubType(T, Marker))
			.OrderBy(T => T.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), StringComparer.Ordinal);
	}

	[Doc("判斷 Candidate 是否為 Marker 之子型別：實作／繼承 Marker。")]
	static bool IsSubType(INamedTypeSymbol Candidate, INamedTypeSymbol Marker) {
		if(Candidate.AllInterfaces.Contains(Marker, SymbolEqualityComparer.Default)){
			return true;
		}
		for(var Base = Candidate.BaseType; Base is not null; Base = Base.BaseType){
			if(SymbolEqualityComparer.Default.Equals(Base, Marker)){
				return true;
			}
		}
		return false;
	}

	[Doc("遞歸收集命名空間下所有命名型別（含嵌套命名空間與嵌套型別）。")]
	static IEnumerable<INamedTypeSymbol> GetAllNamedTypes(this INamespaceSymbol Ns) {
		foreach(var Member in Ns.GetMembers()){
			switch(Member){
				case INamespaceSymbol NestedNs:
					foreach(var T in NestedNs.GetAllNamedTypes()){
						yield return T;
					}
					break;
				case INamedTypeSymbol Type:
					yield return Type;
					foreach(var Nested in Type.GetAllNestedTypes()){
						yield return Nested;
					}
					break;
			}
		}
	}

	[Doc("遞歸收集嵌套型別。")]
	static IEnumerable<INamedTypeSymbol> GetAllNestedTypes(this INamedTypeSymbol Type) {
		foreach(var Nested in Type.GetTypeMembers()){
			yield return Nested;
			foreach(var Deeper in Nested.GetAllNestedTypes()){
				yield return Deeper;
			}
		}
	}

	[Doc($$"""
	組裝單檔內容。

	*為何三個 partial 宣告、而不是一個*：`[JsonSerializable]` 必須全部集中同一個宣告
	（STJ 源生成器不支援分散）；`[JsonSourceGenerationOptions]` 與方法實現分開放比較清楚。
	""")]
	static str BuildCode(IReadOnlyList<INamedTypeSymbol> AppTypes, IReadOnlyList<INamedTypeSymbol> RawTypes) {
		var JsonLines = string.Join('\n', AppTypes.Select(T => JsonSerializablePair(FullName(T))));
		var ExtraLines = string.Join('\n', ExtraJsonTypes.Select(T => $"[JsonSerializable(typeof({T}))]"));
		var ConverterLines = string.Join('\n', RawTypes.Select(
			T => $"\t\t\tnew global::Tsinswreng.CsJson.JsonConvtr<{FullName(T)}>(),"
		));

		return $$"""
		// <auto-generated/>
		// 由 TswgCsTpl.Scripts 的 GenAppJsonCtx 生成；勿手動編輯。
		// 改了 DTO／實體／強類型 Id 之後，跑一次：
		//   dotnet run --project proj/TswgCsTpl.Scripts -- GenAppJsonCtx
		namespace {{Ns}};

		using System.Text.Json.Serialization;

		// ===== AppJsonCtx：JsonSerializable 註冊（必須集中於此宣告）=====
		{{JsonLines}}
		{{ExtraLines}}
		public partial class AppJsonCtx { }

		// ===== AppJsonCtx：生成期選項 =====
		// *必須與 AppJsonCtx.WireOptions 的運行時設定一致*：STJ 有「生成期選項」與「運行時選項」
		// 兩個表面，兩邊不一致時同一個型別走不同路徑會寫出不同結果。改一處就要同改另一處。
		[JsonSourceGenerationOptions(
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			UseStringEnumConverter = true
		)]
		public partial class AppJsonCtx { }

		// ===== AppJsonCtx：強類型值之轉換器清單 =====
		public partial class AppJsonCtx {
			public static partial IList<JsonConverter> MkJsonConverters() {
				return [
		{{ConverterLines}}
				];
			}
		}
		""";
	}

	[Doc("型別的全限定名（含 `global::` 前綴），避免生成檔受 using 影響。")]
	static str FullName(INamedTypeSymbol T) {
		return T.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
	}

	[Doc("一個型別的註冊對：本體 ＋ `IList<T>`（列表響應很常見，缺了就得回頭補）。")]
	static str JsonSerializablePair(str Full) {
		return $"""
		[JsonSerializable(typeof({Full}))]
		[JsonSerializable(typeof(global::System.Collections.Generic.IList<{Full}>))]
		""";
	}
}
