using System;
using System.Collections;
using System.Collections.Generic;
using SimpleGameFramework.Base;
using UniFramework.Event;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using YooAsset;

public class Launcher :MonoBehaviour
{
    //资源配置
    public YooAssetConfig yooAssetConfig;
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

        //..................资源热更........................
        YooAssets.Initialize();
        // 加载更新页面
        var go = Resources.Load<GameObject>("PatchWindow");
        GameObject.Instantiate(go);
        // 开始补丁更新流程
        var operation = new PatchOperation(this,yooAssetConfig);
        YooAssets.StartOperation(operation);
        yield return operation;
        // 设置默认的资源包
        var gamePackage = YooAssets.GetPackage("DefaultPackage");
        YooAssets.SetDefaultPackage(gamePackage);
        
        //..................代码热更........................
        
        
        //..................框架初始化........................
        
        //..................切换主场景........................
        YooAssets.LoadSceneAsync("Assets/Jit/PakageAsset/Scenes/Home");
    }
    void Update()
    {
       
    }

    void FixedUpdate()
    {
       
    }

    private void OnDestroy()
    {
       
    }
}

[System.Serializable]
public class YooAssetConfig
{
    public EPlayMode playMode ;
    public string packageName ;
    public string HostServerURL ;
}
