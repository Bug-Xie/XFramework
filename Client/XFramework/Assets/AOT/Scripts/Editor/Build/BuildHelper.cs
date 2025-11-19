using UnityEngine;
using System.IO;
using System.Collections.Generic;
namespace AOT.Scripts.Editor.Build
{
    public static class BuildHelper
    {
        /// <summary>
        /// 获取构建输出目录（Unity生成的中间文件）
        /// </summary>
        public static string GetDLLOutputPath() => Path.Combine(Application.dataPath, "Library/YooAssetBuild", AOTGlobalConstants.DEFAULT_PACKAGE_NAME);

        /// <summary>
        /// 获取内置资源目录（StreamingAssets下）
        /// 匹配实际的Yoo文件夹名称（大写Y）
        /// </summary>
        public static string GetAOTDLLDir() => Path.Combine(Application.streamingAssetsPath, "Jit/PakageAsset/AOTDLL");

        /// <summary>
        /// 获取热更新DLL输出目录（Assets/HotCodeDll）
        /// 与图片中的目录位置一致
        /// </summary>
        public static string GetJITDllDir() => Path.Combine(Application.dataPath, "Jit/PakageAsset/JITDLL");
        
        //版本号文件路径
        public static string VersionFilePath => Path.Combine(Application.dataPath, "version.txt");
      
        public const string OFFLINE_MODE_SYMBOL = "RESOURCE_OFFLINE"; // 离线资源符号
        public const string ASSETBUNDLE_MODE_SYMBOL = "RESOURCE_ASSETBUNDLE"; // AssetBundle资源符号
        
        public static List<string> GetAotDLLNames()
        {
            return new List<string>
            {
                "System.Core.dll",
                "System.dll",
                "mscorlib.dll",
            };
        }
        
        public static List<string> GetJITDLLNames()
        {
            return new List<string>
            {
                AOTGlobalConstants.HOT_UPDATE_ASSEMBLY_NAME
            };
        }
    }
}