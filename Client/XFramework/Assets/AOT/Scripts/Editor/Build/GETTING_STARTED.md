# 🚀 开始使用 - 5分钟快速上手

## 您已获得什么？

一个**完整的Unity编辑器打包系统**，自动处理：
- ✅ HybridCLR热更新构建
- ✅ YooAsset资源打包
- ✅ 多平台打包
- ✅ 服务器资源部署
- ✅ 游戏包生成

---

## 📁 文件位置

所有文件都在这里：
```
Assets/AOT/Scripts/Editor/Build/
```

您会看到：
- `BuildPackageWindow.cs` - 主脚本
- `BuildPackageUtility.cs` - 工具脚本
- `README.md` - 项目说明
- `BUILD_GUIDE.md` - 使用指南
- `QUICK_REFERENCE.md` - 快速参考
- 其他文档文件

---

## 🎯 立即使用（3步）

### 步骤1: 打开打包窗口
在Unity菜单中点击：
```
XFramework → Build → Open Build Package Window
```

### 步骤2: 选择配置
- 选择**目标平台**（Android/iOS等）
- 选择**打包模式**（OfflinePlayMode/HostPlayMode等）
- 如果选择了非离线模式，选择**服务器文件夹**

### 步骤3: 点击打包按钮
点击绿色的 **"Start Build Package"** 按钮

脚本会自动执行所有步骤！

---

## ⏸ 重要：AssetBundle打包时的操作

打包进行到 "Step 4" 时会暂停，显示提示：

```
"AssetBundle Builder window has been opened.
Please click the 'ClickBuild' button to build AssetBundles."
```

此时您需要：
1. 在打开的 **AssetBundle Builder** 窗口中
2. 点击 **"ClickBuild"** 按钮
3. 等待构建完成（约5-10分钟）

---

## ✅ 完成后继续

AssetBundle构建完成后，执行菜单：
```
XFramework → Build → Continue Build After AssetBundle
```

脚本会自动完成剩余步骤：
- 资源上传到服务器
- 游戏包构建
- 打开输出文件夹

---

## 📊 3种打包模式说明

### 1. OfflinePlayMode（离线模式）
- **资源**: 内置在游戏包里
- **优点**: 快速，无需服务器
- **缺点**: 包体大，无法热更新
- **用途**: Demo或内置资源版本

### 2. HostPlayMode（主机模式）⭐ 推荐
- **资源**: 从服务器下载
- **优点**: 支持热更新，包体小
- **缺点**: 需要服务器
- **用途**: 线上运营版本

### 3. WebPlayMode（Web模式）
- **资源**: 从Web服务器下载
- **优点**: 跨平台共享
- **缺点**: 依赖网络
- **用途**: Web应用

---

## 📂 输出位置

打包完成后，您会在以下位置找到文件：

```
OutPut/                          # 游戏包输出目录
├── Android/
│   └── 2025-11-18-1430/         # 版本时间戳
│       └── game.apk
├── iOS/
│   └── 2025-11-18-1430/
│       └── (Xcode项目)
└── Windows64/
    └── 2025-11-18-1430/
        └── game.exe

{ServerPath}/                    # 服务器资源
├── Android/                     # （如果使用HostPlayMode）
│   ├── catalog.json
│   ├── package.manifest
│   └── (bundle files)
└── ...其他平台
```

---

## 🔧 如果出现问题

### 错误：菜单找不到
→ 检查 HybridCLR 和 YooAsset 是否已安装

### 错误：配置找不到
→ 确保 AOTGlobalConfig.asset 已在项目中配置

### 无法继续
→ 删除项目根目录的 `.buildstate` 文件，重新开始

更多问题见 `BUILD_GUIDE.md` 中的常见问题部分。

---

## 📚 详细文档

| 文档 | 说明 | 何时阅读 |
|------|------|--------|
| **README.md** | 完整项目说明 | 了解全貌 |
| **BUILD_GUIDE.md** | 详细使用指南 | 首次使用 |
| **QUICK_REFERENCE.md** | 快速速查表 | 日常参考 |
| **IMPLEMENTATION_SUMMARY.md** | 技术实现细节 | 需要扩展时 |

---

## 💡 提示

1. **首次使用建议**
   - 先用 OfflinePlayMode 测试
   - 确保流程能正常运行

2. **加快打包速度**
   - AssetBundle构建是最耗时的
   - 如无必要，不要频繁打包

3. **保持Unity打开**
   - AssetBundle构建中不要关闭编辑器
   - 不要切换场景

4. **监控Console输出**
   - 查看详细的日志信息
   - 帮助快速定位问题

---

## 🎉 就这么简单！

现在您可以：
1. ✅ 打开 `XFramework → Build → Open Build Package Window`
2. ✅ 选择平台和模式
3. ✅ 点击 "Start Build Package"
4. ✅ 在AssetBundle时点击ClickBuild
5. ✅ 继续构建完成

**总耗时**: 15-30分钟，完全自动化！

---

## 📞 需要帮助？

### 快速查找
1. 打开 `QUICK_REFERENCE.md` - 快速参考卡片
2. 查看 Console 输出 - 详细的日志信息
3. 阅读 `BUILD_GUIDE.md` - 完整的使用指南

### 遇到错误
1. 查看控制台错误信息
2. 参考 `BUILD_GUIDE.md` 中的常见问题
3. 检查各个配置文件

---

**准备好了吗？现在就开始吧！** 🚀

在Unity编辑器中点击菜单：
```
XFramework → Build → Open Build Package Window
```

祝您使用愉快！✨
