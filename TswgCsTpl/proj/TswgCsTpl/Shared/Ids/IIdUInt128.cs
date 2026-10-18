namespace TswgCsTpl.Shared.Ids;

using Tsinswreng.CsSql;
using Tsinswreng.CsU128Id;

[Doc($$"""
強類型 Id 的共同契約：包一個底層標量值。

*為何要有這層*：所有 Id 都是 UInt128，若直接用裸 UInt128 到處傳，
「用戶 Id」與「密碼記錄 Id」互換了編譯器也不會吭聲；包成不同結構後，這種錯在編譯期就被擋住。

#See[{{nameof(IIdUInt128)}}][以 UInt128 為底的具體契約與工具。]
""")]
public interface IStrongId<T>
	where T : struct
{
	[Doc("底層標量值。")]
	public T Value { get; set; }
}

[Doc($$"""
以 UInt128 為底的強類型 Id 契約。

具體結構由 `StronglyTypedId` 源生成器照 `UInt128Template.typedid` 產生，
故每個 Id 型別都自動具備：無參構造器（= 新 Id）、`Value`、與 UInt128 的隱式互轉、`ToString` 走 Low64Base。

#Eg[
	```cs
	[StronglyTypedId(ConstStrongTypeIdTemplate.UInt128)]
	public partial struct IdUser {}

	var Id = new IdUser();
	var S = Id.ToString();          // 22 字元、URL 安全
	var Back = IdUser.FromLow64Base(S);
	```
]
""")]
public interface IIdUInt128 : IStrongId<UInt128> {
}

[Doc($$"""
{{nameof(IIdUInt128)}} 的共通工具：字符串／位元組／數據庫映射。

*為何集中在此*：所有強類型 Id 的轉換規則必須一致，散在各實體裡寫會漂移；
尤其數據庫那一側——列型別是 `bytea`，映射函數一旦不一致，查詢就會靜默查不到資料。
""")]
public static class ExtnIIdUInt128 {
	extension<T>(T z)
		where T : struct, IIdUInt128
	{
		[Doc("零值 Id。用於「沒有登錄用戶」「不存在的 Id」等哨兵語義。")]
		public static T Zero => default;

		[Doc("轉成 16 位元組（大端序），即數據庫裡 `bytea` 列的值。")]
		public u8[] ToByteArr() {
			return ToolU128Id.ToByteArr(z.Value);
		}

		[Doc($$"""
		從 16 位元組還原 Id。
		#Prm[{{nameof(Bytes)}}][{nameof(ToByteArr)} 產生的位元組。]
		#Rtn[還原出的 Id。]
		""")]
		public static T FromByteArr(u8[] Bytes) {
			return new T {
				Value = ToolU128Id.ByteArrToUInt128(Bytes),
			};
		}

		[Doc($$"""
		數據庫列的型別映射：`bytea` ↔ 強類型 Id。
		每個 Id 型別都要在建表時用一次，故寫成共用擴展而不是各實體各一份。
		#Rtn[供 CsSql 的 `Col(...).MapType(...)` 使用的映射函數。]
		""")]
		public static IUpperTypeMapFnT<u8[], T> MkTypeMapFn() {
			return UpperTypeMapFnT<u8[], T>.Mk(
				(Raw) => FromByteArr<T>(Raw)
				, (Upper) => Upper.Value.ToByteArr()
			);
		}

		[Doc($$"""
		從 Low64Base 字符串還原 Id。
		#Prm[{{nameof(Low64Base)}}][{nameof(ToString)} 產生的字符串。]
		#Throw[{{nameof(FormatException)}}][字符串不是合法的 Low64Base 編碼時。]
		""")]
		public static T FromLow64Base(str Low64Base) {
			return new T {
				Value = ToolU128Id.Low64BaseToUInt128(Low64Base),
			};
		}

		[Doc($$"""
		嘗試解析 Low64Base 字符串；失敗不拋例外，適用於「用戶輸入」這種不可信來源。
		#Prm[{{nameof(S)}}][待解析字符串，可為 null。]
		#Prm[{{nameof(R)}}][成功時的 Id，失敗時為默認值。]
		#Rtn[是否解析成功。]
		""")]
		public static bool TryParse(str? S, out T R) {
			if (S is null) {
				R = default;
				return false;
			}
			if (ToolU128Id.TryLow64BaseToUInt128(S, out var Num)) {
				R = new T { Value = Num };
				return true;
			}
			R = default;
			return false;
		}
	}
}

[Doc("強類型 Id 使用的源生成模板名常量。寫成常量是為了讓 `[StronglyTypedId(...)]` 不硬編碼字符串。")]
public static class ConstStrongTypeIdTemplate {
	[Doc("對應專案根目錄的 `UInt128Template.typedid`。")]
	public const str UInt128 = nameof(UInt128) + "Template";
}
