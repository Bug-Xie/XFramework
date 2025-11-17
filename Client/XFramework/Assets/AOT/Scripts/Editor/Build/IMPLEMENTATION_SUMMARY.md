# 打包编辑器脚本实现总结

## 创建的文件

### 1. BuildPackageWindow.cs
**路径**: `Assets/AOT/Scripts/Editor/Build/BuildPackageWindow.cs`

这是主编辑器脚本，包含以下功能：

#### 菜单入口
- `XFramework/Build/Open Build Package Window` - 打开打包窗口
- `XFramework/Build/Continue Build After AssetBundle` - 继续构建（AssetBundle后）

#### 主要类和方法

**BuildPackageWindow 类 (EditorWindow)**

公开方法：
- `ShowWindow()` - 显示打包窗口
- `ContinueBuildAfterAssetBundle()` - 从菜单继续构建

私有方法：
- `OnGUI()` - 绘制用户界面
- `StartBuildProcess()` - 开始打包流程
- `ContinueBuildProcess(BuildPackageState state)` - 从保存的状态继续

执行步骤（Step系列方法）：
- `ExecuteStep1_SwitchPlatformAndConfig()` - 平台切换和配置修改
- `ExecuteStep2_HybridCLRBuild()` - HybridCLR构建
- `ExecuteStep3_CopyDllFiles()` - DLL文件复制
- `ExecuteStep4_AssetBundleBuild()` - AssetBundle构建
- `ExecuteStep5_CopyToServer()` - 复制到服务器
- `ExecuteStep6_RestoreConfig()` - 还原配置
- `ExecuteStep7_BuildGame()` - 游戏包构建
- `ExecuteStep8_OpenOutputFolder()` - 打开输出文件夹

辅助方法：
- `ValidateInputs()` - 验证用户输入
- `SetTargetPlatform()` - 切换Unity平台
- `DrawPlatformSelection()` - 绘制平台选择UI
- `DrawPlayModeSelection()` - 绘制模式选择UI
- `DisplayCurrentConfig()` - 显示当前配置
- `GetPlatformName()` - 获取平台名称
- `UpdateHostServerURL()` - 更新服务器URL
- `GetEnabledScenes()` - 获取启用的场景
- `GetOutputFilePath()` - 获取输出文件路径
- `CopyDirectory()` - 递归复制目录
- `LoadAOTGlobalConfig()` - 加载配置文件

#### 主要特性

1. **完整的UI界面**
   - 配置文件选择
   - 平台选择按钮（Android, iOS, Windows64, OSX）
   - 打包模式选择（OfflinePlayMode, HostPlayMode, WebPlayMode）
   - 服务器路径输入和浏览
   - 实时配置显示

2. **自动化流程**
   - 自动检测并切换平台
   - 自动修改配置文件
   - 自动执行HybridCLR构建
   - 自动复制DLL文件
   - 自动调用AssetBundle Builder
   - 自动复制资源到服务器
   - 自动还原配置
   - 自动构建游戏包
   - 自动打开输出文件夹

3. **容错机制**
   - 配置修改前保存原始值
   - 出错时自动还原配置
   - 详细的日志记录
   - 用户友好的错误提示

4. **平台支持**
   - Android
   - iOS
   - Windows64
   - macOS

5. **模式支持**
   - OfflinePlayMode（离线模式，资源内置）
   - HostPlayMode（推荐，支持热更新）
   - WebPlayMode（Web服务器模式）

---

### 2. BuildPackageUtility.cs
**路径**: `Assets/AOT/Scripts/Editor/Build/BuildPackageUtility.cs`

辅助工具类，提供状态管理和工具方法。

#### 主要类

**BuildPackageUtility 类**

静态方法：
- `SaveBuildState(BuildPackageState state)` - 保存构建状态到文件
- `TryLoadBuildState(out BuildPackageState state)` - 从文件加载构建状态
- `ClearBuildState()` - 清除保存的构建状态
- `WaitForAssetBundleBuild(int timeoutSeconds)` - 等待AssetBundle构建完成

**BuildPackageState 类 (Serializable)**

用于保存构建过程中的重要信息：
- `PlayModeValue` - 所选的打包模式
- `HostServerURL` - 服务器URL
- `ServerFolderPath` - 服务器文件夹路径
- `PlatformName` - 目标平台名称

#### 主要特性

1. **状态持久化**
   - 在AssetBundle构建阶段保存状态
   - 允许用户随后从中断处继续
   - 状态文件位置：项目根目录 `.buildstate`

2. **工具方法**
   - 等待异步操作完成
   - 状态序列化和反序列化

---

### 3. BUILD_GUIDE.md
**路径**: `Assets/AOT/Scripts/Editor/Build/BUILD_GUIDE.md`

完整的使用指南文档，包含：

- 功能概述
- 文件结构
- 菜单入口
- 详细的使用流程（8个步骤）
- 配置说明
- 打包模式说明
- 常见问题解答
- 调试日志说明
- 技术细节
- 最佳实践

---

## 核心流程说明

### 打包流程的8个步骤

