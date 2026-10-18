namespace TswgCsTpl.Shared.User.Models.Po;

using StronglyTypedIds;
using TswgCsTpl.Shared.Ids;

[Doc("用戶 Id。以 UInt128（ULID）為底，對外是 22 字元的 Low64Base 字符串。")]
[StronglyTypedId(ConstStrongTypeIdTemplate.UInt128)]
public partial struct IdUser {
}

[Doc("密碼記錄 Id。與用戶一對多：同一用戶可以有歷史密碼記錄，改密時追加而不覆蓋。")]
[StronglyTypedId(ConstStrongTypeIdTemplate.UInt128)]
public partial struct IdPassword {
}

[Doc("登錄會話 Id。一個會話＝一次登錄簽發的令牌；退出登錄即撤銷它。")]
[StronglyTypedId(ConstStrongTypeIdTemplate.UInt128)]
public partial struct IdSession {
}
