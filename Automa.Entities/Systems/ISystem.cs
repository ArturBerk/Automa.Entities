using System;

namespace Automa.Entities.Systems
{
    public interface ISystem
    {
        bool IsEnabled { get; set; }
        event Action<ISystem, bool> EnabledChanged;

        void OnAttachToContext(IContext context);
        void OnDetachFromContext(IContext context);
    }

    public interface IUpdateSystem
    {
        void OnUpdate();
    }
}