```
Step 1: 平台切换与配置修改
  ├─ 切换Unity编辑器平台（如需要）
  ├─ 保存原始配置
  ├─ 更新PlayMode
  └─ 更新HostServerURL

Step 2: HybridCLR构建
  └─ 执行 HybridCLR/Generate/All 菜单

Step 3: 复制DLL文件
  ├─ 清空 Assets/Jit/PakageAsset/ScriptDLL
  ├─ 复制AOT DLL文件（添加.bytes后缀）
  └─ 复制热更新DLL文件（添加.bytes后缀）

Step 4: AssetBundle构建（**需要手动操作**)
  ├─ 打开YooAsset AssetBundle Builder窗口
  ├─ 保存构建状态
  └─ 等待用户点击ClickBuild

[用户在AssetBundle Builder中点击ClickBuild并等待完成]

Step 5: 复制资源到服务器（从保存的状态恢复）
  ├─ 查找Bundles中最新的版本文件夹
  ├─ 清空服务器对应平台的文件夹
  └─ 复制AssetBundle文件到服务器

Step 6: 还原配置
  ├─ 恢复PlayMode到原始值
  └─ 恢复HostServerURL到原始值

Step 7: 构建游戏包
  ├─ 准备输出目录（OutPut/{平台}/{版本时间}）
  └─ 执行BuildPipeline.BuildPlayer()

Step 8: 打开输出文件夹
  └─ 使用系统文件浏览器打开输出目录
```

### 配置修改逻辑

**PlayMode 修改**:
- 读取当前值：`aotGlobalConfig.aotGlobalYooAssetConfig.playMode`
- 修改为选择值
- 保存修改

**HostServerURL 修改**:
- 找到URL中最后一个 `/` 的位置
- 替换其后的部分为选择的平台名
- 例如：`http://192.168.1.167:8084/XFramework/Android` → `http://192.168.1.167:8084/XFramework/iOS`

### DLL文件复制逻辑

**AOT DLL**:
- 源: `HybridCLRData/AssembliesPostIl2CppStrip/{平台名}/`
- 目标: `Assets/Jit/PakageAsset/ScriptDLL/{DLL名}.dll.bytes`
- 根据 `AOTScriptDllNames` 列表复制

**热更新DLL**:
- 源: `HybridCLRData/HotUpdateDlls/{平台名}/`
- 目标: `Assets/Jit/PakageAsset/ScriptDLL/{hotScriptDllName}.dll.bytes`

### 资源服务器部署逻辑

1. 查找最新的AssetBundle版本
   - 路径: `Bundles/{平台名}/{包名}/`
   - 按文件夹修改时间排序，获取最新的

2. 部署到服务器
   - 创建 `{服务器路径}/{平台名}/` 目录
   - 清空该目录（删除所有文件和子文件夹）
   - 递归复制最新版本的所有文件

---

## 代码特点

### 1. 健壮的错误处理
- Try-catch块保护关键操作
- 出错时自动还原配置
- 详细的错误日志记录
- 用户友好的错误对话框

### 2. 灵活的配置管理
- 自动查找AOTGlobalConfig.asset
- 配置修改前后的完整记录
- 支持配置的自动还原

### 3. 自动化与人工控制的平衡
- 前4个步骤完全自动化
- 第4步在AssetBundle构建处暂停（需要用户操作）
- 后4个步骤在恢复后自动执行

### 4. 跨平台兼容性
- 支持Windows、macOS等编辑器运行
- 使用 Path.Combine 处理路径（兼容不同系统的路径分隔符）
- EditorUtility.RevealInFinder 在各平台自动调用正确的文件浏览器

### 5. 详细的日志记录
```
========== Build Process Started ==========
Step 1: Switching platform and updating configuration...
  Updated PlayMode from HostPlayMode to OfflinePlayMode
  Updated HostServerURL to ...
  Configuration saved
Step 2: Executing HybridCLR Generate ALL...
  HybridCLR Generate ALL executed
...
========== Build Process Completed Successfully ==========
```

---

## 使用场景

### 场景1：本地Offline打包
1. 打开打包窗口
2. 选择平台（如Android）
3. 选择 OfflinePlayMode
4. 点击 "Start Build Package"
5. 构建完成，游戏包包含所有资源

### 场景2：线上HostPlayMode打包（推荐用于热更新）
1. 打开打包窗口
2. 选择平台（如Android）
3. 选择 HostPlayMode
4. 选择服务器资源文件夹
5. 点击 "Start Build Package"
6. 等待提示，在AssetBundle Builder中点击ClickBuild
7. AssetBundle构建完成后，使用菜单继续
8. 资源自动部署到服务器，游戏包生成

### 场景3：快速迭代开发
- 修改了hot-update代码或资源
- 执行打包脚本重新生成AssetBundle
- 资源自动更新到服务器
- 游戏运行时自动下载最新资源

---

## 环境要求

### Unity版本
- 2020.3 LTS 或更高版本（建议）

### 必需的包
- HybridCLR（用于热更新）
- YooAsset（用于资源管理）

### 项目配置
- AOTGlobalConfig.asset 已配置
- AssetBundleCollectorSetting.asset 已配置
- Build Settings 中至少有一个场景配置

---

## 扩展建议

1. **添加自动化命令行打包**
   - 添加命令行参数支持
   - 支持CI/CD集成

2. **增强的资源管理**
   - 自动版本管理
   - 资源差异更新

3. **更多平台支持**
   - WebGL
   - Switch/PS等主机平台

4. **性能优化**
   - 增量更新支持
   - 并行构建

---

## 注意事项

1. **AssetBundle构建时不要关闭Unity**
   - 构建可能需要几分钟
   - 关闭Unity会导致构建失败

2. **服务器路径确保可写入权限**
   - 脚本需要在服务器路径创建目录
   - 需要删除旧文件的权限

3. **首次使用建议备份**
   - 备份AOTGlobalConfig.asset
   - 备份原始服务器文件

4. **定期清理**
   - 删除旧的Bundles文件夹内容
   - 删除旧的OutPut文件夹内容
   - 节省磁盘空间

---

## 许可和支持

这些脚本是为XFramework项目创建的工具，完全集成了HybridCLR热更新和YooAsset资源管理系统。

如有问题，请检查：
1. 日志输出中的错误信息
2. 相关菜单（HybridCLR, YooAsset）是否可用
3. 配置文件是否完整和正确
