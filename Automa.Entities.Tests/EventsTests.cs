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
            EventManager eventManager = new EventManager();
            EventListener listener = new EventListener();
            eventManager.RegisterListener<Event1>(listener);

            Assert.AreEqual(0, listener.ValueSum);
            eventManager.Raise(new Event1(new Entity(0, 0), 10));
            eventManager.OnUpdate();

            Assert.AreEqual(10, listener.ValueSum);
        }

        [Test]
        public void RaiseMultipleEventsTest()
        {
            EventManager eventManager = new EventManager();
            EventListener listener = new EventListener();
            eventManager.RegisterListener<Event1>(listener);

            eventManager.Raise(new Event1(new Entity(0, 0), 10));
            eventManager.Raise(new Event1(new Entity(0, 0), 20));
            eventManager.OnUpdate();

            Assert.AreEqual(30, listener.ValueSum);
        }

        [Test]
        public void RaiseDifferentEventsTest()
        {
            EventManager eventManager = new EventManager();
            EventListener listener = new EventListener();
            eventManager.RegisterListener<Event1>(listener);
            eventManager.RegisterListener<Event2>(listener);

            eventManager.Raise(new Event1(new Entity(0, 0),10));
            eventManager.Raise(new Event2(new Entity(0, 0), 20));
            eventManager.Raise(new Event3(new Entity(0, 0), 30));
            eventManager.OnUpdate();

            Assert.AreEqual(30, listener.ValueSum);
        }

        [Test]
        public void ClearEventsTest()
        {
            EventManager eventManager = new EventManager();
            EventListener listener = new EventListener();
            eventManager.RegisterListener<Event1>(listener);

            eventManager.Raise(new Event1(new Entity(0, 0), 10));
            eventManager.OnUpdate();

            Assert.AreEqual(10, listener.ValueSum);
            eventManager.OnUpdate();
            Assert.AreEqual(10, listener.ValueSum);
        }

        private class EventListener : IEventListener<Event1>, IEventListener<Event2>
        {
            public int ValueSum;
            
            public void OnEvent(Event1 eventInstance)
            {
                ValueSum += eventInstance.Value;
            }

            public void OnEvent(Event2 eventInstance)
            {
                ValueSum += eventInstance.Value;
            }
        }

        public struct Event1
        {
            public Entity Source;
            public int Value;

            public Event1(Entity source, int value)
            {
                Source = source;
                Value = value;
            }
        }

        public struct Event2
        {
            public Entity Source;
            public int Value;

            public Event2(Entity source, int value)
            {
                Source = source;
                Value = value;
            }
        }

        public struct Event3
        {
            public Entity Source;
            public int Value;

            public Event3(Entity source, int value)
            {
                Source = source;
                Value = value;
            }
        }
    }
}
