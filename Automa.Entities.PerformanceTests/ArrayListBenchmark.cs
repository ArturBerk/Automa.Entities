using Automa.Benchmarks;
using Automa.Common;
using Automa.Entities.Internal;
using Automa.Entities.PerformanceTests.Model;

namespace Automa.Entities.PerformanceTests
{
    class ArrayListBenchmark : Benchmark
    {
        private struct StructTest
        {
            public StructComponent s1;
            public StructComponent s2;
            public StructComponent s3;
            public StructComponent s4;
            public StructComponent s5;
        }

        private ArrayList<StructTest> test = new ArrayList<StructTest>(4);

        protected override void Prepare()
        {
            IterationCount = 1000;
            for (int i = 0; i < 12000; i++)
            {
                test.Add(new StructTest());
            }
        }

        [Case("Direct buffer ref")]
        private void TestBufferRef()
        {
            for (int i = 0; i < test.Count; i++)
            {
                ref var t = ref test.Buffer[i];
                t.s1 = new StructComponent();
            }
        }

        [Case("Direct buffer")]
        private void TestBuffer()
        {
            for (int i = 0; i < test.Count; i++)
            {
                var t = test.Buffer[i];
                t.s1 = new StructComponent();
                test.Buffer[i] = t;
            }
        }

//        [Case("Getter ref")]
//        private void TestGetterRef()
//        {
//            for (int i = 0; i < test.Count; i++)
//            {
//                ref var t = ref test[i];
//                t.s1 = new StructComponent();
//            }
//        }
//
//        [Case("Getter")]
//        private void TestGetter()
//        {
//            for (int i = 0; i < test.Count; i++)
//            {
//                var t = test[i];
//                t.s1 = new StructComponent();
//                test[i] = t;
//            }
//        }
    }
}
