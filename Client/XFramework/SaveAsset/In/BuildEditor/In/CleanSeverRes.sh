#!/bin/bash
# 用于清空：http://171.217.93.95:25381/gameupdate/petgame_test/

# 配置参数
res_Dir="/www/wwwroot/171.217.93.95_25381/gameupdate/petgame_test"
res_Server_Ip="171.217.93.95"
res_Server_Port="33003"

# 日志输出
echo "[$(date +'%Y-%m-%d %H:%M:%S')] 开始清理远程资源目录"
echo "清理路径: $res_Dir"
echo "服务器: $res_Server_Ip:$res_Server_Port"

# 执行清理操作
ssh game@$res_Server_Ip -p$res_Server_Port \
    "rm -rf $res_Dir/*; \
     mkdir -p $res_Dir; \
     touch $res_Dir/.keep_file"

# 验证结果
if ssh game@$res_Server_Ip -p$res_Server_Port "[ -d $res_Dir ]"; then
    file_count=$(ssh game@$res_Server_Ip -p$res_Server_Port "ls $res_Dir | wc -l")
    echo "清理完成! 目录存在, 文件数量: $file_count"
    echo "剩余文件: $(ssh game@$res_Server_Ip -p$res_Server_Port "ls $res_Dir")"
    exit 0
else
    echo "❌ 错误: 清理后目录不存在"
    exit 1
fi