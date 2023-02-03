using File.Manager.BusinessLogic.Models.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Services.EventBus
{
    internal class EventBus : IEventBus
    {
        private readonly Dictionary<Type, object> listeners;

        public EventBus()
        {
            listeners = new Dictionary<Type, object>();
        }

        public void Register<T>(IEventListener<T> listener) where T : BaseEvent
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            Type t = typeof(T);

            List<IEventListener<T>> eventListeners;
            if (!listeners.ContainsKey(t))
            {
                eventListeners = new List<IEventListener<T>>();
                listeners[t] = eventListeners;
            }
            else
            {
                eventListeners = listeners[t] as List<IEventListener<T>>;
                if (eventListeners == null)
                    throw new InvalidOperationException("Invalid list type in dictionary!");
            }

            if (!eventListeners.Contains(listener))
                eventListeners.Add(listener);
        }

        public void Unregister<T>(IEventListener<T> listener) where T : BaseEvent
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            Type t = typeof(T);

            if (!listeners.ContainsKey(t))
                return;

            List<IEventListener<T>> eventListeners = listeners[t] as List<IEventListener<T>>;
            eventListeners.Remove(listener);

            if (eventListeners.Count == 0)
                listeners.Remove(t);
        }

        public void Send<T>(T @event) where T : BaseEvent
        {
            Type t = typeof(T);
            if (listeners.ContainsKey(t))
            {
                List<IEventListener<T>> eventListeners = listeners[t] as List<IEventListener<T>>;
                foreach (var listener in eventListeners)
                    listener.Receive(@event);
            }
        }
    }
}
