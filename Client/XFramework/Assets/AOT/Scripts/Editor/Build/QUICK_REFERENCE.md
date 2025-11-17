# XFramework Build Package - 快速参考卡片

## 菜单快速访问

### 打开打包窗口
```
XFramework/Build/Open Build Package Window
```

### 继续构建（AssetBundle后）
```
XFramework/Build/Continue Build After AssetBundle
```

---

## 完整的打包流程（4步操作）

### 1️⃣ 打开窗口
点击菜单 `XFramework/Build/Open Build Package Window`

### 2️⃣ 配置并开始
- 选择 **平台** (Android/iOS/Windows64/OSX)
- 选择 **打包模式** (OfflinePlayMode/HostPlayMode/WebPlayMode)
- 选择 **服务器路径** (仅非Offline模式需要)
- 点击 **"Start Build Package"** 按钮

### 3️⃣ 等待提示
脚本会自动执行步骤1-4：
- 平台切换和配置修改 ✓
- HybridCLR构建 ✓
- DLL文件复制 ✓
- 打开AssetBundle Builder窗口 ⏸

### 4️⃣ 手动操作AssetBundle
在弹出的AssetBundle Builder窗口中：
- 确认构建设置
- 点击 **"ClickBuild"** 按钮
- 等待构建完成

### 5️⃣ 继续构建
构建完成后，使用菜单继续：
```
XFramework/Build/Continue Build After AssetBundle
```

脚本会自动执行步骤5-8：
- 复制资源到服务器 ✓
- 还原配置 ✓
- 构建游戏包 ✓
- 打开输出文件夹 ✓

---

## 关键文件夹位置

| 用途 | 路径 |
|------|------|
| **配置文件** | Assets/AOT/Settings/AOT/AOTGlobalConfig.asset |
| **DLL存放** | Assets/Jit/PakageAsset/ScriptDLL |
| **HybridCLR输出** | 项目根目录/HybridCLRData |
| **AssetBundle输出** | 项目根目录/Bundles |
| **游戏包输出** | 项目上级目录/OutPut |

---

## 打包模式速查

| 模式 | 资源位置 | 包体大小 | 热更新 | 场景 |
|------|---------|--------|--------|------|
| **OfflinePlayMode** | 内置 | 大 | ❌ | Demo/内置版本 |
| **HostPlayMode** | 服务器 | 小 | ✅ | 线上运营（推荐）|
| **WebPlayMode** | Web | 小 | ✅ | 跨平台/Web |

---

## 配置URL快速查询

### 修改URL规则
原URL: `http://192.168.1.167:8084/XFramework/Android`

根据选择的平台：
- Android → `.../Android`
- iOS → `.../iOS`
- Windows64 → `.../Windows64`
- OSX → `.../OSX`

---

## 常见快速修复

### ❌ HybridCLR菜单不存在
→ 检查HybridCLR包是否已安装导入

### ❌ YooAsset菜单不存在
→ 检查YooAsset包和YooAssetSettings配置

### ❌ AssetBundle构建失败
→ 检查Collector设置和资源完整性

### ❌ 无法继续构建
→ 删除项目根目录的 `.buildstate` 文件，重新开始

---

## 输出示例

构建完成后，文件夹结构：

```
OutPut/
├── Android/
│   └── 2025-11-18-1430/
│       └── game.apk
├── iOS/
│   └── 2025-11-18-1430/
│       └── (iOS Xcode项目)
└── Windows64/
    └── 2025-11-18-1430/
        └── game.exe
```

服务器资源结构：
```
{ServerPath}/
├── Android/
│   ├── catalog.json
│   ├── package.manifest
│   └── (asset bundles)
├── iOS/
│   └── ...
└── Windows64/
    └── ...
```

---

## 日志查看

Unity Console 中的日志格式：

```
========== Build Process Started ==========
Step 1: Switching platform and updating configuration...
  Updated PlayMode from X to Y
  Configuration saved
Step 2: Executing HybridCLR Generate ALL...
  HybridCLR Generate ALL executed
Step 3: Copying DLL files...
  Cleared: Assets/Jit/PakageAsset/ScriptDLL
  Copied AOT DLL: System.dll -> System.dll.bytes
  Copied Hot Update DLL: HotUpdate.dll -> HotUpdate.dll.bytes
  DLL files copied successfully
Step 4: Executing AssetBundle build...
  AssetBundle Builder window opened
========== Build Process Paused at Step 4 ==========
```

---

## 脚本文件位置

| 文件 | 路径 | 作用 |
|------|------|------|
| **BuildPackageWindow.cs** | Assets/AOT/Scripts/Editor/Build/ | 主窗口和流程控制 |
| **BuildPackageUtility.cs** | Assets/AOT/Scripts/Editor/Build/ | 工具和状态管理 |
| **BUILD_GUIDE.md** | Assets/AOT/Scripts/Editor/Build/ | 详细使用指南 |
| **IMPLEMENTATION_SUMMARY.md** | Assets/AOT/Scripts/Editor/Build/ | 实现细节说明 |

---

## 快速命令行启动 (可选)

虽然主要通过UI操作，但也可以通过代码调用：

```csharp
// 打开窗口
BuildPackageWindow.ShowWindow();

// 继续构建
BuildPackageWindow.ContinueBuildAfterAssetBundle();
```

---

## ⚠️ 重要提醒

1. **AssetBundle构建时保持Unity打开**
   - 不要关闭Unity编辑器
   - 不要切换场景或项目

2. **首次使用请备份**
   - 备份AOTGlobalConfig.asset
   - 备份服务器文件夹

3. **检查磁盘空间**
   - AssetBundle和OutPut文件夹占用较大磁盘空间
   - 定期清理旧文件

4. **确保权限正确**
   - 服务器文件夹必须可读写
   - Output文件夹必须可写

---

## 返回帮助

更详细的信息请查看：
- `BUILD_GUIDE.md` - 完整使用指南
- `IMPLEMENTATION_SUMMARY.md` - 技术实现细节

在Unity Console中查看详细的日志输出。
