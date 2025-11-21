using UnityEngine;

public class Launcher : MonoBehaviour
{
    public YooAssetModule yooAssetModule;
    public HybridCLRModule hybridCLRModule;
    private PatchWindow patchWindow;

    void Awake()
    {
        Log.Info("Launcher初始化开始");
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);
        Log.Info("Launcher初始化完成");
    }

    async void Start()
    {
        try
        {
            Log.Info("开始启动游戏");
            //加载更新界面
            Log.Info("正在加载更新界面...");
            GameObject patchWindowGameObject = Instantiate(Resources.Load<GameObject>("PatchWindow"));
            patchWindow = patchWindowGameObject.AddComponent<PatchWindow>();
            // 连接事件
            ConnectManagerEvents();
            Log.Info("UI事件连接完成");
            //..................资源更新........................
            Log.Info("开始资源更新流程");
            if (!await yooAssetModule.InitializeAndUpdate())
            {
                Log.Error("资源更新失败，启动流程终止");
                return;
            }
            //..................代码更新........................
            Log.Info("开始代码更新流程");
            await hybridCLRModule.StartHybridCLRUpdate();
            Log.Info("游戏启动流程完成");
        }
        catch (System.Exception e)
        {
            Log.Error($"启动流程异常: {e.Message}");
            if (patchWindow != null)
                patchWindow.OnError($"启动流程异常: {e.Message}");
        }
    }

    private void ConnectManagerEvents()
    {
        // 连接UI事件，用于显示进度和错误信息
        yooAssetModule.OnStepChange += patchWindow.OnStepChange;
        yooAssetModule.OnFoundUpdateFiles += patchWindow.OnFoundUpdateFiles;
        yooAssetModule.OnDownloadProgress += patchWindow.OnDownloadProgress;
        yooAssetModule.OnError += patchWindow.OnError;

        hybridCLRModule.OnStepChange += patchWindow.OnStepChange;
        hybridCLRModule.OnError += patchWindow.OnError;
    }
}