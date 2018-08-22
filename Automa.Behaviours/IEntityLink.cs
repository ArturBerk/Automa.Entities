using System;

namespace Automa.Behaviours
{
    public interface IEntityLink : IDisposable
    {
        object Entity { get; }
    }

    public interface IEntityLink<T> : IEntityLink
    {
        new ref T Entity { get; }
    }
}