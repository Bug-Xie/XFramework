using System;
using System.Collections;
using UnityEngine;
using YooAsset;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class YooAssetModule : MonoBehaviour
{
    [Header("YooAsset配置")]
    [SerializeField] private string packageName = "DefaultPackage";
    [SerializeField] private string packageURL = "https://192.168.1.167:8084/XFramework";

    // 事件
    public event Action<string> OnStepChange;
    public event Action<int, long> OnFoundUpdateFiles;
    public event Action<int, int, long, long> OnDownloadProgress;
    public event Action<string> OnError;

    private ResourcePackage _package;
    private ResourceDownloaderOperation _downloader;
    private string _packageVersion;

    /// <summary>
    /// 运行时获取播放模式
    /// </summary>
    private EPlayMode GetPlayMode()
    {
#if UNITY_EDITOR
        return  EPlayMode.EditorSimulateMode; // 编辑器下使用配置的模式
#else
    #if RESOURCE_OFFLINE
        return EPlayMode.OfflinePlayMode;
    #else
        return EPlayMode.HostPlayMode;
    #endif
#endif
    }

    /// <summary>
    /// 初始化并开始更新流程
    /// </summary>
    public async UniTask<bool> InitializeAndUpdate()
    {
        try
        {
            OnStepChange?.Invoke("初始化YooAsset...");
            YooAssets.Initialize();

            // 初始化资源包
            if (!await InitializePackage())
                return false;

            // 请求资源版本
            if (!await RequestPackageVersion())
                return false;

            // 更新资源清单
            if (!await UpdatePackageManifest())
                return false;

            // 检查并下载更新文件
            if (!await CheckAndDownloadFiles())
                return false;

            // 清理缓存文件
            await ClearCacheFiles();

            OnStepChange?.Invoke("YooAsset更新完成");
            FinishUpdate();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"YooAsset更新失败: {e.Message}");
            OnError?.Invoke($"YooAsset更新失败: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 初始化资源包
    /// </summary>
    private async UniTask<bool> InitializePackage()
    {
        OnStepChange?.Invoke("初始化资源包!");

        var currentPlayMode = GetPlayMode();

        // 创建资源包裹类
        _package = YooAssets.TryGetPackage(packageName);
        if (_package == null)
            _package = YooAssets.CreatePackage(packageName);

        InitializationOperation initializationOperation = null;

        // 编辑器下的模拟模式
        if (currentPlayMode == EPlayMode.EditorSimulateMode)
        {
            var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
            var packageRoot = buildResult.PackageRootDirectory;
            var createParameters = new EditorSimulateModeParameters();
            createParameters.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            initializationOperation = _package.InitializeAsync(createParameters);
        }
        // 单机运行模式
        else if (currentPlayMode == EPlayMode.OfflinePlayMode)
        {
            var createParameters = new OfflinePlayModeParameters();
            createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            initializationOperation = _package.InitializeAsync(createParameters);
        }
        // 联机运行模式
        else if (currentPlayMode == EPlayMode.HostPlayMode)
        {
            IRemoteServices remoteServices = new RemoteServices(packageURL, packageURL);
            var createParameters = new HostPlayModeParameters();
            createParameters.BuildinFileSystemParameters = null;
            createParameters.CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
            initializationOperation = _package.InitializeAsync(createParameters);
        }
        // WebGL运行模式
        else if (currentPlayMode == EPlayMode.WebPlayMode)
        {
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
            var createParameters = new WebPlayModeParameters();
            string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE";
            IRemoteServices remoteServices = new RemoteServices(packageURL, packageURL);
            createParameters.WebServerFileSystemParameters = WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);
            initializationOperation = _package.InitializeAsync(createParameters);
#else
            var createParameters = new WebPlayModeParameters();
            createParameters.WebServerFileSystemParameters = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
            initializationOperation = _package.InitializeAsync(createParameters);
#endif
        }

        await initializationOperation.ToUniTask();

        if (initializationOperation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning($"初始化资源包失败: {initializationOperation.Error}");
            OnError?.Invoke("初始化资源包失败!");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 请求资源版本
    /// </summary>
    private async UniTask<bool> RequestPackageVersion()
    {
        OnStepChange?.Invoke("请求资源版本!");

        var operation = _package.RequestPackageVersionAsync();
        await operation.ToUniTask();

        if (operation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning($"请求资源版本失败: {operation.Error}");
            OnError?.Invoke("请求资源版本失败!");
            return false;
        }

        Debug.Log($"Request package version: {operation.PackageVersion}");
        _packageVersion = operation.PackageVersion;
        return true;
    }

    /// <summary>
    /// 更新资源清单
    /// </summary>
    private async UniTask<bool> UpdatePackageManifest()
    {
        OnStepChange?.Invoke("更新资源清单!");

        var operation = _package.UpdatePackageManifestAsync(_packageVersion);
        await operation.ToUniTask();

        if (operation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning($"更新资源清单失败: {operation.Error}");
            OnError?.Invoke("更新资源清单失败!");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 检查并下载更新文件
    /// </summary>
    private async UniTask<bool> CheckAndDownloadFiles()
    {
        OnStepChange?.Invoke("创建资源下载器!");

        int downloadingMaxNum = 10;
        int failedTryAgain = 3;
        _downloader = _package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);

        if (_downloader.TotalDownloadCount == 0)
        {
            Debug.Log("No files to download!");
            return true;
        }

        // 发现更新文件，直接开始下载（不等待用户点击）
        int totalDownloadCount = _downloader.TotalDownloadCount;
        long totalDownloadBytes = _downloader.TotalDownloadBytes;

        OnFoundUpdateFiles?.Invoke(totalDownloadCount, totalDownloadBytes);

        return await DownloadFiles();
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    private async UniTask<bool> DownloadFiles()
    {
        OnStepChange?.Invoke("开始下载资源文件!");

        // 设置下载回调
        _downloader.DownloadUpdateCallback = (updateData) =>
        {
            OnDownloadProgress?.Invoke(updateData.CurrentDownloadCount, updateData.TotalDownloadCount,
                updateData.CurrentDownloadBytes, updateData.TotalDownloadBytes);
        };

        _downloader.DownloadErrorCallback = (errorData) =>
        {
            Debug.LogError($"下载文件失败: {errorData.FileName}, {errorData.ErrorInfo}");
            OnError?.Invoke($"下载文件失败: {errorData.FileName}");
        };

        _downloader.BeginDownload();
        await _downloader.ToUniTask();

        if (_downloader.Status != EOperationStatus.Succeed)
        {
            OnError?.Invoke("资源下载失败!");
            return false;
        }

        OnStepChange?.Invoke("资源文件下载完毕!");
        return true;
    }

    /// <summary>
    /// 清理缓存文件
    /// </summary>
    private async UniTask ClearCacheFiles()
    {
        OnStepChange?.Invoke("清理未使用的缓存文件!");

        var operation = _package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
        await operation.ToUniTask();
    }

    /// <summary>
    /// 完成YooAsset更新
    /// </summary>
    public void FinishUpdate()
    {
        // 设置默认的资源包
        var gamePackage = YooAssets.GetPackage(packageName);
        YooAssets.SetDefaultPackage(gamePackage);
        Debug.Log("YooAsset更新完成");
    }

    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    private class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }

        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }

        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }
}