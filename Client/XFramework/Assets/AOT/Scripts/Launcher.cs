using System;
using System.Collections;
using System.Collections.Generic;
using SimpleGameFramework.Base;
using UniFramework.Event;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using YooAsset;

public class Launcher : MonoBehaviour
{
    //资源配置
    public YooAssetConfig yooAssetConfig;
    //DLL配置
    public HybridCLRConfig hybridCLRConfig;
    void Awake()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
        DontDestroyOnLoad(this.gameObject);
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
        var operation = new PatchOperation(this, yooAssetConfig,hybridCLRConfig);
        YooAssets.StartOperation(operation);
        yield return operation;
    }
}

[System.Serializable]
public class YooAssetConfig
{
    public EPlayMode playMode;
    public string packageName;
    public string HostServerURL;
}

[System.Serializable]
public class HybridCLRConfig
{
    public string hotScriptDllPath;
    public string hotScriptDllName;
    public string mainScriptName;
    public string mainScriptMethod;
}
