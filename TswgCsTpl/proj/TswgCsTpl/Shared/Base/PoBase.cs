namespace TswgCsTpl.Shared.Base;

using Tsinswreng.CsSql;
using TswgCsTpl.Infra.Json;

[Doc($$"""
所有實體都有、且由數據庫層自己維護的列。

*為何要這個接口*：建表時要靠型別推出「這張表有沒有軟刪列、主鍵叫什麼」，
若每個實體各寫一份、早晚漂移。故把共同列的形狀固定在這裡，建表器只認這個接口。

*時間一律用 i64 毫秒*：不用專門的時間型別，是為了讓映射、比較、序列化都只有一種形態；
時間語義（UTC 毫秒）寫在成員名與本註釋裡。

*為何繼承 {nameof(IAppSerializable)}*：實體不是拿來上網線的，但它們的成員要被 CsSql
按名讀寫（建表、DbDict⇄實體映射），那條路徑走 {nameof(Tsinswreng.CsRefl.ITypeInfoSrc)}；AOT 下反射列舉成員會被裁掉，
故實體也必須進 JSON 源生成上下文。打在這個介面上，三個實體就一次收全，
不必逐個 `Po` 加標記。
""")]
public interface IPoBase : IAppSerializable {
	[Doc("數據庫插入時間（UTC 毫秒）。由 `SqlRepo` 在插入時填，業務不應改。")]
	public i64 DbCreatedAt { get; set; }

	[Doc("數據庫最後更新時間（UTC 毫秒）。由 `SqlRepo` 在更新時填。")]
	public i64 DbUpdatedAt { get; set; }

	[Doc($$"""
	軟刪時間（UTC 毫秒）。`0` 表示未刪，非 `0` 表示已刪。

	*為何用時間而不是布爾*：刪除時刻本身是排錯與審計所需的信息；
	而且「未刪」用 0 表示，索引條件 `DelAt = 0` 可以直接走索引。
	""")]
	public i64 DelAt { get; set; }
}

[Doc($$"""
{nameof(IPoBase)} 的默認實現。實體繼承它即可獲得三個共同列。
#Eg[
	```cs
	public partial class PoUser : PoBase, I_Id<IdUser> {
		public IdUser Id { get; set; } = new();
	}
	```
]
""")]
public class PoBase : IPoBase {
	[Doc("插入時間；實例剛建立時就給當前時間，免得漏填出 0。")]
	public i64 DbCreatedAt { get; set; } = NowMs();

	[Doc("更新時間；若從未更新，值等於插入時間。")]
	public i64 DbUpdatedAt { get; set; } = NowMs();

	[Doc("軟刪時間；0 表示未刪。")]
	public i64 DelAt { get; set; }

	[Doc("取當前 UTC 毫秒。抽成方法，方便日後換成可注入的時鐘。")]
	protected static i64 NowMs() {
		return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
	}
}

[Doc("建表相關的共用配置：主鍵列名、三共同列的映射、軟刪列的 SQL 片段。")]
public static class PoBaseTblCfg {
	[Doc($$"""
	配置 {nameof(IPoBase)} 的三個共同列，並把軟刪語義接到 CsSql 上。

	*為何軟刪要在這裡接*：CsSql 的 `WhereNonDel()`／`SqlIsNonDel()` 全靠表上的
	`SoftDelCol`；不配它的話查詢會把已刪的行一起返回，這是最容易漏、也最難察覺的錯。
	#Prm[{{nameof(Setter)}}][目標表的配置器。]
	#Rtn[同一個配置器，便於串接。]
	""")]
	public static ITblSetter<T> CfgPoBase<T>(ITblSetter<T> Setter)
		where T : IPoBase, new()
	{
		var Tbl = Setter.Tbl;
		// 1: 主鍵在代碼裡一律叫 Id；數據庫列名也用它。
		Tbl.CodeIdName = nameof(I_Id<nil>.Id);
		Tbl.Col(nameof(I_Id<nil>.Id)).AdditionalSqls(["PRIMARY KEY"]);

		// 2: 三共同列都是 i64，不需要額外型別映射。
		Tbl.Col(nameof(IPoBase.DbCreatedAt));
		Tbl.Col(nameof(IPoBase.DbUpdatedAt));
		Tbl.Col(nameof(IPoBase.DelAt));

		// 3: 接上軟刪：刪除＝把 DelAt 寫成當前時刻，恢復＝寫回 0。
		Tbl.SoftDelCol = new SoftDelol {
			CodeColName = nameof(IPoBase.DelAt)
			, FnDelete = (_) => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
			, FnRestore = (_) => 0L
			, FnSqlIsDel = () => Tbl.QtCol<IPoBase>(x => x.DelAt) + " <> 0"
			, FnSqlIsNonDel = () => Tbl.QtCol<IPoBase>(x => x.DelAt) + " = 0"
		};
		return Setter;
	}
}
