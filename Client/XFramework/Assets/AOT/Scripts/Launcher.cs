using System.Collections;
using UniFramework.Event;
using UnityEngine;
using YooAsset;

public class Launcher : MonoBehaviour
{
    void Awake()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);
    }

    IEnumerator Start()
    {
        //..................基础模块........................
        UniEvent.Initalize();
        //..................资源+代码更新........................
        YooAssets.Initialize();
        // 加载更新页面
        var go = Resources.Load<GameObject>("PatchWindow");
        GameObject.Instantiate(go);
        // 开始补丁更新流程
        var operation = new PatchOperation(this);
        YooAssets.StartOperation(operation);
        yield return operation;
    }
}


