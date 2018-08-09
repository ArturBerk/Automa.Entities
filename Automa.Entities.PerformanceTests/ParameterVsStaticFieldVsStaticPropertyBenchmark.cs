using Automa.Benchmarks;

namespace Automa.Entities.PerformanceTests
{
    internal class ParameterVsStaticFieldVsStaticPropertyBenchmark : Benchmark
    {
        private static readonly int entityCount = 24000;

        private UpdatableField[] updatableField;
        private UpdatableProperty[] updatableProperty;
        private UpdatableParameter[] updatableParameter;

        protected override void Prepare()
        {
            IterationCount = 10000;

            updatableField = new UpdatableField[entityCount];
            updatableProperty = new UpdatableProperty[entityCount];
            updatableParameter = new UpdatableParameter[entityCount];
        }

        [Case("Parameter")]
        private void InterfaceCall1()
        {
            for (var i = 0; i < entityCount; i++)
            {
                updatableParameter[i].Update(1.0f);
            }
        }

        [Case("Static Field")]
        private void StaticField()
        {
            Time.dt = 1.0f;
            for (var i = 0; i < entityCount; i++)
            {
                updatableField[i].Update();
            }
        }

        [Case("Static Property")]
        private void StaticProperty()
        {
            Time.Dt = 1.0f;
            for (var i = 0; i < entityCount; i++)
            {
                updatableProperty[i].Update();
            }
        }

        public static class Time
        {
            public static float dt;
            public static float Dt { get; set; }
        }

        public interface IUpdatable
        {
            void Update();
        }

        public interface IUpdatableParameter
        {
            void Update(float dt);
        }

        private struct UpdatableField : IUpdatable
        {
            private float t;

            public void Update()
            {
                t = Time.dt;
            }
        }

        private struct UpdatableProperty : IUpdatable
        {
            private float t;

            public void Update()
            {
                t = Time.Dt;
            }
        }

        private struct UpdatableParameter : IUpdatableParameter
        {
            private float t;

            public void Update(float dt)
            {
                t = dt;
            }
        }
    }
}