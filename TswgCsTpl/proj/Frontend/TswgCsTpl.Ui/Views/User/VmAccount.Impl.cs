namespace TswgCsTpl.Ui.Views.User;

using Avalonia.Threading;
using Tsinswreng.CsCore;
using Tsinswreng.CsErr;
using Tsinswreng.CsI18n;
using TswgCsTpl.Infra.I18n;
using TswgCsTpl.Shared.User.Models.Req;
using TswgCsTpl.Shared.User.Svc;
using TswgCsTpl.Ui.Client;

public partial class VmAccount {
	public partial VmAccount() {
		Identity = "";
		Password = "";
		RegUniqName = "";
		RegEmail = "";
		RegPassword = "";
		RegConfirmPassword = "";
		StatusText = "";
	}

	public partial VmAccount(ISvcUser SvcUser, UserSession Session) : this() {
		this.SvcUser = SvcUser;
		this.Session = Session;
		Init();
	}

	public partial void Init() {
		IsInited = true;
		RefreshState();
	}

	II18n I18n => AppI18n.Inst;



	public partial void FireLogin() {
		Fire(LoginAsy(default));
	}

	public partial void FireRegister() {
		Fire(RegisterAsy(default));
	}

	public partial void FireLogout() {
		Fire(LogoutAsy(default));
	}

	public partial void RefreshState() {
		// 1: 未登錄時給出明確提示，避免界面看起來「什麼都沒發生」。
		IsLogin = Session?.IsLogin == true;
		StatusText = IsLogin
			? I18n.Get(KeysI18n.Ui.Me) + "：" + (Session?.UniqName ?? "")
			: I18n[KeysI18n.Ui.NotLogin];
		OnPropertyChanged(nameof(StatusText));
		OnPropertyChanged(nameof(IsLogin));
		// 2: 通知視圖刷新可見性（登錄表單 vs 已登錄面板）。
		OnStateChanged?.Invoke(this, EventArgs.Empty);
	}

	[Doc("把界面上的空值校驗集中在一處：兩處（登錄／註冊）共用同一套文案。")]
	static str MkVacant(II18n I18n, II18nKey FieldKey) {
		return I18n.Get(KeysI18n.Ui.CannotBeEmpty, I18n[FieldKey]);
	}

	public partial async Task<nil> LoginAsy(CT Ct) {
		CheckInit();
		if (SvcUser is null) {
			return NIL;
		}
		// 1: 先做本地校驗，省一次往返。
		if (string.IsNullOrWhiteSpace(Identity)) {
			StatusText = MkVacant(I18n, KeysI18n.Ui.Identity);
			OnPropertyChanged(nameof(StatusText));
			return NIL;
		}
		if (string.IsNullOrEmpty(Password)) {
			StatusText = MkVacant(I18n, KeysI18n.Ui.Password);
			OnPropertyChanged(nameof(StatusText));
			return NIL;
		}
		// 2: 調服務；成功後清空密碼（不留在記憶體裡的明文）。
		await SvcUser.Login(new ReqLogin { Identity = Identity.Trim(), Password = Password }, Ct);
		Password = "";
		OnPropertyChanged(nameof(Password));
		StatusText = I18n[KeysI18n.Ui.LoginSucceeded];
		RefreshState();
		return NIL;
	}

	public partial async Task<nil> RegisterAsy(CT Ct) {
		CheckInit();
		if (SvcUser is null) {
			return NIL;
		}
		// 1: 本地校驗：非空、兩次密碼一致、郵箱形狀。
		if (string.IsNullOrWhiteSpace(RegUniqName)) {
			StatusText = MkVacant(I18n, KeysI18n.Ui.UniqName);
			OnPropertyChanged(nameof(StatusText));
			return NIL;
		}
		if (string.IsNullOrWhiteSpace(RegEmail)) {
			StatusText = MkVacant(I18n, KeysI18n.Ui.Email);
			OnPropertyChanged(nameof(StatusText));
			return NIL;
		}
		if (string.IsNullOrEmpty(RegPassword)) {
			StatusText = MkVacant(I18n, KeysI18n.Ui.Password);
			OnPropertyChanged(nameof(StatusText));
			return NIL;
		}
		if (RegPassword != RegConfirmPassword) {
			StatusText = I18n[KeysI18n.Ui.PasswordMismatch];
			OnPropertyChanged(nameof(StatusText));
			return NIL;
		}
		// 2: 調服務。註冊成功不自動登錄：讓用戶明確走一次登錄，權責清楚。
		await SvcUser.Register(new ReqRegister {
			UniqName = RegUniqName.Trim(),
			Email = RegEmail.Trim(),
			Password = RegPassword,
		}, Ct);
		RegPassword = "";
		RegConfirmPassword = "";
		OnPropertyChanged(nameof(RegPassword));
		OnPropertyChanged(nameof(RegConfirmPassword));
		StatusText = I18n[KeysI18n.Ui.RegisterSucceeded];
		OnPropertyChanged(nameof(StatusText));
		return NIL;
	}

	public partial async Task<nil> LogoutAsy(CT Ct) {
		CheckInit();
		if (SvcUser is null) {
			return NIL;
		}
		await SvcUser.Logout(Ct);
		StatusText = I18n[KeysI18n.Ui.LogoutSucceeded];
		RefreshState();
		return NIL;
	}

	protected override partial void HandleErr(Exception Ex) {
		// 1: 業務異常（鍵＋參數）→ 用當前語言翻成一句話。
		var Text = Ex switch {
			TypedErr Err => I18n.Get(Err.Type.ToI18nKey(), Err.Args?.ToArray() ?? []),
			_ => I18n[KeysI18n.Ui.PleaseRetryLater],
		};
		// 2: 一律回 UI 線程再改綁定屬性（非同步工作可能落在背景線程）。
		Dispatcher.UIThread.Post(() => {
			StatusText = Text;
			OnPropertyChanged(nameof(StatusText));
		});
	}
}


