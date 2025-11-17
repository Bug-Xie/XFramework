using System.Collections.Generic;
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
    public List<string> AOTScriptDllNames;
    public string mainScriptName ;
    public string mainScriptMethod;
}


#if UNITY_EDITOR
[CustomEditor(typeof(AOTGlobalConfig))]
public class GameConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 强制刷新序列化对象
        serializedObject.Update();
        
        // 绘制默认Inspector
        DrawDefaultInspector();
        
        // 应用修改
        serializedObject.ApplyModifiedProperties();
        
        // 强制重绘
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
            Repaint();
        }
    }
    
    void OnEnable()
    {
        // 确保在启用时刷新
        EditorApplication.delayCall += () => Repaint();
    }
}
#endif
