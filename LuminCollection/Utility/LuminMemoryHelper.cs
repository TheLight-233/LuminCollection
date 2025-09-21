using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LuminCollection;

public static unsafe class LuminMemoryHelper
{
    
    #region 辅助方法

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AbortOrThrow()
    {
        throw new OutOfMemoryException();
    }

#if DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddMemoryPressure(long bytes)
    {
        if (bytes <= 0) return;
        
        GC.AddMemoryPressure(bytes);
    }

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RemoveMemoryPressure(long bytes)
    {
        if (bytes <= 0) return;
        
        GC.RemoveMemoryPressure(bytes);
    }
    
#endif
    /// <summary>
    /// 获取类型的对齐要求
    /// </summary>
    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint AlignOf<T>() where T : unmanaged => (nuint)sizeof(AlignOfHelper<T>) - (nuint)sizeof(T);

    /// <summary>
    /// 检查指针是否对齐
    /// </summary>
    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAligned<T>(void* ptr, nuint elementCount = 1) where T : unmanaged
    {
        nuint alignment = AlignOf<T>();
        return elementCount > 1 ? IsAligned<T>() && (nint)ptr % (nint)alignment == 0 : (nint)ptr % (nint)alignment == 0;
    }

    /// <summary>
    /// 检查类型是否自然对齐
    /// </summary>
    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAligned<T>() where T : unmanaged => (nuint)sizeof(T) % AlignOf<T>() == 0;

    /// <summary>
    /// 向上对齐到指定边界
    /// </summary>
    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint AlignUp(nuint size, nuint alignment) => (size + (alignment - 1)) & ~(alignment - 1);

    /// <summary>
    /// 向下对齐到指定边界
    /// </summary>
    [Intrinsic]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint AlignDown(nuint size, nuint alignment) => size - (size & (alignment - 1));

    /// <summary>Helper structure for calculating type alignment.</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct AlignOfHelper<T> where T : unmanaged
    {
        private byte _dummy;
        private T _data;
    }

    #endregion
}