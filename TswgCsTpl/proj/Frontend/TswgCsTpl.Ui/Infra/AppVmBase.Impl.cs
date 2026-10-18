namespace TswgCsTpl.Ui.Infra;

public partial class AppVmBase{

	public partial void Init(){
		IsInited = true;
	}

	public partial void CheckInit(){
		if(!IsInited){
			throw new InvalidOperationException(
				$"{GetType().Name} 尚未初始化：請用依賴注入建立的實例，或先呼叫 Init()。");
		}
	}

	protected partial void Fire(Task<nil> Op){
		// 丟棄回傳值是安全的：RunFire 內部已把例外接住並轉給 HandleErr，
		// 故不會有「未觀察的例外」。這裡若改成 async void 反而會讓例外逃出去。
		_ = RunFire(Op);
	}

	private partial async Task<nil> RunFire(Task<nil> Op){
		try{
			await Op;
		}catch(Exception Ex){
			HandleErr(Ex);
		}
		return NIL;
	}

	// 默認出口：寫到標準錯誤。子類覆寫以顯示在自己的狀態列或提示列上。
	protected virtual partial void HandleErr(Exception Ex){
		Console.Error.WriteLine(Ex);
	}
}
