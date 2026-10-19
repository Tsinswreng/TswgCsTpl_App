namespace TswgCsTpl.Ui.Views.Account;

using Avalonia.Threading;
using Tsinswreng.CsCore;
using Tsinswreng.CsErr;
using Tsinswreng.CsI18n;
using TswgCsTpl.Infra.I18n;
using TswgCsTpl.Shared.User.Models.Req;
using TswgCsTpl.Shared.User.Svc;
using TswgCsTpl.Ui.Client;

public partial class VmAccount {
	public partial VmAccount(ISvcUser SvcUser, UserSession Session) {
		this.SvcUser = SvcUser;
		this.Session = Session;
		base.Init();
		RefreshState();
	}

	public static partial VmAccount Mk() {
		return new VmAccount();
	}

	[Doc("界面文案門面；與注入的 II18n 指向同一份。")]
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
			? I18n.Get(KeysI18n.Me) + "：" + (Session?.UniqName ?? "")
			: I18n[KeysI18n.NotLogin];
	}

	[Doc("把界面上的空值校驗集中在一處：登錄／註冊共用同一套文案。")]
	static str MkVacant(II18n I18n, II18nKey FieldKey) {
		return I18n.Get(KeysI18n.CannotBeEmpty, I18n[FieldKey]);
	}

	public partial async Task<nil> LoginAsy(CT Ct) {
		CheckInit();
		// 1: 先做本地校驗，省一次往返。
		if (string.IsNullOrWhiteSpace(Identity)) {
			StatusText = MkVacant(I18n, KeysI18n.Identity);
			return NIL;
		}
		if (string.IsNullOrEmpty(Password)) {
			StatusText = MkVacant(I18n, KeysI18n.Password);
			return NIL;
		}
		// 2: 調服務；成功後清空密碼（不留在記憶體裡的明文）。
		await SvcUser.Login(new ReqLogin { Identity = Identity.Trim(), Password = Password }, Ct);
		Password = "";
		StatusText = I18n[KeysI18n.LoginSucceeded];
		RefreshState();
		return NIL;
	}

	public partial async Task<nil> RegisterAsy(CT Ct) {
		CheckInit();
		// 1: 本地校驗：非空、兩次密碼一致、郵箱形狀。
		if (string.IsNullOrWhiteSpace(RegUniqName)) {
			StatusText = MkVacant(I18n, KeysI18n.UniqName);
			return NIL;
		}
		if (string.IsNullOrWhiteSpace(RegEmail)) {
			StatusText = MkVacant(I18n, KeysI18n.Email);
			return NIL;
		}
		if (string.IsNullOrEmpty(RegPassword)) {
			StatusText = MkVacant(I18n, KeysI18n.Password);
			return NIL;
		}
		if (RegPassword != RegConfirmPassword) {
			StatusText = I18n[KeysI18n.PasswordMismatch];
			return NIL;
		}
		// 2: 調服務。註冊成功不自動登錄：讓用戶明確走一次登錄。
		await SvcUser.Register(new ReqRegister {
			UniqName = RegUniqName.Trim(),
			Email = RegEmail.Trim(),
			Password = RegPassword,
		}, Ct);
		RegPassword = "";
		RegConfirmPassword = "";
		StatusText = I18n[KeysI18n.RegisterSucceeded];
		return NIL;
	}

	public partial async Task<nil> LogoutAsy(CT Ct) {
		CheckInit();
		await SvcUser.Logout(Ct);
		StatusText = I18n[KeysI18n.LogoutSucceeded];
		RefreshState();
		return NIL;
	}

	protected override partial void HandleErr(Exception Ex) {
		// 1: 業務異常（鍵＋參數）→ 用當前語言翻成一句話。
		var Text = Ex switch {
			TypedErr Err => I18n.Get(Err.Type, Err.Args?.ToArray() ?? []),
			_ => I18n[KeysI18n.PleaseRetryLater],
		};
		// 2: 一律回 UI 線程再改綁定屬性（非同步工作可能落在背景線程）。
		Dispatcher.UIThread.Post(() => {
			StatusText = Text;
		});
	}
}

