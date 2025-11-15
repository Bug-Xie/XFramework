
using System;

namespace Framework
{
    /// <summary>
    /// 状态机节点接口
    /// </summary>
    public interface IFsmNode : IDisposable
    {
        /// <summary>
        /// 节点创建时调用
        /// </summary>
        /// <param name="machine">状态机实例</param>
        void OnCreate(FsmMachine machine);

        /// <summary>
        /// 进入状态时调用
        /// </summary>
        void OnEnter();

        /// <summary>
        /// 状态更新时调用
        /// </summary>
        void OnUpdate();

        /// <summary>
        /// 退出状态时调用
        /// </summary>
        void OnExit();
    }
}