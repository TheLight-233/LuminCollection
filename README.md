# LuminCollection



## High-performance C# unmanaged collections based on Unsafe and mimalloc


LuminCollection是基于Unsafe和mimalloc的C#非托管集合，它具有以下优点：

*   通过mimalloc实现高性能内存分配
*   基于Unsafe实现指针操作
*   无Runtime GC
*   可与LuminJob交互



## 后续更新计划

*   LuminConcurrentDictionary
*   LuminConcurrentHashSet
*   LuminConcurrentBag
*   LuminConcurrentQueue
*   LuminConcurrentStack



## Installation

**LuminCollection**依赖**LuminAllocator**内存分配器（基于mimalloc的封装）。

因此使用LuminCollection前请确保您的项目安装了**LuminAllocator。**





## Quick Start

LuminCollection将所有集合拆成Unsafe版和Safe版。

前者位于LuminCollection.UnsafeCollection命名空间下，后者位于LuminCollection命名空间

使用方法基本与BCL集合一致，以下为部分集合示例：

```cpp
static void Main()
{
    var random = new System.Random();
    
    using LuminDictionary<int, int> dictionary = new LuminDictionary<int, int>();
    using LuminHashSet<int> hashSet = new LuminHashSet<int>();
    using LuminPriorityQueue<int, int> priorityQueue = new LuminPriorityQueue<int, int>(); 
    using LuminList<int> array = new LuminList<int>();
    using LuminStack<int> stack = new LuminStack<int>();
    using LuminArray<int> array2 = new LuminArray<int>(10);

    Console.WriteLine(hashSet.Capacity);
    hashSet.Add(1);
    dictionary.Add(1, 1);
    dictionary.Add(2, 2);
    Console.WriteLine(dictionary.Count);
    Console.WriteLine(dictionary.ContainsValue(2));
    Console.WriteLine(dictionary.ContainsKey(3));
    Console.WriteLine(dictionary[1]);
    Console.WriteLine(hashSet.Contains(1));
        
    priorityQueue.Enqueue(1, 1);
    priorityQueue.Enqueue(2, 2);

    while (priorityQueue.TryDequeue(out var value))
    {
        Console.WriteLine(value);
    }
        
    for (int i = 0; i < 10; i++)
    {
        array.Add(random.Next());
        stack.Push(i);
        array2[i] = random.Next();
    }

    foreach (ref var v in array)
    {
        Console.WriteLine(v); 
    }
        
    Console.WriteLine("排序");
    array.Sort();
    foreach (ref var v in array)
    {
        Console.WriteLine(v); 
    }
        
    Console.WriteLine("<UNK>");
        
    foreach (ref var v in array2)
    {
        Console.WriteLine(v); 
    }
}
```



## 关于\_version字段

参照Span / ReadOnlySpan内部并不维护version，并且Span的迭代器的Current返回的是ref。

因此我个人认为微软对于Unsafe集合，不应有过多限制。

因此部分LuminCollection例如LuminList，LuminArray内部不维护version，并且迭代器Current同样通过ref返回

用户可以通过以下方式在Foreach里修改值

```cpp
foreach (ref var v in array)
{
    Console.WriteLine(v); 
}
```



## License

This library is licensed under the MIT License.



