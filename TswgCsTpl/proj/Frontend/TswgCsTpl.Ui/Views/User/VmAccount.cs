namespace TswgCsTpl.Ui.Views.User;

using Tsinswreng.CsCore;
using TswgCsTpl.Shared.User.Svc;
using TswgCsTpl.Ui.Client;
using TswgCsTpl.Ui.Infra;

[Doc($$"""
賬戶面板的 ViewModel：登錄、註冊、退出登錄。

*它只認 {nameof(ISvcUser)}*：跑在本機服務端還是遠程服務端由註冊進容器的實現決定，界面不必知道。
*為何不用繼承來複用*：與 {nameof(AppVmBase)} 的關係只有「屬性通知＋非同步錯誤出口」兩件事，
繼承足夠；其餘一切都是本類自己的狀態。
""")]
public partial class VmAccount : AppVmBase {
	[Doc("共享契約的用戶服務（客戶端實現）。")]
	protected ISvcUser? SvcUser;

	[Doc("前端會話狀態（令牌與當前用戶）。")]
	protected UserSession? Session;

	[Doc("無依賴建構器；僅供設計期與測試。正常應由容器建立。")]
	public partial VmAccount();

	[Doc("由容器建立並注入依賴；建立後由 DI 呼叫 `Init`。")]
	public partial VmAccount(ISvcUser SvcUser, UserSession Session);

	[Doc("標記初始化完成。")]
	public partial void Init();

	[Doc("登錄身份：登錄名或郵箱。")]
	public str Identity { get; set; }

	[Doc("登錄密碼。")]
	public str Password { get; set; }

	[Doc("註冊用登錄名。")]
	public str RegUniqName { get; set; }

	[Doc("註冊用郵箱。")]
	public str RegEmail { get; set; }

	[Doc("註冊用密碼。")]
	public str RegPassword { get; set; }

	[Doc("註冊用確認密碼。")]
	public str RegConfirmPassword { get; set; }

	[Doc("狀態文字：成功提示、校驗失敗提示、當前用戶摘要。")]
	public str StatusText { get; set; }

	[Doc("是否已登錄（決定界面顯示哪一組操作）。")]
	public bool IsLogin { get; set; }

	[Doc("登錄狀態變化事件；由視圖訂閱以刷新可見性。")]
	public event EventHandler? OnStateChanged;

	[Doc("啟動登錄（供按鈕事件調用的同步入口）。")]
	public partial void FireLogin();

	[Doc("啟動註冊。")]
	public partial void FireRegister();

	[Doc("啟動退出登錄。")]
	public partial void FireLogout();

	[Doc("刷新 `IsLogin`／`StatusText` 並通知視圖。")]
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

