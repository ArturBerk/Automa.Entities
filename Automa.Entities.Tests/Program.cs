using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Automa.Entities.Tests
{
    class Program
    {
        public static void Main1()
        {
            var count = 10000000;
            TestArray array = new TestArray(count);

            for (int i = 0; i < count; i++)
            {
                var value = array.Get(i);
                value.i1 += 10;
                value.i2 += 10;
                value.i3 += 10;
                value.i4 += 10;
                value.i5 += 10;
                value.i6 += 10;
                value.i7 += 10;
                value.i8 += 10;
                array.Set(i, value);
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < count; i++)
            {
                ref var value = ref array.GetRef(i);
                value.i1 += 10;
//                value.i2 += 10;
//                value.i3 += 10;
//                value.i4 += 10;
//                value.i5 += 10;
//                value.i6 += 10;
//                value.i7 += 10;
//                value.i8 += 10;
            }
            stopwatch.Stop();
            Console.WriteLine($"Ref: {stopwatch.ElapsedTicks}");
            
            stopwatch.Restart();
            for (int i = 0; i < count; i++)
            {
                var value = array.Get(i);
                value.i1 += 10;
//                value.i2 += 10;
//                value.i3 += 10;
//                value.i4 += 10;
//                value.i5 += 10;
//                value.i6 += 10;
//                value.i7 += 10;
//                value.i8 += 10;
                array.Set(i, value);
            }
            stopwatch.Stop();
            Console.WriteLine($"Get/Set: {stopwatch.ElapsedTicks}");

            stopwatch.Restart();
            TestStruct[] arrayRaw = array.Array;
            for (int i = 0; i < count; i++)
            {
                var value = arrayRaw[i];
                value.i1 += 10;
//                value.i2 += 10;
//                value.i3 += 10;
//                value.i4 += 10;
//                value.i5 += 10;
//                value.i6 += 10;
//                value.i7 += 10;
//                value.i8 += 10;
                arrayRaw[i] = value;
            }
            stopwatch.Stop();
            Console.WriteLine($"Raw access: {stopwatch.ElapsedTicks}");

            Console.ReadKey();
        }

        private class TestArray
        {
            private readonly TestStruct[] array;

            public TestStruct[] Array => array;

            public ref TestStruct GetRef(int index)
            {
                return ref array[index];
            }

            public TestStruct Get(int index)
            {
                return array[index];
            }

            public void Set(int index, TestStruct value)
            {
                array[index] = value;
            }

            public TestArray(int count)
            {
                array = new TestStruct[count];
            }
        }

        private struct TestStruct
        {
            public int i1;
            public int i2;
            public int i3;
            public int i4;
            public int i5;
            public int i6;
            public int i7;
            public int i8;
        }
    }
}
