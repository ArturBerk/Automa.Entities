using System;
using System.Collections.Generic;
using System.Text;
using Automa.Entities.Collections;
using Automa.Entities.Internal;
using Automa.Entities.Tests.Model;
using NUnit.Framework;

namespace Automa.Entities.Tests
{
    [TestFixture]
    [Category("Groups")]
    public class GroupsTests
    {
        [Test]
        public void IsEntityTypeMatching()
        {
            ComponentType[] types1 = new[]
            {
                ComponentType.Create<ClassComponent>(),
                ComponentType.Create<StructComponent>(),
            };

            var entityType = new EntityType(
                ComponentType.Create<StructComponent>(),
                ComponentType.Create<ClassComponent>());

            var testGroup = new EntityGroup();
            testGroup.includedTypes = types1;
            Assert.IsTrue(testGroup.IsEntityTypeMatching(ref entityType));
        }

        [Test]
        public void IsEntityTypeMatchingExclude()
        {
            ComponentType[] types1 = new[]
            {
                ComponentType.Create<ClassComponent>(),
            };
            ComponentType[] types2 = new[]
            {
                ComponentType.Create<StructComponent>(),
            };

            var entityType = new EntityType(
                ComponentType.Create<StructComponent>(),
                ComponentType.Create<ClassComponent>());

            var testGroup = new EntityGroup();
            testGroup.includedTypes = types1;
            testGroup.excludedTypes = types2;
            Assert.IsFalse(testGroup.IsEntityTypeMatching(ref entityType));
        }

        [Test]
        public void IsEntityTypeMatchingNotMatching()
        {
            ComponentType[] types1 = new[]
            {
                ComponentType.Create<StructComponent>(),
                ComponentType.Create<ClassComponent>()
            };

            var entityType = new EntityType(
                ComponentType.Create<StructComponent>());

            var testGroup = new EntityGroup();
            testGroup.includedTypes = types1;
            Assert.IsFalse(testGroup.IsEntityTypeMatching(ref entityType));
        }

        [Test]
        public void IsEntityTypeMatchingPartlyMatching()
        {
            ComponentType[] types1 = new[]
            {
                ComponentType.Create<StructComponent>()
            };

            var entityType = new EntityType(
                ComponentType.Create<StructComponent>(),
                ComponentType.Create<ClassComponent>());

            var testGroup = new EntityGroup();
            testGroup.includedTypes = types1;
            Assert.IsTrue(testGroup.IsEntityTypeMatching(ref entityType));
        }

        [Test]
        public void GroupSelect()
        {
            var contextEntityManager = new EntityManager();

            var entity = contextEntityManager.CreateEntity(ComponentType.Create<StructComponent>());
            contextEntityManager.AddComponent(entity, new ClassComponent(1));
            contextEntityManager.SetComponent(entity, new StructComponent(2));

            entity = contextEntityManager.CreateEntity(ComponentType.Create<ClassComponent>());
            contextEntityManager.SetComponent(entity, new ClassComponent(3));
            contextEntityManager.AddComponent(entity, new StructComponent(4));

            var group = contextEntityManager.RegisterGroup(new EntityGroup());

            Assert.NotNull(group.Entities);
            Assert.NotNull(group.Classes);
            Assert.NotNull(group.Structures);

            Assert.AreEqual(2, group.Entities.CalculatedCount);
            Assert.AreEqual(0, group.Entities[0].Id);
            Assert.AreEqual(1, group.Entities[1].Id);

            Assert.AreEqual(1, group.Classes[0].Value);
            Assert.AreEqual(2, group.Structures[0].Value);

            Assert.AreEqual(3, group.Classes[1].Value);
            Assert.AreEqual(4, group.Structures[1].Value);
        }

        [Test]
        public void ExcludeComponent()
        {
            var contextEntityManager = new EntityManager();

            var entity = contextEntityManager.CreateEntity(ComponentType.Create<StructComponent>());
            contextEntityManager.AddComponent(entity, new ClassComponent(1));

            var group = contextEntityManager.RegisterGroup(new ExcludeGroup());
            Assert.AreEqual(0, group.Entities.CalculatedCount);
        }

        [Test]
        public void GroupWithLength()
        {
            var contextEntityManager = new EntityManager();

            var entity = contextEntityManager.CreateEntity(ComponentType.Create<StructComponent>());
            contextEntityManager.AddComponent(entity, new ClassComponent(1));

            var group = contextEntityManager.RegisterGroup(new EntityGroup());
            group.Update();
            Assert.AreEqual(1, group.Count);
        }

        [Test]
        public void GroupRemoveComponent()
        {
            var context = ContextFactory.CreateEntitiesContext();
            var contextEntityManager = context.GetManager<EntityManager>();

            var entity = contextEntityManager.CreateEntity(ComponentType.Create<StructComponent>());
            var group = contextEntityManager.RegisterGroup(new ExcludeGroup());

            contextEntityManager.AddComponent(entity, new ClassComponent(1));
            group.Update();
            Assert.AreEqual(0, group.Count);

            contextEntityManager.RemoveComponent<ClassComponent>(entity);
            group.Update();
            Assert.AreEqual(1, group.Count);

            contextEntityManager.AddComponent(entity, new ClassComponent(1));
            group.Update();
            Assert.AreEqual(0, group.Count);
        }

