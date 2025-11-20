using UnityEngine;

public class Launcher : MonoBehaviour
{
    public YooAssetModule yooAssetModule;
    public HybridCLRModule hybridCLRModule;
    private PatchWindow patchWindow;

    void Awake()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        try
        {
            //加载更新界面
            GameObject patchWindowGameObject = Instantiate(Resources.Load<GameObject>("PatchWindow"));
            patchWindow = patchWindowGameObject.AddComponent<PatchWindow>();
            // 连接事件
            ConnectManagerEvents();
            //..................资源更新........................
            if (!await yooAssetModule.InitializeAndUpdate())
                return;
            //..................代码更新........................
            await hybridCLRModule.StartHybridCLRUpdate();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"启动流程异常: {e.Message}");
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