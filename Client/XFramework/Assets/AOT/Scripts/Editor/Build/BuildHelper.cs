using UnityEngine;
using System.IO;
using System.Collections.Generic;

    public static class BuildHelper
    {
        public static string GetAOTDLLDir() => Path.Combine(Application.dataPath, "JIT/PakageAsset/AOTDLL");
        public static string GetJITDllDir() => Path.Combine(Application.dataPath, "JIT/PakageAsset/JITDLL");
        //版本号文件路径
        public static string VersionFilePath => Path.Combine(Application.dataPath, "AOT/Scripts/Editor/Build/Buildversion.txt");
      
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
