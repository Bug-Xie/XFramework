using UnityEngine;
using YooAsset;
#if UNITY_EDITOR
using UnityEditor;
#endif



[CreateAssetMenu(fileName = "AOTGlobalConfig", menuName = "ScriptObject/AOTGlobalConfig", order = 1)]
public class AOTGlobalConfig : ScriptableObject
{
    [Header("YooAsset设置")]
    public AOTGlobalYooAssetConfig aotGlobalYooAssetConfig;
    [Header("HybridCLR设置")]
    public AOTGlobalHybridCLRConfig aotGlobalHybridClrConfig;

}
[System.Serializable]
public class AOTGlobalYooAssetConfig
{
    public EPlayMode playMode ;
    public string packageName ;
    public string hostServerURL ;
}

[System.Serializable]
public class AOTGlobalHybridCLRConfig
{
    public string hotScriptDllPath;
    public string hotScriptDllName ;
    public string mainScriptName ;
    public string mainScriptMethod;
}

