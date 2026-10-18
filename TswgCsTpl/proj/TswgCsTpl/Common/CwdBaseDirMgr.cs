namespace TswgCsTpl.Common;

public class CwdBaseDirMgr:I_GetBaseDir{
	public static CwdBaseDirMgr Inst =>field??= new CwdBaseDirMgr();
	public str _BaseDir{get;set;} = Directory.GetCurrentDirectory();
	public str GetBaseDir(){
		return _BaseDir;
	}
}
