使用此模板

刪除原.git目錄

複製整個Cs/ 目錄
```sh
sh RenameProj.sh <NewProjectName>
git init
```

維護腳本

proj/Cs.Scripts 是模板附帶的維護腳本項目，與 Spec 中 `*.Scripts/` 的既有模式一致：

- dispatcher：`Program.cs` 依首個引數分派到各命令，命令類位於同目錄
- 上下文：dispatcher 統一建立 `ISCtx`（含 `RootDir`）傳給子命令，子命令不再自行解析路徑
- 定位：`TestProjects.Find` 以 `proj/*.Test/*.csproj` 模式尋找測試專案，
  因此 RenameProj 改名後無需改動腳本代碼

已內建命令

- `Test`：以 `dotnet run` 直接運行測試專案（非 AOT 的快速驗證）
- `TestAot`：以 NativeAOT 發布測試專案（Release + 目標 RID）後運行發布物；
  RID 預設為 win-x64 / linux-x64，可用環境變數 AOT_TEST_RID 覆蓋

用法（在模板根目錄）：
```sh
dotnet run --project proj/Cs.Scripts/Cs.Scripts.csproj -- Test
dotnet run --project proj/Cs.Scripts/Cs.Scripts.csproj -- TestAot
```

在 Scripts 專案內新增命令時：

- 新增 `Xxx.cs`（partial class + `partial Task Main(ISCtx Ctx, CT Ct)` 聲明）與 `Xxx.Impl.cs`（實現）
- 在 `Program.cs` 的 switch 與 PrintUsage 中加入分派