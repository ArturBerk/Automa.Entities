using Automa.Benchmarks;

namespace Automa.Entities.PerformanceTests
{
    internal class VirtualCallVsCallBenchmark : Benchmark
    {
        private static readonly int entityCount = 24000;
        private ClassGroup[] classGroup;
        private StructGroup[] structGroup;

        private IUpdatable[] updatableClass;
        private IUpdatable[] updatableStruct;

        protected override void Prepare()
        {
            IterationCount = 10000;

            classGroup = new ClassGroup[entityCount];
            for (var i = entityCount - 1; i >= 0; i--)
            {
                classGroup[i] = new ClassGroup();
            }
            structGroup = new StructGroup[entityCount];
            updatableClass = new IUpdatable[entityCount];
            for (var i = entityCount - 1; i >= 0; i--)
            {
                updatableClass[i] = new ClassGroup();
            }
            updatableStruct = new IUpdatable[entityCount];
            for (var i = entityCount - 1; i >= 0; i--)
            {
                updatableStruct[i] = new StructGroup();
            }
        }

        [Case("Interface call (boxing)")]
        private void InterfaceCall1()
        {
            for (var i = 0; i < entityCount; i++)
            {
                updatableStruct[i].Update();
            }
        }

        [Case("Interface virtual call")]
        private void InterfaceCall()
        {
            for (var i = 0; i < entityCount; i++)
            {
                updatableClass[i].Update();
            }
        }

        [Case("Virtual call")]
        private void VirtualCall()
        {
            for (var i = 0; i < entityCount; i++)
            {
                classGroup[i].Update();
            }
        }

        [Case("Call")]
        private void Call()
        {
            for (var i = 0; i < entityCount; i++)
            {
                structGroup[i].Update();
            }
        }

        public interface IUpdatable
        {
            void Update();
        }

        private struct StructGroup : IUpdatable
        {
            private int t;

            public void Update()
            {
                t = t + 1;
            }
        }

        private class ClassGroup : IUpdatable
        {
            private int t;

            public void Update()
            {
                t = t + 1;
            }
        }
    }
}