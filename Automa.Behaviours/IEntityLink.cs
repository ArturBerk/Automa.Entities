using System;

namespace Automa.Behaviours
{
    public interface IEntityLink : IDisposable
    {
        object Entity { get; }
    }

    public interface IEntityLink<out T> : IEntityLink
    {
        new T Entity { get; }
    }
}