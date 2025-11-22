using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using Cysharp.Threading.Tasks;
public class GameMain
{
   
    public static async UniTask  Start()
    { 
        await YooAssets.LoadSceneAsync("Assets/JIT/PakageAsset/Scenes/Home");
        // yield return sceneOperation;
        // yield return null;
        // // 现在查找对象
        GameObject cube = GameObject.Find("Cube");
        if (cube == null)
        {
            Debug.LogError("找不到名为 'Cube' 的游戏对象！");
            //yield break;
        }
        
        Transform go = cube.transform;
        go.localScale = new Vector3(1, 4, 1);
        //Debug.Log("Cube缩放设置完成");
        
    }
}


