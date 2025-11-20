using YooAsset;

public class AOTGlobalConstants
    {
        /// <summary>
        /// 主热更程序集名称
        /// </summary>
        public const string HOT_UPDATE_ASSEMBLY_NAME = "HotUpdate.dll";
    
        /// <summary>
        /// 默认入口类名
        /// </summary>
        public const string DEFAULT_MAIN_CLASS = "GameMain";
    
        /// <summary>
        /// 默认入口方法名
        /// </summary>
        public const string DEFAULT_MAIN_METHOD = "Start";
        
        /// <summary>
        /// 默认资源包名称
        /// </summary>
        public const string DEFAULT_PACKAGE_NAME = "DefaultPackage";
        
        /// <summary>
        /// 默认资源包服务器地址
        /// </summary>
        public const string DEFAULT_PACKAGE_URL = "https://192.168.1.167:8084/XFramework";
        
        public static EPlayMode PlayMode
        {
            get
            {
#if UNITY_EDITOR
                return EPlayMode.EditorSimulateMode;
#else
    #if RESOURCE_OFFLINE
        return EPlayMode.OfflinePlayMode;
    #else 
        return EPlayMode.HostPlayMode;
    #endif
#endif
            }
        }
    }
