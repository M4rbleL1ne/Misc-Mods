/*using System.Diagnostics;
using System.Numerics;
using System.Runtime.Versioning;*/
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices
{
    public static class Unsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, JitIntrinsic, NonVersionable]
        public unsafe static void* AsPointer<T>(ref T value) => throw new PlatformNotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SizeOf<T>()
        {
            typeof(T)!.ToString();
            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]//, JitIntrinsic, NonVersionable]
        [return: NotNullIfNotNull(nameof(o))]
        public static T As<T>(object? o) where T : class? => throw new PlatformNotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]//, JitIntrinsic, NonVersionable]
        public static ref TTo As<TFrom, TTo>(ref TFrom source) => throw new PlatformNotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]//, JitIntrinsic, NonVersionable]
        public static ref T Add<T>(ref T source, int elementOffset)
        {
            typeof(T)!.ToString();
            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]//, JitIntrinsic, NonVersionable]
        public static ref T Add<T>(ref T source, IntPtr elementOffset)
        {
            typeof(T)!.ToString();
            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, JitIntrinsic, NonVersionable]
        public unsafe static void* Add<T>(void* source, int elementOffset)
        {
            typeof(T)!.ToString();
            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, JitIntrinsic, NonVersionable]
        public static ref T Add<T>(ref T source, UIntPtr elementOffset)
        {
            typeof(T)!.ToString();
            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, JitIntrinsic, NonVersionable]
        public static ref T AddByteOffset<T>(ref T source, UIntPtr byteOffset)
        {
            typeof(T)!.ToString();
            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]//, JitIntrinsic, NonVersionable]
        public static bool AreSame<T>([AllowNull] ref T left, [AllowNull] ref T right) => throw new PlatformNotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, JitIntrinsic, NonVersionable]
        public unsafe static void Copy<T>(void* destination, ref T source) => throw new PlatformNotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, JitIntrinsic, NonVersionable]
        public unsafe static void Copy<T>(ref T destination, void* source) => throw new PlatformNotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, JitIntrinsic, NonVersionable]
        public unsafe static void CopyBlock(void* destination, void* source, uint byteCount) => throw new PlatformNotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, JitIntrinsic, NonVersionable]
        public static void CopyBlock(ref byte destination, ref byte source, uint byteCount) => throw new PlatformNotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, JitIntrinsic, NonVersionable]
        public unsafe static void CopyBlockUnaligned(void* destination, void* source, uint byteCount) => throw new PlatformNotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, JitIntrinsic, NonVersionable]
        public static void CopyBlockUnaligned(ref byte destination, ref byte source, uint byteCount) => throw new PlatformNotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]//, JitIntrinsic, NonVersionable]
        public static bool IsAddressGreaterThan<T>([AllowNull] ref T left, [AllowNull] ref T right) => throw new PlatformNotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]//, JitIntrinsic, NonVersionable]
        public static bool IsAddressLessThan<T>([AllowNull] ref T left, [AllowNull] ref T right) => throw new PlatformNotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, JitIntrinsic, NonVersionable]
        public unsafe static void InitBlock(void* startAddress, byte value, uint byteCount) => throw new PlatformNotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, JitIntrinsic, NonVersionable]
        public static void InitBlock(ref byte startAddress, byte value, uint byteCount) => throw new PlatformNotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, JitIntrinsic, NonVersionable]
        public unsafe static void InitBlockUnaligned(void* startAddress, byte value, uint byteCount) => throw new PlatformNotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, JitIntrinsic, NonVersionable]
        public static void InitBlockUnaligned(ref byte startAddress, byte value, uint byteCount)
        {
            for (var num = 0u; num < byteCount; num++)
                AddByteOffset(ref startAddress, (UIntPtr)num) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, JitIntrinsic, NonVersionable]
        public unsafe static T ReadUnaligned<T>(void* source)
        {
            typeof(T)!.ToString();
            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]//, JitIntrinsic, NonVersionable]
        public static T ReadUnaligned<T>(ref byte source)
        {
            typeof(T)!.ToString();
            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, JitIntrinsic, NonVersionable]
        public unsafe static void WriteUnaligned<T>(void* destination, T value)
        {
            typeof(T)!.ToString();
            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]//, JitIntrinsic, NonVersionable]
        public static void WriteUnaligned<T>(ref byte destination, T value)
        {
            typeof(T)!.ToString();
            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]//, JitIntrinsic, NonVersionable]
        public static ref T AddByteOffset<T>(ref T source, IntPtr byteOffset) => throw new PlatformNotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, NonVersionable]
        public unsafe static T Read<T>(void* source) => As<byte, T>(ref *(byte*)source);

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, NonVersionable]
        public unsafe static void Write<T>(void* destination, T value) => As<byte, T>(ref *(byte*)destination) = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, JitIntrinsic, NonVersionable]
        public unsafe static ref T AsRef<T>(void* source) => ref As<byte, T>(ref *(byte*)source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]//, JitIntrinsic, NonVersionable]
        public static ref T AsRef<T>(scoped in T source) => throw new PlatformNotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]//, JitIntrinsic, NonVersionable]
        public static IntPtr ByteOffset<T>([AllowNull] ref T origin, [AllowNull] ref T target) => throw new PlatformNotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]//, JitIntrinsic, NonVersionable]
        public unsafe static ref T NullRef<T>() => ref AsRef<T>(null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]//, JitIntrinsic, NonVersionable]
        public unsafe static bool IsNullRef<T>(ref T source) => AsPointer(ref source) == null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]//, JitIntrinsic, NonVersionable]
        public static void SkipInit<T>(out T value) => throw new PlatformNotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]//, JitIntrinsic, NonVersionable]
        public static ref T Subtract<T>(ref T source, int elementOffset)
        {
            typeof(T)!.ToString();
            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, JitIntrinsic, NonVersionable]
        public unsafe static void* Subtract<T>(void* source, int elementOffset)
        {
            typeof(T)!.ToString();
            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]//, JitIntrinsic, NonVersionable]
        public static ref T Subtract<T>(ref T source, IntPtr elementOffset)
        {
            typeof(T)!.ToString();
            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, JitIntrinsic, NonVersionable]
        public static ref T Subtract<T>(ref T source, UIntPtr elementOffset)
        {
            typeof(T)!.ToString();
            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]//, JitIntrinsic, NonVersionable]
        public static ref T SubtractByteOffset<T>(ref T source, IntPtr byteOffset) => throw new PlatformNotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining), CLSCompliant(false)]//, JitIntrinsic, NonVersionable]
        public static ref T SubtractByteOffset<T>(ref T source, UIntPtr byteOffset)
        {
            typeof(T)!.ToString();
            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]//, JitIntrinsic, NonVersionable]
        public static ref T Unbox<T>(object box) where T : struct => throw new PlatformNotSupportedException();
    }
}

/*namespace System.Numerics
{
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property)]
    class JitIntrinsicAttribute : Attribute { }
}

namespace System.Runtime.Versioning
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false, Inherited = false), Conditional("FEATURE_READYTORUN")]
    sealed class NonVersionableAttribute : Attribute { }
}*/

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
    sealed class NotNullIfNotNullAttribute : Attribute
    {
        public string ParameterName { get; }

        public NotNullIfNotNullAttribute(string parameterName) => ParameterName = parameterName;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
    sealed class AllowNullAttribute : Attribute { }
}