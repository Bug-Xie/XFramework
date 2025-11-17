# XFramework Build Package 编辑器脚本使用指南

## 功能概述

这个编辑器脚本提供了一个完整的Unity项目打包工作流程，集成了以下功能：
- 自动平台切换
- 配置文件管理
- HybridCLR热更新构建
- DLL文件处理
- AssetBundle资源打包
- 游戏包构建
- 自动文件分发到服务器

## 文件结构

```
Assets/AOT/Scripts/Editor/Build/
├── BuildPackageWindow.cs          # 主编辑器窗口 (UI和流程控制)
└── BuildPackageUtility.cs         # 工具类 (状态管理和辅助功能)
```

## 菜单入口

### 打开打包窗口
菜单路径: `XFramework/Build/Open Build Package Window`

打开图形化打包窗口，用于选择平台、打包模式和服务器路径。

### 继续构建 (AssetBundle后)
菜单路径: `XFramework/Build/Continue Build After AssetBundle`

在AssetBundle构建完成后，使用此菜单继续剩余的构建步骤。

## 使用流程

### 步骤1: 打开打包窗口

1. 在Unity编辑器中点击菜单: `XFramework/Build/Open Build Package Window`
2. 打包窗口会显示以下选项：

#### 配置区域
- **AOT Global Config**: 自动查找或手动指定 AOTGlobalConfig.asset 文件

#### 平台选择
选择目标打包平台：
- Android
- iOS
- Windows64
- OSX

#### 打包模式选择
根据您的需求选择游戏运行模式：
- **OfflinePlayMode**: 本地离线模式，资源完全内置，无需服务器
- **HostPlayMode**: 主机托管模式，资源从服务器下载（推荐用于热更新）
- **WebPlayMode**: Web服务器模式，资源从Web服务器下载

#### 服务器文件夹路径
当选择非Offline模式时，需要指定服务器资源存放路径：
- 点击 "Browse" 按钮浏览选择文件夹
- 或直接在输入框中粘贴路径

### 步骤2: 点击 "Start Build Package" 按钮

脚本会自动执行以下操作：

#### Step 1: 平台切换与配置修改
- 检查当前Unity编辑器平台是否与选择的平台一致
- 如果不一致，自动切换到选定平台
- 保存原始配置信息
- 更新AOTGlobalConfig中的PlayMode
- 更新HostServerURL（将URL末尾的平台名改为所选平台）

#### Step 2: 执行HybridCLR构建
- 执行菜单: `HybridCLR/Generate/All`
- 生成AOT元数据和热更新相关文件
- 输出位置: 项目根目录的 `HybridCLRData` 文件夹

#### Step 3: 复制DLL文件
- 清空 `Assets/Jit/PakageAsset/ScriptDLL` 文件夹中的所有文件
- 从 `HybridCLRData/AssembliesPostIl2CppStrip/{平台名}` 复制AOT DLL文件
- 从 `HybridCLRData/HotUpdateDlls/{平台名}` 复制热更新DLL文件
- 所有复制的DLL文件添加 `.bytes` 后缀

#### Step 4: 打开AssetBundle构建器（**需要手动操作**）
- 自动打开YooAsset的 AssetBundle Builder 窗口
- 显示提示信息，告诉用户需要手动点击 "ClickBuild" 按钮
- 此时需要用户在AssetBundle Builder窗口中：
  1. 确认和修改构建设置（如需要）
  2. 点击 "ClickBuild" 按钮开始构建
  3. 等待构建完成（通常需要几分钟）

> **重要**: AssetBundle构建完成后，**不要关闭Unity**，直接执行下一步

### 步骤3: 继续构建（AssetBundle构建完成后）

当AssetBundle构建完成后，脚本会自动保存构建状态。您有两种方式继续：

#### 方法A: 使用菜单继续
点击菜单: `XFramework/Build/Continue Build After AssetBundle`

#### 方法B: 使用窗口继续 (推荐)
1. 打开打包窗口: `XFramework/Build/Open Build Package Window`
2. 点击 "Start Build Package" 按钮
3. 脚本会自动检测上次的构建状态并从正确的步骤继续

脚本会继续执行：

#### Step 5: 复制资源到服务器
- 仅当选择了非Offline模式时执行
- 在Bundles文件夹中查找最新的版本文件夹（按修改时间）
- 清空服务器对应平台的文件夹
- 将最新生成的AssetBundle文件复制到服务器指定路径

#### Step 6: 还原配置
- 恢复AOTGlobalConfig的PlayMode为原始值
- 恢复HostServerURL为原始值
- 保存配置

#### Step 7: 构建游戏包
- 使用Build Settings中配置的场景列表
- 根据平台生成对应的游戏包：
  - Android: game.apk
  - iOS: Xcode项目目录
  - Windows: game.exe
  - macOS: game.app
