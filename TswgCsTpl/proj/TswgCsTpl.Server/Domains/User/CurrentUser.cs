namespace TswgCsTpl.Server.Domains.User;

using Tsinswreng.CsCore;
using TswgCsTpl.Shared.User.Models.Po;

[Doc("當前請求的登錄身份。由認證中間件填寫、業務服務只讀。作用域＝一次請求（Scoped）。")]
public interface ICurrentUser {
	[Doc("當前用戶 Id；`default`（零值）表示匿名。")]
	public IdUser UserId { get; set; }

	[Doc("當前會話 Id；退出登錄時要撤銷的就是它。零值表示沒有會話。")]
	public IdSession SessionId { get; set; }
}

[Doc($$"""
{nameof(ICurrentUser)} 的默認實現。

*為何是可變的對象而不是構造參數*：認證發生在請求管線（中間件）裡，
業務服務在之後才解析；中間件把結果寫進這個作用域物件，服務再讀，
比「把身份穿過每個函數調用」乾淨得多。
""")]
public class CurrentUser : ICurrentUser {
	public IdUser UserId { get; set; }
	public IdSession SessionId { get; set; }
}
