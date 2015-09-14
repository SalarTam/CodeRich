using System;
using System.Threading;

namespace Wintellect.Threading
{
    public static class InterlockedEx
    {
        public static bool IfThen(ref int val, int @if, int then)
        {
            return Interlocked.CompareExchange(ref val, then, @if) == @if;
        }

        public static bool IfThen(ref int val, int @if, int then, out int prevVal)
        {
            prevVal = Interlocked.CompareExchange(ref val, then, @if);
            return prevVal == @if;
        }

        public static int And(ref int target, int with)
        {
            int num = target;
            int num2;
            do
            {
                num2 = num;
                num = Interlocked.CompareExchange(ref target, num2 & with, num2);
            }
            while (num2 != num);
            return num;
        }

        public static int Or(ref int target, int with)
        {
            int num = target;
            int num2;
            do
            {
                num2 = num;
                num = Interlocked.CompareExchange(ref target, num2 | with, num2);
            }
            while (num2 != num);
            return num;
        }

        public static int Xor(ref int target, int with)
        {
            int num = target;
            int num2;
            do
            {
                num2 = num;
                num = Interlocked.CompareExchange(ref target, num2 ^ with, num2);
            }
            while (num2 != num);
            return num;
        }

        public static int MaskedAnd(ref int target, int with, int mask)
        {
            int num = target;
            int num2;
            do
            {
                num2 = (num & mask);
                num = Interlocked.CompareExchange(ref target, num2 & with, num2);
            }
            while (num2 != num);
            return num;
        }

        public static int MaskedOr(ref int target, int with, int mask)
        {
            int num = target;
            int num2;
            do
            {
                num2 = (num & mask);
                num = Interlocked.CompareExchange(ref target, num2 | with, num2);
            }
            while (num2 != num);
            return num;
        }

        public static int MaskedXor(ref int target, int with, int mask)
        {
            int num = target;
            int num2;
            do
            {
                num2 = (num & mask);
                num = Interlocked.CompareExchange(ref target, num2 ^ with, num2);
            }
            while (num2 != num);
            return num;
        }

        public static int MaskedExchange(ref int target, int mask, int value)
        {
            int num = target;
            int num2;
            do
            {
                num2 = num;
                num = Interlocked.CompareExchange(ref target, (num2 & ~mask) | value, num);
            }
            while (num2 != num);
            return num;
        }

        public static bool BitTestAndSet(ref int target, int bitNum)
        {
            int num = 1 << bitNum;
            return (InterlockedEx.Or(ref target, num) & num) != 0;
        }

        public static bool BitTestAndReset(ref int target, int bitNum)
        {
            int num = 1 << bitNum;
            return (InterlockedEx.And(ref target, ~num) & num) != 0;
        }

        public static bool BitTestAndCompliment(ref int target, int bitNum)
        {
            int num = 1 << bitNum;
            return (InterlockedEx.Xor(ref target, num) & num) != 0;
        }

        public static int MaskedAdd(ref int target, int value, int mask)
        {
            int num = target;
            int num2;
            do
            {
                num2 = (num & mask);
                num = Interlocked.CompareExchange(ref target, num2 + value, num2);
            }
            while (num2 != num);
            return num;
        }

        public static int Max(ref int target, int val)
        {
            int num = target;
            int num2;
            do
            {
                num2 = num;
                num = Interlocked.CompareExchange(ref target, Math.Max(num2, val), num2);
            }
            while (num2 != num);
            return num;
        }

        public static int Min(ref int target, int val)
        {
            int num = target;
            int num2;
            do
            {
                num2 = num;
                num = Interlocked.CompareExchange(ref target, Math.Min(num2, val), num2);
            }
            while (num2 != num);
            return num;
        }
    }
}