        [Test]
        public void ReactiveAddTest()
        {
            EntityManager entityManager = new EntityManager();
            var group = entityManager.RegisterGroup(new ReactiveAddGroup());
            var e = entityManager.CreateEntityReferenced(
                ComponentType.Create<ClassComponent>(), 
                ComponentType.Create<StructComponent>());
            e.SetComponent(new ClassComponent(10));
            e.SetComponent(new StructComponent(20));
            entityManager.OnUpdate();
            Assert.AreEqual(1, group.added);
        }

        [Test]
        public void ReactiveAddBeforeRegisterTest()
        {
            EntityManager entityManager = new EntityManager();
            var e = entityManager.CreateEntityReferenced(
                ComponentType.Create<ClassComponent>(),
                ComponentType.Create<StructComponent>());
            e.SetComponent(new ClassComponent(10));
            e.SetComponent(new StructComponent(20));
            var group = entityManager.RegisterGroup(new ReactiveAddGroup());
            entityManager.OnUpdate();
            Assert.AreEqual(1, group.added);
        }

        [Test]
        public void ReactiveRemoveTest()
        {
            EntityManager entityManager = new EntityManager();
            var group = entityManager.RegisterGroup(new ReactiveRemoveGroup());
            var e = entityManager.CreateEntityReferenced(
                ComponentType.Create<ClassComponent>(),
                ComponentType.Create<StructComponent>());
            e.SetComponent(new ClassComponent(10));
            e.SetComponent(new StructComponent(20));

            e.Remove();

            Assert.AreEqual(1, group.removed);
        }

        [Test]
        public void ReactiveSwitchDataTypeComponentTest()
        {
            EntityManager entityManager = new EntityManager();
            var group = entityManager.RegisterGroup(new ReactiveRemoveGroup());
            var e = entityManager.CreateEntityReferenced(
                ComponentType.Create<ClassComponent>(),
                ComponentType.Create<StructComponent>());
            e.SetComponent(new ClassComponent(10));
            e.SetComponent(new StructComponent(20));

            e.RemoveComponent<StructComponent>();

            Assert.AreEqual(1, group.removed);
        }

        [Test]
        public void ReactiveSwitchDataTypeComponent2Test()
        {
            EntityManager entityManager = new EntityManager();
            var group = entityManager.RegisterGroup(new ReactiveRemoveGroup());
            var e = entityManager.CreateEntityReferenced(
                ComponentType.Create<ClassComponent>(),
                ComponentType.Create<StructComponent>(),
                ComponentType.Create<Struct2Component>());
            e.SetComponent(new ClassComponent(10));
            e.SetComponent(new StructComponent(20));

            e.RemoveComponent<Struct2Component>();

            Assert.AreEqual(0, group.removed);
        }

        [Test]
        public void ReactiveSwitchDataTypeAddComponentTest()
        {
            EntityManager entityManager = new EntityManager();
            var addGroup = entityManager.RegisterGroup(new ReactiveAddGroup());
            var removeGroup = entityManager.RegisterGroup(new ReactiveRemoveGroup());
            var e = entityManager.CreateEntityReferenced(
                ComponentType.Create<ClassComponent>(),
                ComponentType.Create<StructComponent>());
            e.SetComponent(new ClassComponent(10));
            e.SetComponent(new StructComponent(20));

            e.AddComponent(new Struct2Component(3));

            Assert.AreEqual(0, addGroup.added);
            Assert.AreEqual(0, removeGroup.removed);
        }

        [Test]
        public void ReactiveSwitchDataTypeAddComponent2Test()
        {
            EntityManager entityManager = new EntityManager();
            var addGroup = entityManager.RegisterGroup(new ReactiveAddGroup());
            var removeGroup = entityManager.RegisterGroup(new ReactiveRemoveGroup());
            var e = entityManager.CreateEntityReferenced(
                ComponentType.Create<ClassComponent>());
            e.SetComponent(new ClassComponent(10));
            entityManager.OnUpdate();
            Assert.AreEqual(0, addGroup.added);

            e.AddComponent(new StructComponent(20));
            entityManager.OnUpdate();

            Assert.AreEqual(1, addGroup.added);
        }

        [ExcludeComponent(typeof(ClassComponent))]
        private class ExcludeGroup : Group
        {
            public Collections.EntityCollection Entities;
            public ComponentCollection<StructComponent> Structures;
        }

        private class EntityGroup : Group
        {
            public Collections.EntityCollection Entities;
            public ComponentCollection<ClassComponent> Classes;
            public ComponentCollection<StructComponent> Structures;
        }

        private class ReactiveAddGroup : Group, IEntityAddedListener
        {
            public Collections.EntityCollection Entities;
            public ComponentCollection<ClassComponent> Classes;
            public ComponentCollection<StructComponent> Structures;

            public int added;

            public void OnEntityAdded(Group.EntityIndex index)
            {
                Assert.AreEqual(10, Classes[index].Value);
                Assert.AreEqual(20, Structures[index].Value);
                ++added;
            }
        }

        private class ReactiveRemoveGroup : Group, IEntityRemovingListener
        {
            public Collections.EntityCollection Entities;
            public ComponentCollection<ClassComponent> Classes;
            public ComponentCollection<StructComponent> Structures;

            public int removed;

            public void OnEntityRemoving(EntityIndex index)
            {
                Assert.AreEqual(10, Classes[index].Value);
                Assert.AreEqual(20, Structures[index].Value);
                ++removed;
            }
        }
    }
}
