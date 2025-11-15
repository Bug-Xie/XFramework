using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 有限状态机
    /// 支持线程安全、错误处理和生命周期管理
    /// </summary>
    public class FsmMachine : IDisposable
    {
        private readonly Dictionary<string, object> m_Blackboard = new Dictionary<string, object>(100);
        private readonly Dictionary<Type, IFsmNode> m_NodesByType = new Dictionary<Type, IFsmNode>(100);
        private readonly Dictionary<string, Type> m_TypeCache = new Dictionary<string, Type>();
        private readonly object m_Lock = new object();
        
        private IFsmNode m_CurrentNode;
        private IFsmNode m_PreviousNode;
        private bool m_IsDisposed = false;

        /// <summary>
        /// 状态机持有者
        /// </summary>
        public object Owner { get; private set; }

        /// <summary>
        /// 当前运行的节点名称
        /// </summary>
        public string CurrentNode
        {
            get
            {
                lock (m_Lock)
                {
                    return m_CurrentNode?.GetType().FullName ?? string.Empty;
                }
            }
        }

        /// <summary>
        /// 之前运行的节点名称
        /// </summary>
        public string PreviousNode
        {
            get
            {
                lock (m_Lock)
                {
                    return m_PreviousNode?.GetType().FullName ?? string.Empty;
                }
            }
        }

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning
        {
            get
            {
                lock (m_Lock)
                {
                    return m_CurrentNode != null;
                }
            }
        }

        /// <summary>
        /// 状态变化事件
        /// </summary>
        public event Action<string, string> OnStateChanged;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="owner">状态机持有者</param>
        public FsmMachine(object owner = null)
        {
            Owner = owner;
        }

        /// <summary>
        /// 更新状态机
        /// </summary>
        public void Update()
        {
            if (m_IsDisposed)
                return;

            IFsmNode currentNode;
            lock (m_Lock)
            {
                currentNode = m_CurrentNode;
            }

            try
            {
                currentNode?.OnUpdate();
            }
            catch (Exception ex)
            {
                Debug.LogError($"状态机更新异常: {ex}");
            }
        }

        /// <summary>
        /// 启动状态机
        /// </summary>
        /// <typeparam name="TNode">节点类型</typeparam>
        public void Run<TNode>() where TNode : class, IFsmNode, new()
        {
            if (m_IsDisposed)
                throw new ObjectDisposedException(nameof(FsmMachine));

            Type nodeType = typeof(TNode);
            Run(nodeType);
        }

        /// <summary>
        /// 启动状态机
        /// </summary>
        /// <param name="entryNodeType">入口节点类型</param>
        public void Run(Type entryNodeType)
        {
            if (m_IsDisposed)
                throw new ObjectDisposedException(nameof(FsmMachine));

            if (entryNodeType == null)
                throw new ArgumentNullException(nameof(entryNodeType));

            if (!typeof(IFsmNode).IsAssignableFrom(entryNodeType))
                throw new ArgumentException($"类型 {entryNodeType.Name} 必须实现 IFsmNode 接口");

            lock (m_Lock)
            {
                IFsmNode node = GetOrCreateNode(entryNodeType);
                if (node == null)
                    throw new InvalidOperationException($"无法创建状态节点: {entryNodeType.FullName}");

                try
                {
                    m_CurrentNode = node;
                    m_PreviousNode = node;
                    m_CurrentNode.OnEnter();
                }
                catch (Exception ex)
                {
                    m_CurrentNode = null;
                    m_PreviousNode = null;
                    Debug.LogError($"启动状态机失败: {ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// 添加状态节点
        /// </summary>
        /// <typeparam name="TNode">节点类型</typeparam>
        public void AddNode<TNode>() where TNode : class, IFsmNode, new()
        {
            if (m_IsDisposed)
                throw new ObjectDisposedException(nameof(FsmMachine));

            Type nodeType = typeof(TNode);
            AddNode(nodeType);
        }

        /// <summary>
        /// 添加状态节点
        /// </summary>
        /// <param name="nodeType">节点类型</param>
        public void AddNode(Type nodeType)
        {
            if (m_IsDisposed)
                throw new ObjectDisposedException(nameof(FsmMachine));

            if (nodeType == null)
                throw new ArgumentNullException(nameof(nodeType));

            if (!typeof(IFsmNode).IsAssignableFrom(nodeType))
                throw new ArgumentException($"类型 {nodeType.Name} 必须实现 IFsmNode 接口");

            lock (m_Lock)
            {
                if (!m_NodesByType.ContainsKey(nodeType))
                {
                    GetOrCreateNode(nodeType);
                }
            }
        }

        /// <summary>
        /// 移除状态节点
        /// </summary>
        /// <typeparam name="TNode">节点类型</typeparam>
        public void RemoveNode<TNode>() where TNode : class, IFsmNode
        {
            RemoveNode(typeof(TNode));
        }

        /// <summary>
        /// 移除状态节点
        /// </summary>
        /// <param name="nodeType">节点类型</param>
        public void RemoveNode(Type nodeType)
        {
            if (m_IsDisposed)
                return;

            if (nodeType == null)
                return;

            lock (m_Lock)
            {
                if (m_NodesByType.TryGetValue(nodeType, out IFsmNode node))
                {
                    // 如果是当前状态，不能移除
                    if (m_CurrentNode == node)
                    {
                        Debug.LogWarning($"无法移除当前状态节点: {nodeType.Name}");
                        return;
                    }

                    m_NodesByType.Remove(nodeType);
                    
                    try
                    {
                        node.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"释放状态节点异常: {ex}");
                    }
                }
            }
        }

        /// <summary>
        /// 转换状态
        /// </summary>
        /// <typeparam name="TNode">目标状态类型</typeparam>
        public void ChangeState<TNode>() where TNode : class, IFsmNode, new()
        {
            ChangeState(typeof(TNode));
        }

        /// <summary>
        /// 转换状态
        /// </summary>
        /// <param name="nodeType">目标状态类型</param>
        public void ChangeState(Type nodeType)
        {
            if (m_IsDisposed)
                throw new ObjectDisposedException(nameof(FsmMachine));

            if (nodeType == null)
                throw new ArgumentNullException(nameof(nodeType));

            if (!typeof(IFsmNode).IsAssignableFrom(nodeType))
                throw new ArgumentException($"类型 {nodeType.Name} 必须实现 IFsmNode 接口");

            lock (m_Lock)
            {
                IFsmNode targetNode = GetOrCreateNode(nodeType);
                if (targetNode == null)
                    throw new InvalidOperationException($"无法获取状态节点: {nodeType.FullName}");

                if (m_CurrentNode == targetNode)
                    return; // 相同状态，不需要切换

                string oldState = m_CurrentNode?.GetType().FullName ?? string.Empty;
                string newState = nodeType.FullName;

                try
                {
                    // 退出当前状态
                    m_CurrentNode?.OnExit();

                    // 切换状态
                    m_PreviousNode = m_CurrentNode;
                    m_CurrentNode = targetNode;

                    // 进入新状态
                    m_CurrentNode.OnEnter();

                    // 触发状态变化事件
                    OnStateChanged?.Invoke(oldState, newState);
                }
                catch (Exception ex)
                {
                    // 状态切换失败，尝试恢复
                    Debug.LogError($"状态切换失败: {oldState} -> {newState}, 错误: {ex}");
                    
                    try
                    {
                        m_CurrentNode = m_PreviousNode;
                        m_CurrentNode?.OnEnter();
                    }
                    catch (Exception recoverEx)
                    {
                        Debug.LogError($"状态恢复失败: {recoverEx}");
                        m_CurrentNode = null;
                    }
                    
                    throw;
                }
            }
        }

        /// <summary>
        /// 停止状态机
        /// </summary>
        public void Stop()
        {
            if (m_IsDisposed)
                return;

            lock (m_Lock)
            {
                try
                {
                    m_CurrentNode?.OnExit();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"停止状态机异常: {ex}");
                }
                finally
                {
                    m_CurrentNode = null;
                    m_PreviousNode = null;
                }
            }
        }

        /// <summary>
        /// 设置黑板数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetBlackboardValue(string key, object value)
        {
            if (m_IsDisposed)
                return;

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("键不能为空", nameof(key));

            lock (m_Lock)
            {
                m_Blackboard[key] = value;
            }
        }

        /// <summary>
        /// 获取黑板数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public object GetBlackboardValue(string key)
        {
            if (m_IsDisposed)
                return null;

            if (string.IsNullOrEmpty(key))
                return null;

            lock (m_Lock)
            {
                return m_Blackboard.TryGetValue(key, out object value) ? value : null;
            }
        }

        /// <summary>
        /// 获取黑板数据（泛型版本）
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public T GetBlackboardValue<T>(string key)
        {
            object value = GetBlackboardValue(key);
            return value is T result ? result : default(T);
        }

        /// <summary>
        /// 移除黑板数据
        /// </summary>
        /// <param name="key">键</param>
        public void RemoveBlackboardValue(string key)
        {
            if (m_IsDisposed)
                return;

            if (string.IsNullOrEmpty(key))
                return;

            lock (m_Lock)
            {
                m_Blackboard.Remove(key);
            }
        }

        /// <summary>
        /// 清空黑板数据
        /// </summary>
        public void ClearBlackboard()
        {
            if (m_IsDisposed)
                return;

            lock (m_Lock)
            {
                m_Blackboard.Clear();
            }
        }

        /// <summary>
        /// 获取或创建节点
        /// </summary>
        /// <param name="nodeType">节点类型</param>
        /// <returns>节点实例</returns>
        private IFsmNode GetOrCreateNode(Type nodeType)
        {
            if (m_NodesByType.TryGetValue(nodeType, out IFsmNode existingNode))
                return existingNode;

            try
            {
                var node = Activator.CreateInstance(nodeType) as IFsmNode;
                if (node != null)
                {
                    node.OnCreate(this);
                    m_NodesByType[nodeType] = node;
                    return node;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"创建状态节点失败: {nodeType.FullName}, 错误: {ex}");
            }

            return null;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (m_IsDisposed)
                return;

            lock (m_Lock)
            {
                m_IsDisposed = true;

                // 停止状态机
                try
                {
                    m_CurrentNode?.OnExit();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"停止状态机异常: {ex}");
                }

                // 释放所有节点
                foreach (var kvp in m_NodesByType)
                {
                    try
                    {
                        kvp.Value.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"释放状态节点异常: {ex}");
                    }
                }

                // 清理数据
                m_NodesByType.Clear();
                m_TypeCache.Clear();
                m_Blackboard.Clear();
                m_CurrentNode = null;
                m_PreviousNode = null;
                OnStateChanged = null;
            }
        }
    }
}