- 输出路径: `项目上级路径/OutPut/{平台名}/{版本号}/`
- 版本号格式: `YYYY-MM-DD-HHMM`

#### Step 8: 打开输出文件夹
- 自动打开包含最新游戏包的文件夹

## 配置说明

### AOTGlobalConfig.asset 配置

脚本依赖以下配置项：

**YooAsset配置:**
- `playMode`: 资源加载模式 (EPlayMode枚举)
- `packageName`: 资源包名称（默认: DefaultPackage）
- `hostServerURL`: 服务器资源路径（格式: http://server.com/path/{平台名}）

**HybridCLR配置:**
- `hotScriptDllPath`: 热更新DLL存放路径（默认: Assets/Jit/PakageAsset/ScriptDLL）
- `hotScriptDllName`: 热更新DLL文件名（默认: HotUpdate.dll）
- `AOTScriptDllNames`: AOT程序集名称列表（System.Core.dll, System.dll, mscorlib.dll等）
- `mainScriptName`: 热更新入口脚本名（默认: HotUpdateMain）
- `mainScriptMethod`: 入口方法名（默认: Start）

## 打包模式说明

### OfflinePlayMode (离线模式)
- 所有资源内置到游戏包中
- 优点：无需服务器，启动快速
- 缺点：包体大，无法热更新
- 应用场景：Demo、内置资源版本

### HostPlayMode (推荐)
- 游戏启动时从服务器下载资源
- 优点：支持热更新，包体小
- 缺点：需要服务器支持
- 应用场景：线上运营版本

### WebPlayMode
- 使用Web服务器托管资源
- 优点：跨平台资源共享
- 缺点：依赖网络，加载较慢
- 应用场景：Web和跨平台应用

## 常见问题

### Q: 如果HybridCLR菜单不存在怎么办？
A: 确保HybridCLR包已正确安装。检查Package Manager中是否已导入，必要时重新安装。

### Q: 如果YooAsset菜单不存在怎么办？
A: 确保YooAsset包已正确安装和初始化。检查Assets/Resources目录下是否有YooAssetSettings配置。

### Q: AssetBundle构建失败怎么办？
A: 检查以下内容：
1. AssetBundleCollectorSetting.asset 中的收集规则是否正确
2. ScriptDLL文件夹中的DLL文件是否完整
3. 构建前是否有Shader编译错误或资源缺失

### Q: 服务器路径应该如何设置？
A: 服务器路径应该指向存放资源的目录。脚本会根据选择的平台自动创建对应的子文件夹。
例如：
- 选择Android，路径为 `/server/files`
- 脚本会在 `/server/files/Android` 创建资源

### Q: 如何重新开始打包流程？
A: 如果中途出错，可以手动删除项目根目录下的 `.buildstate` 文件，然后重新开始。

### Q: 打包后需要修改什么配置？
A: 一般情况下，脚本会自动还原配置。但如果选择的是HostPlayMode，需要确保：
1. 游戏的服务器URL指向正确的资源服务器地址
2. 服务器已正确部署资源文件

## 调试日志

打包过程中，所有步骤都会在Unity Console中输出详细的日志信息，格式为：

```
========== Build Process Started ==========
Step 1: Switching platform and updating configuration...
Step 2: Executing HybridCLR Generate ALL...
...
========== Build Process Completed Successfully ==========
```

根据这些日志可以追踪每个步骤的执行情况，快速定位问题。

## 技术细节

### 状态保存机制
脚本在第4步（AssetBundle构建）时保存构建状态到 `.buildstate` 文件：
- 位置: 项目根目录
- 内容: 序列化的BuildPackageState对象
- 用途: 在AssetBundle构建完成后恢复构建过程

### 平台名称映射
脚本内置以下平台名称映射：
- BuildTarget.Android → "Android"
- BuildTarget.iOS → "iOS"
- BuildTarget.StandaloneWindows64 → "Windows64"
- BuildTarget.StandaloneOSX → "OSX"

### URL更新逻辑
HostServerURL更新规则：
- 找到URL中最后一个 `/` 的位置
- 替换其后的内容为所选平台名
- 例如: `http://server.com/game/Android` → `http://server.com/game/iOS`

## 最佳实践

1. **提前检查**: 打包前确保所有资源都已添加到AssetBundle配置
2. **备份配置**: 在首次使用前备份AOTGlobalConfig.asset
3. **测试**: 先用Offline模式测试，确保基础构建流程正常
4. **监控日志**: 密切关注Console日志，及时发现问题
5. **定期清理**: 删除旧的Bundles和OutPut文件夹内容，节省磁盘空间

## 支持

如有问题或建议，请检查：
1. 相关菜单是否存在（HybridCLR, YooAsset）
2. 配置文件是否完整
3. Unity和包的版本是否兼容
