使用此模板

刪除原.git目錄

複製整個Cs/ 目錄
```sh
sh RenameProj.sh <NewProjectName>
git init
```

維護腳本

proj/Cs.Scripts 是模板附帶的維護腳本項目：

- dispatcher：`Program.cs` 依首個引數分派到各命令
- 上下文：dispatcher 統一建立 `ISCtx`（`RootDir`、`Tfm`）傳給子命令
- 命令直接跑 `Cs.Test` 專案，不依賴 RenameProj 改名後的路徑

已內建命令

- `Test`：以 `dotnet run` 直接運行 Cs.Test（非 AOT 的快速驗證）
- `TestAotWin`：以 win-x64 NativeAOT 發布 Cs.Test 後運行發布物

用法（在模板根目錄）：
```sh
dotnet run --project proj/Cs.Scripts/Cs.Scripts.csproj -- Test
dotnet run --project proj/Cs.Scripts/Cs.Scripts.csproj -- TestAotWin
```

在 Scripts 專案內新增命令時：

- 新增 `Xxx.cs`（partial class + `partial Task Main(ISCtx Ctx, CT Ct)` 聲明）與 `Xxx.Impl.cs`（實現）
- 在 `Program.cs` 的 switch 與 PrintUsage 中加入分派