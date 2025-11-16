using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using YooAsset;

public class FsmUpdatePackageManifest : IStateNode
{
    private StateMachine _machine;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }
    void IStateNode.OnEnter()
    {
        PatchEventDefine.PatchStepsChange.SendEventMessage("更新资源清单！");
        ((MonoBehaviour)_machine.GetBlackboardValue("Behaviour")).StartCoroutine(UpdateManifest());
    }
    void IStateNode.OnUpdate()
    {
    }
    void IStateNode.OnExit()
    {
    }

    private IEnumerator UpdateManifest()
    {
        var packageName = ((YooAssetConfig)_machine.GetBlackboardValue("yooAssetConfig")).packageName;
        var packageVersion = (string)_machine.GetBlackboardValue("PackageVersion");
        var package = YooAssets.GetPackage(packageName);
        var operation = package.UpdatePackageManifestAsync(packageVersion);
        yield return operation;

        if (operation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning(operation.Error);
            PatchEventDefine.PackageManifestUpdateFailed.SendEventMessage();
            yield break;
        }
        else
        {
            _machine.ChangeState<FsmCreateDownloader>();
        }
    }
}