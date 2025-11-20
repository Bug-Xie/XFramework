# 因为 YooAsset打包后是形如 1.0.0 格式的文件夹
# 所以该脚本会把当前目录下的 时间最新的形如 "1.0.0" 的文件夹中的内容同步到资源服务器上
# 如果同时存在 1.0.0 1.0.1 两个文件夹，那么会对比两个文件夹差异，把增量资源更新到资源服务器

# 游戏资源路径（不同的更新地址只需要改这个）
res_Dir=/www/wwwroot/171.217.93.95_25381/gameupdate/petgame_test
# 对应的更新url http://171.217.93.95:25381/gameupdate/petgame_test

#res_Dir=/www/wwwroot/171.217.93.95_25381/gameupdate/petgame_test2
# 对应的更新url http://171.217.93.95:25381/gameupdate/petgame_test2

#res_Dir=/www/wwwroot/171.217.93.95_25381/gameupdate/petgame_banhao
# 对应的更新url http://171.217.93.95:25381/gameupdate/petgame_banhao

# 资源服务器ip
res_Server_Ip=171.217.93.95

# 资源服务器端口
res_Server_Port=33003


# ===== 新增部分：切换到目标目录 =====
# 脚本所在的基础目录 (F:\jenkins_petproject\UnityProject\Assets)
base_dir="$(cd "$(dirname "$0")" && pwd)"
# YooAsset打包的输出目录
target_dir="$base_dir/Library/YooAssetBuild/DefaultPackage/Android/DefaultPackage"

echo "脚本位置：$base_dir"
echo "目标目录：$target_dir"

# 检查目标目录是否存在
if [ ! -d "$target_dir" ]; then
    echo "错误：目标目录不存在: $target_dir"
    exit 1
fi

# 切换到目标目录执行操作
cd "$target_dir" || { echo "无法进入目录: $target_dir"; exit 1; }
echo "当前工作目录: $(pwd)"

# 函数：精确匹配数字版本号文件夹
find_dirs() {
    find . -maxdepth 1 -type d -regextype egrep -regex '\./[0-9]+\.[0-9]+\.[0-9]+' | 
    sed 's|^\./||' | 
    sort -V
}

echo "===== 扫描版本文件夹 ====="
dir_list=$(find_dirs)

# 输出文件夹列表用于调试
echo "检测到的版本文件夹:"
[ -n "$dir_list" ] && echo "$dir_list" || echo "<无>"
echo "=========================="

if [[ -z "$dir_list" ]]; then
    echo "错误：未找到有效的版本文件夹，退出"
    exit 1
fi

# 获取版本号
curDir=$(echo "$dir_list" | tail -n1)
lastDir=$(echo "$dir_list" | tail -n2 | head -n1)

# 避免重复比较
[[ "$curDir" == "$lastDir" ]] && lastDir=""

echo "当前版本: $curDir"
echo "上一版本: ${lastDir:-无}"

if [ -z "$curDir" ]; then
	echo "不存资源文件夹，失败"
else
	echo "等待上传"
	
	if [ -z "$lastDir" ]; then
		echo "之前没有打包过资源，全量更新1"
		
		ssh game@$res_Server_Ip -p$res_Server_Port "rm -rf $res_Dir"
		ssh game@$res_Server_Ip -p$res_Server_Port "mkdir $res_Dir"
		
		cd $curDir
		rm update.tgz
		tar -zcf update.tgz *
		cp -f update.tgz ../
		rm update.tgz
		cd ..
		
	elif [ $curDir == $lastDir ]; then
		echo "之前没有打包过资源，全量更新2"
		
		ssh game@$res_Server_Ip -p$res_Server_Port "rm -rf $res_Dir"
		ssh game@$res_Server_Ip -p$res_Server_Port "mkdir $res_Dir"
		
		cd $curDir
		rm update.tgz
		tar -zcf update.tgz *
		cp -f update.tgz ../
		rm update.tgz
		cd ..
		
	else
		echo "开始对比上次打包，和本次打包的增量更新"
	
		rm -rf ./update
		mkdir update
		
		diff -q $curDir $lastDir | grep $curDir | grep -v differ | awk '{for(i=4; i<NF; i++) printf "%s ", $i; printf "%s", $NF; print""}' | xargs -I {} cp $curDir/{} ./update/
		diff -q $curDir $lastDir | grep $curDir | grep differ | awk '{print $2}' | xargs -I {} cp {} ./update/
		
		cd update
		tar -zcf update.tgz *
		cp -f update.tgz ../
		cd ../
		rm -rf ./update
		
		echo "挑选文件完成"
	fi
	
	echo "开始上传"
	
	scp -P$res_Server_Port update.tgz game@$res_Server_Ip:$res_Dir
	ssh game@$res_Server_Ip -p$res_Server_Port "cd $res_Dir ; tar -zxf update.tgz"

	echo "上传完成"
fi

rm -f update.tgz

sleep 1
exit