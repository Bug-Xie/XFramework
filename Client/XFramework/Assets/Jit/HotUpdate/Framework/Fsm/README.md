# Framework.Fsm 状态机模块

## 概述

提供线程安全的有限状态机实现，支持状态转换、黑板数据和生命周期管理。

## 核心文件

- `FsmMachine.cs` - 状态机核心实现
- `IFsmNode.cs` - 状态节点接口

## 快速使用

### 1. 定义状态节点

```csharp
public class IdleState : IFsmNode
{
    private FsmMachine machine;
    
    public void OnCreate(FsmMachine machine)
    {
        this.machine = machine;
    }
    
    public void OnEnter()
    {
        Debug.Log("进入空闲状态");
    }
    
    public void OnUpdate()
    {
        // 检查条件，切换到其他状态
        if (Input.GetKeyDown(KeyCode.Space))
        {
            machine.ChangeState<RunState>();
        }
    }
    
    public void OnExit()
    {
        Debug.Log("退出空闲状态");
    }
    
    public void Dispose()
    {
        // 清理资源
    }
}

public class RunState : IFsmNode
{
    private FsmMachine machine;
    
    public void OnCreate(FsmMachine machine) { this.machine = machine; }
    public void OnEnter() { Debug.Log("开始跑步"); }
    public void OnUpdate() 
    { 
        if (Input.GetKeyDown(KeyCode.S))
            machine.ChangeState<IdleState>();
    }
    public void OnExit() { Debug.Log("停止跑步"); }
    public void Dispose() { }
}
```

### 2. 使用状态机

```csharp
public class Player : MonoBehaviour
{
    private FsmMachine fsm;
    
    void Start()
    {
        // 创建状态机
        fsm = new FsmMachine(this);
        
        // 添加状态
        fsm.AddNode<IdleState>();
        fsm.AddNode<RunState>();
        
        // 启动状态机
        fsm.Run<IdleState>();
    }
    
    void Update()
    {
        // 更新状态机
        fsm.Update();
    }
    
    void OnDestroy()
    {
        // 释放状态机
        fsm?.Dispose();
    }
}
```

### 3. 黑板数据

```csharp
// 设置数据
fsm.SetBlackboardValue("Health", 100);
fsm.SetBlackboardValue("Speed", 5.0f);

// 获取数据
int health = fsm.GetBlackboardValue<int>("Health");
float speed = fsm.GetBlackboardValue<float>("Speed");

// 移除数据
fsm.RemoveBlackboardValue("Health");
```

### 4. 状态变化监听

```csharp
fsm.OnStateChanged += (oldState, newState) =>
{
    Debug.Log($"状态切换: {oldState} -> {newState}");
};
```

## 主要特性

- ✅ **线程安全** - 支持多线程环境
- ✅ **错误处理** - 完善的异常处理和状态恢复
- ✅ **生命周期管理** - 自动资源清理
- ✅ **黑板数据** - 状态间数据共享
- ✅ **事件通知** - 状态变化事件
- ✅ **热更新友好** - 纯C#实现

## 注意事项

1. **状态节点必须实现IFsmNode接口**
2. **记得在OnDestroy中调用Dispose()**
3. **状态切换在Update中进行，避免在OnEnter/OnExit中切换**
4. **黑板数据线程安全，可在任意线程访问**

## 常见问题

**Q: 如何在状态中访问其他组件？**
A: 通过Owner属性或黑板数据传递引用。

**Q: 状态切换失败怎么办？**
A: 系统会自动尝试恢复到之前状态，并输出错误日志。

**Q: 可以嵌套状态机吗？**
A: 可以，在状态节点中创建子状态机即可。