namespace TswgCsTpl.Ui.Views.Account;

using Tsinswreng.CsCore;
using TswgCsTpl.Common;
using TswgCsTpl.Shared.User.Svc;
using TswgCsTpl.Ui.Client;
using TswgCsTpl.Ui.Infra;

[Doc($$"""
賬戶面板的 ViewModel：登錄、註冊、退出登錄。

*它只認 {nameof(ISvcUser)}*：跑在本機服務端還是遠程服務端由註冊進容器的實現決定，界面不必知道。
*只曝露狀態與命令*：不做視圖跳轉、不碰控件。
""")]
public partial class VmAccount : AppVmBase, IMk<VmAccount> {
	[Doc("共享契約的用戶服務（客戶端實現）。")]
	ISvcUser SvcUser = default!;

	[Doc("前端會話狀態（令牌與當前用戶）。")]
	UserSession Session = default!;

	[Doc("所有 Vm 都要有 protected 的無參構造器，供 Mk 用。")]
	protected VmAccount() {
	}

	[Doc("供外部直接建立（不注入依賴），單元測試也用它。")]
	public static partial VmAccount Mk();

	[Doc("唯一的 public 構造器，供依賴注入。")]
	public partial VmAccount(ISvcUser SvcUser, UserSession Session);

	[Doc("登錄身份：登錄名或郵箱。")]
	public str Identity {
		get;
		set { SetProperty(ref field, value); }
	} = "";

	[Doc("登錄密碼。")]
	public str Password {
		get;
		set { SetProperty(ref field, value); }
	} = "";

	[Doc("註冊用登錄名。")]
	public str RegUniqName {
		get;
		set { SetProperty(ref field, value); }
	} = "";

	[Doc("註冊用郵箱。")]
	public str RegEmail {
		get;
		set { SetProperty(ref field, value); }
	} = "";

	[Doc("註冊用密碼。")]
	public str RegPassword {
		get;
		set { SetProperty(ref field, value); }
	} = "";

	[Doc("註冊用確認密碼。")]
	public str RegConfirmPassword {
		get;
		set { SetProperty(ref field, value); }
	} = "";

	[Doc("狀態文字：成功提示、校驗失敗提示、當前用戶摘要。")]
	public str StatusText {
		get;
		set { SetProperty(ref field, value); }
	} = "";

	[Doc("是否已登錄。")]
	public bool IsLogin {
		get;
		set { SetProperty(ref field, value); }
	}

	[Doc("啟動登錄（供按鈕事件調用的同步入口）。")]
	public partial void FireLogin();

	[Doc("啟動註冊。")]
	public partial void FireRegister();

	[Doc("啟動退出登錄。")]
	public partial void FireLogout();

	[Doc("刷新 `IsLogin` 與 `StatusText`。")]
	public partial void RefreshState();

	[Doc("登錄實現。")]
	public partial Task<nil> LoginAsy(CT Ct);

	[Doc("註冊實現。")]
	public partial Task<nil> RegisterAsy(CT Ct);

	[Doc("退出登錄實現。")]
	public partial Task<nil> LogoutAsy(CT Ct);

	[Doc("錯誤出口：把鍵翻成目前語言的文案顯示在狀態列。")]
	protected override partial void HandleErr(Exception Ex);
}

