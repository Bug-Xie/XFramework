using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniFramework.Machine;
using System.Reflection;
using HybridCLR;
using YooAsset;

internal class FSMHyBridCLRLoadAndStart : IStateNode
{
    private StateMachine _machine;

    // 配置常量
    private string hotScriptDllPath;
    private string mainScriptName;
    private string mainScriptMethod;
    private string hotScriptDllName;

    private Assembly hotUpdateAssembly = null;

    private static List<string> AOTMetaAssemblyFiles { get; } = new List<string>()
    {
#if UNITY_ANDROID || UNITY_EDITOR
        "System.Core.dll",
        "System.dll",
        "mscorlib.dll",
#endif
#if UNITY_IOS
            "DOTween.dll",
            "Google.Protobuf.dll",
            "Luban.Runtime.dll",
            "Newtonsoft.Json.dll",
            "System.Core.dll",
            "System.dll",
            "UniTask.dll",
            "Unity.Collections.dll",
            "UnityEngine.CoreModule.dll",
            "mscorlib.dll",
            "spine-unity.dll",

#endif
    };

    private PatchOperation _owner;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
        _owner = machine.Owner as PatchOperation;
    }

    void IStateNode.OnEnter()
    {
        hotScriptDllPath = ((AOTGlobalConfig)_machine.GetBlackboardValue("AOTGlobalConfig")).aotGlobalHybridClrConfig
            .hotScriptDllPath;
        mainScriptName = ((AOTGlobalConfig)_machine.GetBlackboardValue("AOTGlobalConfig")).aotGlobalHybridClrConfig
            .mainScriptName;
        mainScriptMethod = ((AOTGlobalConfig)_machine.GetBlackboardValue("AOTGlobalConfig")).aotGlobalHybridClrConfig
            .mainScriptMethod;
        hotScriptDllName = ((AOTGlobalConfig)_machine.GetBlackboardValue("AOTGlobalConfig")).aotGlobalHybridClrConfig
            .hotScriptDllName;
        ((MonoBehaviour)_machine.GetBlackboardValue("Behaviour")).StartCoroutine(HyBridCLRUpdate());
        _owner.SetFinish();
    }

    IEnumerator HyBridCLRUpdate()
    {
        //加载AOT元数据
        yield return LoadAOT();

        //加载热更dll
        yield return LoadHotUpdate();
        Debug.Log("HybridCLR更新完成");
        //进入主入口
        yield return Main();
        _owner.SetFinish();
    }

    //-------------------------------------加载AOT元数据------------------------------------------
    IEnumerator LoadAOT()
    {
#if UNITY_EDITOR
          yield return null;
#else
        //加载AOT
        List<byte[]> AOTAssetDatas = new List<byte[]>();
        foreach (var name in AOTMetaAssemblyFiles)
        {
            string dllPath = $"{hotScriptDllPath}/{name}";
            Debug.Log($"开始加载热更新AOT程序集: {name}, 路径: {dllPath}");
            var package = YooAssets.GetPackage(((AOTGlobalConfig)_machine.GetBlackboardValue("AOTGlobalConfig"))
                .aotGlobalYooAssetConfig.packageName);
            var handle = package.LoadAssetAsync<TextAsset>(dllPath);
            yield return handle;
            if (handle.Status == EOperationStatus.Succeed)
            {
                TextAsset dllAsset = handle.GetAssetObject<TextAsset>();
                if (dllAsset != null)
                {
                    Debug.Log($"{name}AOTDLL加载成功, 字节长度: {dllAsset.bytes.Length}");
                    AOTAssetDatas.Add(dllAsset.bytes);
                }
                else
                {
                    Debug.LogError($"AOTDLL加载成功但AssetObject为空: {dllPath}");
                }
            }
            else
            {
                Debug.LogError($"AOTDLL加载失败: {dllPath}, 错误: {handle.LastError}");
            }

            // 释放资源句柄
            handle.Release();
        }

        //补充元数据
        /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var dllBytes in AOTAssetDatas)
        {
            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            Debug.Log($"LoadMetadataForAOTAssembly:{dllBytes}. mode:{mode} ret:{err}");
        }
#endif
    }

    //-------------------------------------加载热更dll------------------------------------------
    public IEnumerator LoadHotUpdate()
    {
#if UNITY_EDITOR
        // 编辑器模式：直接从当前域中获取程序集
        Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => $"{a.GetName().Name}.dll" == hotScriptDllName);
        if (assembly != null)
        {
            hotUpdateAssembly = assembly;
            Debug.Log($"编辑器模式HotUpdateDLL加载成功: {hotScriptDllName}");
        }
        else
        {
            Debug.LogError($"编辑器模式找不到HotUpdateDLL: {hotScriptDllName}");
        }
        yield return null;
#else
        // 非编辑器模式：使用YooAsset加载资源
        string dllPath = $"{hotScriptDllPath}/{hotScriptDllName}";
        var package = YooAssets.GetPackage(((AOTGlobalConfig)_machine.GetBlackboardValue("AOTGlobalConfig"))
            .aotGlobalYooAssetConfig.packageName);
        var handle = package.LoadAssetAsync<TextAsset>(dllPath);
        yield return handle;
        if (handle.Status == EOperationStatus.Succeed)
        {
            TextAsset dllAsset = handle.GetAssetObject<TextAsset>();
            if (dllAsset != null)
            {
                Debug.Log($"{hotScriptDllName}HotUpdateDLL加载成功, 字节长度: {dllAsset.bytes.Length}");
                hotUpdateAssembly = Assembly.Load(dllAsset.bytes);
            }
            else
            {
                Debug.LogError($"HotUpdateDLL加载成功但AssetObject为空: {dllPath}");
            }
        }
        else
        {
            Debug.LogError($"HotUpdateDLL加载失败: {dllPath}, 错误: {handle.LastError}");
        }

        // 释放资源句柄
        handle.Release();
#endif
    }

    //-------------------------------------进入主入口------------------------------------------
    private IEnumerator Main()
    {
        // 加载程序集
        Type type = hotUpdateAssembly.GetType(mainScriptName);
        // 获取并调用启动方法
        MethodInfo method = type.GetMethod(mainScriptMethod);
        // 调用方法（假设返回IEnumerator，用协程执行）
        if (method.ReturnType == typeof(IEnumerator))
        {
            IEnumerator invokeCoroutine = (IEnumerator)method.Invoke(null, null);
            yield return ((MonoBehaviour)_machine.GetBlackboardValue("Behaviour")).StartCoroutine(invokeCoroutine);
        }
        else
        {
            method.Invoke(null, null);
        }
    }

    void IStateNode.OnUpdate()
    {
    }

    void IStateNode.OnExit()
    {
    }
}