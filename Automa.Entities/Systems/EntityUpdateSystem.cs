using System;
using System.Collections.Generic;
using System.Linq;
using Automa.Entities.Commands;

namespace Automa.Entities.Systems
{
    public abstract class EntityUpdateSystem : EntitySystem, IUpdateSystem
    {
        private ICommandBuffer[] commandBuffers;

        public override void OnAttachToContext(IContext context)
        {
            base.OnAttachToContext(context);
            List<ICommandBuffer> commandBuffersTmp = new List<ICommandBuffer>();
            foreach (var injectableField in GetInjectableFields()
                .Where(info => typeof(ICommandBuffer).IsAssignableFrom(info.FieldType)))
            {
                var value = (ICommandBuffer)injectableField.GetValue(this);
                if (value == null)
                {
                    var constructor = injectableField.FieldType.GetConstructor(new[] { typeof(EntityManager) });
                    if (constructor == null)
                    {
                        throw new ApplicationException("Can't create command buffer " + injectableField.Name);
                    }
                    value = (ICommandBuffer)constructor.Invoke(new object[] { EntityManager });
                    injectableField.SetValue(this, value);
                }
                commandBuffersTmp.Add(value);
            }
            commandBuffers = commandBuffersTmp.ToArray();
        }

        public void OnUpdate()
        {
            OnBeforeUpdate();
            OnSystemUpdate();
            OnAfterUpdate();
        }

        protected void OnBeforeUpdate()
        {
            for (var index = 0; index < groups.Length; index++)
            {
                var group = groups[index];
                group.Update();
            }
        }

        public abstract void OnSystemUpdate();

        protected void OnAfterUpdate()
        {
            if (commandBuffers.Length == 0) return;
            for (var index = 0; index < commandBuffers.Length; index++)
            {
                var commandBuffer = commandBuffers[index];
                commandBuffer.Execute();
            }
        }
    }
}