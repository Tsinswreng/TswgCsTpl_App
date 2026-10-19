// 前端全域 using／別名。與 Ngan.Dict、Magnitree 等前端項目保持同一套習慣，
// 降低跨項目閱讀成本。
//
// 為何放在 Frontend/ 而不是各專案：五個程序集都寫 Avalonia 程式碼，
// 這些別名（GUT／RowDef／VAlign…）出現頻率極高，分散宣告只會各處不一致。

global using Tsinswreng.CsCore;
global using Tsinswreng.CsErr;

global using GUT = Avalonia.Controls.GridUnitType;
global using ColDef = Avalonia.Controls.ColumnDefinition;
global using RowDef = Avalonia.Controls.RowDefinition;
global using TxtAlign = Avalonia.Media.TextAlignment;
global using VAlign = Avalonia.Layout.VerticalAlignment;
global using HAlign = Avalonia.Layout.HorizontalAlignment;
global using BindingMode = Avalonia.Data.BindingMode;
global using IBinding = Avalonia.Data.BindingBase;
global using Style = Avalonia.Styling.Style;

