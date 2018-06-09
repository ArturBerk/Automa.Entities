using Automa.Entities.Events;
using NUnit.Framework;

namespace Automa.Entities.Tests
{
    [TestFixture]
    [Category("Events")]
    public class EventsTests
    {

        [Test]
        public void RaiseTest()
        {
            EventManager<Entity> eventManager = new EventManager<Entity>();
            EventListener listener = new EventListener();
            eventManager.RegisterListener<Event1>(listener);

            Assert.AreEqual(0, listener.ValueSum);
            eventManager.Raise(new Entity(0,0), new Event1(10));
            eventManager.OnUpdate();

            Assert.AreEqual(10, listener.ValueSum);
        }

        [Test]
        public void RaiseMultipleEventsTest()
        {
            EventManager<Entity> eventManager = new EventManager<Entity>();
            EventListener listener = new EventListener();
            eventManager.RegisterListener<Event1>(listener);

            eventManager.Raise(new Entity(0, 0), new Event1(10));
            eventManager.Raise(new Entity(0, 0), new Event1(20));
            eventManager.OnUpdate();

            Assert.AreEqual(30, listener.ValueSum);
        }

        [Test]
        public void RaiseDifferentEventsTest()
        {
            EventManager<Entity> eventManager = new EventManager<Entity>();
            EventListener listener = new EventListener();
            eventManager.RegisterListener<Event1>(listener);
            eventManager.RegisterListener<Event2>(listener);

            eventManager.Raise(new Entity(0, 0), new Event1(10));
            eventManager.Raise(new Entity(0, 0), new Event2(20));
            eventManager.Raise(new Entity(0, 0), new Event3(30));
            eventManager.OnUpdate();

            Assert.AreEqual(30, listener.ValueSum);
        }

        [Test]
        public void ClearEventsTest()
        {
            EventManager<Entity> eventManager = new EventManager<Entity>();
            EventListener listener = new EventListener();
            eventManager.RegisterListener<Event1>(listener);

            eventManager.Raise(new Entity(0, 0), new Event1(10));
            eventManager.OnUpdate();

            Assert.AreEqual(10, listener.ValueSum);
            eventManager.OnUpdate();
            Assert.AreEqual(10, listener.ValueSum);
        }

        private class EventListener : IEventListener<Entity, Event1>, IEventListener<Entity, Event2>
        {
            public int ValueSum;
            
            public void OnEvent(Entity source, Event1 eventInstance)
            {
                ValueSum += eventInstance.Value;
            }

            public void OnEvent(Entity source, Event2 eventInstance)
            {
                ValueSum += eventInstance.Value;
            }
        }

        public struct Event1
        {
            public int Value;

            public Event1(int value)
            {
                Value = value;
            }
        }

        public struct Event2
        {
            public int Value;

            public Event2(int value)
            {
                Value = value;
            }
        }

        public struct Event3
        {
            public int Value;

            public Event3(int value)
            {
                Value = value;
            }
        }
    }
}
