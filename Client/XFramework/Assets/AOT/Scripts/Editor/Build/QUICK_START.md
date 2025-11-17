# 🎯 打包脚本 - 快速参考卡

## 打开窗口
```
菜单: XFramework/Build/Open Build Package Window
```

---

## UI 配置区域

### 1️⃣ Configuration
```
AOT Global Config: [选择文件/自动查找]
```

### 2️⃣ Build Settings
```
Current Platform: [当前编辑器平台]
Target Platform: [Android ▼]      ← 下拉框选择
```

### 3️⃣ Play Mode
```
[OfflinePlayMode ▼]    ← 下拉框选择
[HostPlayMode ▼]
[WebPlayMode ▼]
```

### 4️⃣ Server Settings
```
Server Folder Path: [输入框] [Browse]  ← 仅非Offline模式需要
```

### 5️⃣ Build Options (两个按钮)
```
┌──────────────────────────────────┐
│ 🟢 Build Complete Package        │  ← 所有步骤，生成游戏包
│     (All Steps)                  │
└──────────────────────────────────┘

┌──────────────────────────────────┐
│ 🔵 Build Hot Update Only         │  ← 只到Step 6，只更新资源
│     (Step 1-6)                   │
└──────────────────────────────────┘
```

---

## 两种打包方式对比

### 🟢 完整打包 (Complete Package)

| 项目 | 内容 |
|------|------|
| **执行步骤** | 1-8 全部 |
| **最后一步** | 打开游戏包文件夹 |
| **输出** | 游戏包 (APK/EXE) |
| **耗时** | ~15-30分钟 |
| **用途** | 发布正式版本 |

**流程：**
```
配置 → HybridCLR → DLL复制 → AssetBundle →
服务器资源 → 配置还原 → 游戏包 → 打开文件夹
```

---

### 🔵 热更包 (Hot Update)

| 项目 | 内容 |
|------|------|
| **执行步骤** | 1-6 |
| **最后一步** | 资源上传到服务器 |
| **输出** | 无游戏包 |
| **耗时** | ~10-20分钟 |
| **用途** | 快速更新资源 |

**流程：**
```
配置 → HybridCLR → DLL复制 → AssetBundle →
服务器资源 → 配置还原
```

---

## 打包过程中需要做什么？

### 第1次点击按钮后：

```
1️⃣ 脚本执行 Step 1-3 (自动，很快)
        ↓
2️⃣ AssetBundle Builder 窗口打开
        ↓
3️⃣ 出现对话框："Please click ClickBuild"
        ↓
4️⃣ 点击对话框的 OK
        ↓
5️⃣ 立即前往 AssetBundle Builder 窗口
        ↓
6️⃣ 在Builder中点击 "ClickBuild" 按钮
        ↓
7️⃣ 等待构建完成（5-10分钟）
        ↓
8️⃣ 脚本自动继续 Step 5-8
        ↓
✅ 完成！（对话框或文件夹打开）
```

---

## ⚠️ 常见问题

### Q: AssetBundle构建很慢怎么办？
A: 正常的。取决于资源量大小，通常5-10分钟。

### Q: 脚本似乎卡住了？
A: 检查AssetBundle Builder窗口是否还在运行构建。

### Q: 热更包有什么优势？
A:
- 不生成大游戏包
- 资源直接上传服务器
- 用户自动下载最新资源
- 快速迭代开发

### Q: 何时用完整打包，何时用热更？
A:
- **完整打包**: 有新版本、主程序更新、首次发布
- **热更包**: Bug修复、资源更新、配置调整

---

## 📊 执行时间预估

| 步骤 | 耗时 |
|------|------|
| Step 1-3 | 几秒-1分钟 |
| Step 4 (AssetBundle) | 5-10分钟 |
| Step 5-6 | 1-2分钟 |
| Step 7 (游戏包) | 5-10分钟 |
| **总耗时** | **~15-30分钟** |

---

## 🎮 实际使用示例

### 场景：发布Android正式版本

```
1. 打开窗口
2. 配置：
   - Platform: Android
   - Mode: HostPlayMode
   - Server: /mnt/game/server
3. 点击: Build Complete Package
4. 在Builder中点击ClickBuild
5. 等待完成
6. ✅ 得到 game.apk，自动打开输出文件夹
```

### 场景：修复了一个bug，更新热更资源

```
1. 打开窗口
2. 配置：
   - Platform: Android
   - Mode: HostPlayMode
   - Server: /mnt/game/server
3. 点击: Build Hot Update Only
4. 在Builder中点击ClickBuild
5. 等待完成
6. ✅ 资源已上传服务器，用户下载更新
```

---

## 💾 存储位置

| 内容 | 路径 |
|------|------|
| **游戏包** | OutPut/{平台}/{版本时间}/ |
| **AssetBundle** | Bundles/{平台}/{包名}/ |
| **服务器资源** | {指定的服务器路径}/{平台}/ |
| **配置文件** | Assets/AOT/Settings/AOT/AOTGlobalConfig.asset |

---

## ✨ 总结

**新的打包流程已经非常简化：**

1. ✅ 选择配置（下拉框，不再是按钮）
2. ✅ 点击按钮（两选一）
3. ✅ 所有步骤自动执行
4. ✅ 中间只需操作一次AssetBundle Builder

**就是这么简单！** 🎉
