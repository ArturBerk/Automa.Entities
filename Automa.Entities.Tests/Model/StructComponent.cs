namespace Automa.Entities.Tests.Model
{
    public struct StructComponent : IComponent
    {
        public int Value;

        public StructComponent(int value)
        {
            Value = value;
        }

    }

    public struct Struct2Component : IComponent
    {
        public int Value;

        public Struct2Component(int value)
        {
            Value = value;
        }

    }

    public struct Struct3Component : IComponent
    {
        public int Value;

        public Struct3Component(int value)
        {
            Value = value;
        }

    }
}