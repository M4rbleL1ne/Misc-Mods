using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Contains generic, low-level functionality for manipulating pointers.
    /// </summary>
    public static class Unsafe
    {
        /// <summary>
        /// Returns a pointer to the given by-ref parameter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void* AsPointer<T>(ref T value) => throw new PlatformNotSupportedException();

        /// <summary>
        /// Returns the size of an object of the given type parameter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SizeOf<T>()
        {
            typeof(T).ToString();
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Casts the given object to the specified type, performs no dynamic type checking.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull(nameof(o))]
        public static T As<T>(object? o) where T : class? => throw new PlatformNotSupportedException();

        /// <summary>
        /// Reinterprets the given reference as a reference to a value of type <typeparamref name="TTo"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TTo As<TFrom, TTo>(ref TFrom source) => throw new PlatformNotSupportedException();

#pragma warning disable IDE0060
        /// <summary>
        /// Adds an element offset to the given reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(ref T source, int elementOffset)
        {
            typeof(T).ToString();
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Adds an element offset to the given reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(ref T source, nint elementOffset)
        {
            typeof(T).ToString();
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Adds an element offset to the given pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void* Add<T>(void* source, int elementOffset)
        {
            typeof(T).ToString();
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Adds an element offset to the given reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(ref T source, nuint elementOffset)
        {
            typeof(T).ToString();
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Adds an byte offset to the given reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddByteOffset<T>(ref T source, nuint byteOffset)
        {
            typeof(T).ToString();
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Determines whether the specified references point to the same location.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreSame<T>([AllowNull] ref readonly T left, [AllowNull] ref readonly T right) => throw new PlatformNotSupportedException();

        /// <summary>
        /// Reinterprets the given value of type <typeparamref name="TFrom" /> as a value of type <typeparamref name="TTo" />.
        /// </summary>
        /// <exception cref="NotSupportedException">The sizes of <typeparamref name="TFrom" /> and <typeparamref name="TTo" /> are not the same
        /// or the type parameters are not <see langword="struct"/>s.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TTo BitCast<TFrom, TTo>(TFrom source)
        {
            if (SizeOf<TFrom>() != SizeOf<TTo>() || default(TFrom) is null || default(TTo) is null)
                ThrowHelper.ThrowNotSupportedException();
            return ReadUnaligned<TTo>(ref As<TFrom, byte>(ref source));
        }

        /// <summary>
        /// Copies a value of type T to the given location.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void Copy<T>(void* destination, ref readonly T source) => throw new PlatformNotSupportedException();

        /// <summary>
        /// Copies a value of type T to the given location.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void Copy<T>(ref T destination, void* source) => throw new PlatformNotSupportedException();

        /// <summary>
        /// Copies bytes from the source address to the destination address.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CopyBlock(void* destination, void* source, uint byteCount) => throw new PlatformNotSupportedException();

        /// <summary>
        /// Copies bytes from the source address to the destination address.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyBlock(ref byte destination, ref readonly byte source, uint byteCount) => throw new PlatformNotSupportedException();

        /// <summary>
        /// Copies bytes from the source address to the destination address without assuming architecture dependent alignment of the addresses.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void CopyBlockUnaligned(void* destination, void* source, uint byteCount) => throw new PlatformNotSupportedException();

        /// <summary>
        /// Copies bytes from the source address to the destination address without assuming architecture dependent alignment of the addresses.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyBlockUnaligned(ref byte destination, ref readonly byte source, uint byteCount) => throw new PlatformNotSupportedException();

        /// <summary>
        /// Determines whether the memory address referenced by <paramref name="left"/> is greater than
        /// the memory address referenced by <paramref name="right"/>.
        /// </summary>
        /// <remarks>
        /// This check is conceptually similar to "(void*)(&amp;left) &gt; (void*)(&amp;right)".
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAddressGreaterThan<T>([AllowNull] ref readonly T left, [AllowNull] ref readonly T right) => throw new PlatformNotSupportedException();

        /// <summary>
        /// Determines whether the memory address referenced by <paramref name="left"/> is less than
        /// the memory address referenced by <paramref name="right"/>.
        /// </summary>
        /// <remarks>
        /// This check is conceptually similar to "(void*)(&amp;left) &lt; (void*)(&amp;right)".
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAddressLessThan<T>([AllowNull] ref readonly T left, [AllowNull] ref readonly T right) => throw new PlatformNotSupportedException();

        /// <summary>
        /// Initializes a block of memory at the given location with a given initial value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void InitBlock(void* startAddress, byte value, uint byteCount) => throw new PlatformNotSupportedException();

        /// <summary>
        /// Initializes a block of memory at the given location with a given initial value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InitBlock(ref byte startAddress, byte value, uint byteCount) => throw new PlatformNotSupportedException();

        /// <summary>
        /// Initializes a block of memory at the given location with a given initial value
        /// without assuming architecture dependent alignment of the address.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void InitBlockUnaligned(void* startAddress, byte value, uint byteCount) => throw new PlatformNotSupportedException();

        /// <summary>
        /// Initializes a block of memory at the given location with a given initial value
        /// without assuming architecture dependent alignment of the address.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InitBlockUnaligned(ref byte startAddress, byte value, uint byteCount)
        {
            for (var i = 0u; i < byteCount; i++)
                AddByteOffset(ref startAddress, i) = value;
        }

        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> from the given location.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static T ReadUnaligned<T>(void* source)
        {
            typeof(T).ToString();
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> from the given location.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadUnaligned<T>(scoped ref readonly byte source)
        {
            typeof(T).ToString();
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Writes a value of type <typeparamref name="T"/> to the given location.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void WriteUnaligned<T>(void* destination, T value)
        {
            typeof(T).ToString();
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Writes a value of type <typeparamref name="T"/> to the given location.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUnaligned<T>(ref byte destination, T value)
        {
            typeof(T).ToString();
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Adds an byte offset to the given reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddByteOffset<T>(ref T source, nint byteOffset) => throw new PlatformNotSupportedException();

#pragma warning disable CS8500
        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> from the given location.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static T Read<T>(void* source) => *(T*)source;

        /// <summary>
        /// Writes a value of type <typeparamref name="T"/> to the given location.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void Write<T>(void* destination, T value) => *(T*)destination = value;

        /// <summary>
        /// Reinterprets the given location as a reference to a value of type <typeparamref name="T"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static ref T AsRef<T>(void* source) => ref *(T*)source;
#pragma warning restore CS8500

        /// <summary>
        /// Reinterprets the given location as a reference to a value of type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>The lifetime of the reference will not be validated when using this API.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(scoped ref readonly T source) => throw new PlatformNotSupportedException();

        /// <summary>
        /// Determines the byte offset from origin to target from the given references.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint ByteOffset<T>([AllowNull] ref T origin, [AllowNull] ref T target) => throw new PlatformNotSupportedException();

        /// <summary>
        /// Returns a by-ref to type <typeparamref name="T"/> that is a null reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static ref T NullRef<T>() => ref AsRef<T>(null);

        /// <summary>
        /// Returns if a given by-ref to type <typeparamref name="T"/> is a null reference.
        /// </summary>
        /// <remarks>
        /// This check is conceptually similar to "(void*)(&amp;source) == nullptr".
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static bool IsNullRef<T>(ref readonly T source) => AsPointer(ref AsRef(in source)) == null;

        /// <summary>
        /// Bypasses definite assignment rules by taking advantage of <c>out</c> semantics.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SkipInit<T>(out T value) => throw new PlatformNotSupportedException();

        /// <summary>
        /// Subtracts an element offset from the given reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Subtract<T>(ref T source, int elementOffset)
        {
            typeof(T).ToString();
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Subtracts an element offset from the given void pointer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void* Subtract<T>(void* source, int elementOffset)
        {
            typeof(T).ToString();
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Subtracts an element offset from the given reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Subtract<T>(ref T source, nint elementOffset)
        {
            typeof(T).ToString();
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Subtracts an element offset from the given reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Subtract<T>(ref T source, nuint elementOffset)
        {
            typeof(T).ToString();
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Subtracts a byte offset from the given reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T SubtractByteOffset<T>(ref T source, nint byteOffset) => throw new PlatformNotSupportedException();

        /// <summary>
        /// Subtracts a byte offset from the given reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T SubtractByteOffset<T>(ref T source, nuint byteOffset)
        {
            typeof(T).ToString();
            throw new PlatformNotSupportedException();
        }
#pragma warning restore IDE0060

        /// <summary>
        /// Returns a mutable ref to a boxed value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Unbox<T>(object box) where T : struct => throw new PlatformNotSupportedException();
    }
}

namespace System
{
    static class ThrowHelper
    {
        [DoesNotReturn]
        internal static void ThrowNotSupportedException() => throw new NotSupportedException();
    }
}

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
    sealed class NotNullIfNotNullAttribute(string parameterName) : Attribute
    {
        public string ParameterName { get; } = parameterName;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
    sealed class AllowNullAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    sealed class DoesNotReturnAttribute : Attribute { }
}
#pragma warning restore IDE0130