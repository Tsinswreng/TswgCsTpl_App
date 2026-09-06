# 寫個Bash腳本、把./proj/Cs/Cs.csproj 重命名成 <命令行參數>.csproj
# 然後再把./proj/Cs/ 重命名成 <命令行參數>
# 同理處理 Cs.Test 與 Cs.Scripts

#!/bin/bash
[ $# -ne 1 ] && echo "用法: $0 <新名称>" && exit 1
mv ./proj/Cs/Cs.csproj ./proj/Cs/$1.csproj
mv ./proj/Cs ./proj/$1
mv ./proj/Cs.Test/Cs.Test.csproj ./proj/Cs.Test/$1.Test.csproj
mv ./proj/Cs.Test ./proj/$1.Test
mv ./proj/Cs.Scripts/Cs.Scripts.csproj ./proj/Cs.Scripts/$1.Scripts.csproj
mv ./proj/Cs.Scripts ./proj/$1.Scripts
echo "✅ 重命名完成"