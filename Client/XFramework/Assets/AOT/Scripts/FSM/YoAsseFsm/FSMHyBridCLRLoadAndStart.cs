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

    private Assembly hotUpdateAssembly ;
    private PatchOperation _owner;
    
    private List<AssetHandle> _aotHandles ;
    private List<AssetHandle> _jitHandles ;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
        _owner = machine.Owner as PatchOperation;
    }

    void IStateNode.OnEnter()
    {
        ((MonoBehaviour)_machine.GetBlackboardValue("Behaviour")).StartCoroutine(HyBridCLRUpdate());
    }

    IEnumerator HyBridCLRUpdate()
    {
        // 加载AOT元数据
        yield return LoadAOT();

        // 加载热更dll
        yield return LoadJITDLL();
        
        Debug.Log("HybridCLR更新完成");

        // 进入主入口
        yield return Main();
        
        // 所有操作完成后设置完成状态
        _owner.SetFinish();
    }

    //-------------------------------------加载AOT元数据------------------------------------------
    IEnumerator LoadAOT()
    {
#if UNITY_EDITOR
        // 编辑器模式下跳过AOT元数据加载
        Debug.Log("编辑器模式跳过AOT元数据加载");
        yield return null;
#else
        var package = YooAssets.GetPackage(AOTGlobalConstants.DEFAULT_PACKAGE_NAME);
        var locations = package.GetAssetInfos("AOTDLL");
        Debug.Log($"开始加载 {locations.Length} 个AOT DLL文件");

        foreach (var location in locations)
        {
            var handle = package.LoadAssetAsync<TextAsset>(location.Address);
            _aotHandles.Add(handle);
            yield return handle;
            
            if (handle.Status == EOperationStatus.Succeed)
            {
                TextAsset dllFile = handle.AssetObject as TextAsset;
                HomologousImageMode mode = HomologousImageMode.SuperSet;
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllFile.bytes, mode);
                
                if (err == LoadImageErrorCode.OK)
                {
                    Debug.Log($"成功加载并注册AOT元数据: {location.Address}, mode: {mode}");
                }
                else
                {
                    Debug.LogError($"注册AOT元数据失败: {location.Address}, 错误码: {err}");
                }
            }
            else
            {
                Debug.LogError($"加载AOT DLL失败: {location.Address}");
            }
        }
        
        Debug.Log($"AOT元数据加载完成，共 {_aotHandles.Count} 个文件");
#endif
    }

    //-------------------------------------加载热更dll------------------------------------------
    public IEnumerator LoadJITDLL()
    {
#if UNITY_EDITOR
        // 编辑器模式：直接从当前域中获取程序集
        Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => $"{a.GetName().Name}.dll" == AOTGlobalConstants.HOT_UPDATE_ASSEMBLY_NAME);
        if (assembly != null)
        {
            hotUpdateAssembly = assembly;
            Debug.Log($"编辑器模式HotUpdateDLL加载成功: {AOTGlobalConstants.HOT_UPDATE_ASSEMBLY_NAME}");
        }
        else
        {
            Debug.LogWarning($"编辑器模式找不到HotUpdateDLL: {AOTGlobalConstants.HOT_UPDATE_ASSEMBLY_NAME}");
            yield return null;
        }
#else
        // 非编辑器模式：使用YooAsset加载资源
        var package = YooAssets.GetPackage(AOTGlobalConstants.DEFAULT_PACKAGE_NAME);
        var locations = package.GetAssetInfos("JITDLL");
        Debug.Log($"开始加载 {locations.Length} 个JITDLL文件");

        foreach (var location in locations)
        {
            var handle = package.LoadAssetAsync<TextAsset>(location.Address);
            _jitHandles.Add(handle);
            yield return handle;
            
            if (handle.Status == EOperationStatus.Succeed)
            {
                TextAsset dllFile = handle.AssetObject as TextAsset;
                try
                {
                    Assembly loadedAssembly = Assembly.Load(dllFile.bytes);
                    
                    if (location.Address.Contains(AOTGlobalConstants.HOT_UPDATE_ASSEMBLY_NAME))
                    {
                        hotUpdateAssembly = loadedAssembly;
                        Debug.Log($"设置主热更程序集: {location.Address}");
                    }
                    
                    Debug.Log($"成功加载JIT DLL: {location.Address}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"加载程序集失败: {location.Address}, 错误: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"加载JIT DLL资源失败: {location.Address}");
            }
        }
        
        Debug.Log($"JIT DLL加载完成，共 {_jitHandles.Count} 个文件");
#endif
    }

    //-------------------------------------进入主入口------------------------------------------
    private IEnumerator Main()
    {
        if (hotUpdateAssembly == null)
        {
            Debug.LogError("热更程序集未加载，无法进入主入口");
            yield break;
        }

        try
        {
            Type type = hotUpdateAssembly.GetType(AOTGlobalConstants.DEFAULT_MAIN_CLASS);
            if (type == null)
            {
                Debug.LogError($"找不到主脚本类型: {AOTGlobalConstants.DEFAULT_MAIN_CLASS}");
                yield break;
            }

            MethodInfo method = type.GetMethod(AOTGlobalConstants.DEFAULT_MAIN_METHOD, BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                Debug.LogError($"找不到主方法: {AOTGlobalConstants.DEFAULT_MAIN_METHOD} 在类型 {AOTGlobalConstants.DEFAULT_MAIN_CLASS} 中");
                yield break;
            }

            Debug.Log($"调用热更入口: {AOTGlobalConstants.DEFAULT_MAIN_CLASS}.{AOTGlobalConstants.DEFAULT_MAIN_METHOD}");

            if (method.ReturnType == typeof(IEnumerator))
            {
                IEnumerator invokeCoroutine = (IEnumerator)method.Invoke(null, null); 
                ((MonoBehaviour)_machine.GetBlackboardValue("Behaviour")).StartCoroutine(invokeCoroutine);
            }
            else
            {
                method.Invoke(null, null);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"执行热更入口失败: {e}");
        }
    }

    void IStateNode.OnUpdate()
    {
    }

    void IStateNode.OnExit()
    {
        // 在状态退出时释放所有资源句柄
        foreach (var handle in _aotHandles)
        {
            if (handle.IsValid)
                handle.Release();
        }
        
        foreach (var handle in _jitHandles)
        {
            if (handle.IsValid)
                handle.Release();
        }
        
        _aotHandles.Clear();
        _jitHandles.Clear();
        
        Debug.Log("FSMHyBridCLRLoadAndStart 状态退出，资源已释放");
    }
}