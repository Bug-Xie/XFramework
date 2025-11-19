using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
public class GameMain
{
    public static void  Start()
    { 
        var sceneOperation = YooAssets.LoadSceneAsync("Assets/Jit/PakageAsset/Scenes/Home");
        // yield return sceneOperation;
      // yield return null;
      // // 现在查找对象
      // GameObject cube = GameObject.Find("Cube");
      // if (cube == null)
      // {
      //     Debug.LogError("找不到名为 'Cube' 的游戏对象！");
      //     yield break;
      // }
      //   
      // Transform go = cube.transform;
      // go.localScale = new Vector3(3, 5, 3);
      // Debug.Log("Cube缩放设置完成");
    }
}


