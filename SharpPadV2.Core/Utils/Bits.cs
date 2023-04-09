using System.Runtime.CompilerServices;

namespace SharpPadV2.Core.Utils {
    public static class Bits {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int FloatBitsToI32(float value) {
            return *(int*) &value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe uint FloatBitsToU32(float value) {
            return *(uint*) &value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe float I32ToFloatBits(int value) {
            return *(float*) &value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe float I32ToFloatBits(uint value) {
            return *(float*) &value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe long DoubleBitsToI64(double value) {
            return *(long*) &value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ulong DoubleBitsToU64(double value) {
            return *(ulong*) &value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe double I64ToDoubleBits(long value) {
            return *(double*) &value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe double I64ToDoubleBits(ulong value) {
            return *(double*) &value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe double UnsafeToDoubleBits<T>(T value) where T : unmanaged {
            return *(double*) &value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe double UnsafeToFloatsBits<T>(T value) where T : unmanaged {
            return *(float*) &value;
        }
    }
}