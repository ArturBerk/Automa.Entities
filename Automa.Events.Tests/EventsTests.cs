using NUnit.Framework;

namespace Automa.Events.Tests
{
    [TestFixture]
    [Category("Events")]
    public class EventsTests
    {
        [Test]
        public void RaiseTest()
        {
            EventDispatcher eventManager = new EventDispatcher();
            EventListener listener = new EventListener();
            eventManager.RegisterListener<Event1>(listener);

            Assert.AreEqual(0, listener.ValueSum);
            eventManager.Raise(new Event1(new Entity(0, 0), 10));
            eventManager.Dispatch();

            Assert.AreEqual(10, listener.ValueSum);
        }

        [Test]
        public void RaiseMultipleEventsTest()
        {
            EventDispatcher eventManager = new EventDispatcher();
            EventListener listener = new EventListener();
            eventManager.RegisterListener<Event1>(listener);

            eventManager.Raise(new Event1(new Entity(0, 0), 10));
            eventManager.Raise(new Event1(new Entity(0, 0), 20));
            eventManager.Dispatch();

            Assert.AreEqual(30, listener.ValueSum);
        }

        [Test]
        public void RaiseDifferentEventsTest()
        {
            EventDispatcher eventManager = new EventDispatcher();
            EventListener listener = new EventListener();
            eventManager.RegisterListener<Event1>(listener);
            eventManager.RegisterListener<Event2>(listener);

            eventManager.Raise(new Event1(new Entity(0, 0),10));
            eventManager.Raise(new Event2(new Entity(0, 0), 20));
            eventManager.Raise(new Event3(new Entity(0, 0), 30));
            eventManager.Dispatch();

            Assert.AreEqual(30, listener.ValueSum);
        }

        [Test]
        public void ClearEventsTest()
        {
            EventDispatcher eventManager = new EventDispatcher();
            EventListener listener = new EventListener();
            eventManager.RegisterListener<Event1>(listener);

            eventManager.Raise(new Event1(new Entity(0, 0), 10));
            eventManager.Dispatch();

            Assert.AreEqual(10, listener.ValueSum);
            eventManager.Dispatch();
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

        public struct Entity
        {
            public int Id;
            public int Version;

            public Entity(int id, int version)
            {
                Id = id;
                Version = version;
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
