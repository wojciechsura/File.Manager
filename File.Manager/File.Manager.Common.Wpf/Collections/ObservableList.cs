using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace File.Manager.Common.Wpf.Collections
{
    public class ObservableList<T> : IList<T>, INotifyCollectionChanged
    {
        // Private fields -----------------------------------------------------

        private readonly List<T> list = new();
        private T[] snapshot = null;
        private object lockObject = new();

        // Private methods ----------------------------------------------------

        private T[] EnsureSnapshot()
        {
            T[] returnSnapshot = snapshot;

            if (snapshot == null)
            {
                lock (lockObject)
                {
                    snapshot ??= list.ToArray();
                    returnSnapshot = snapshot;
                }
            }

            return returnSnapshot;
        }

        private void SetItem(int index, T value)
        {
            T oldValue;

            lock(lockObject)
            {          
                oldValue = list[index];
                list[index] = value;
                snapshot = null;
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldValue, index));
        }

        private T GetItem(int index)
        {
            return EnsureSnapshot()[index];
        }

        // Public methods -----------------------------------------------------

        public void Add(T item)
        {
            int addedIndex;

            lock (lockObject)
            {
                list.Add(item);
                addedIndex = list.Count - 1;
                snapshot = null;
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, addedIndex));
        }

        public void Clear()
        {
            lock (lockObject)
            {
                list.Clear();
                snapshot = null;
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(T item)
        {            
            return EnsureSnapshot().Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            EnsureSnapshot().CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)EnsureSnapshot()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)EnsureSnapshot()).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(EnsureSnapshot(), item);
        }

        public void Insert(int index, T item)
        {
            lock (lockObject)
            {
                list.Insert(index, item);
                snapshot = null;
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public void Move(int fromIndex, int toIndex)
        {
            T item;

            lock (lockObject)
            {
                item = list[fromIndex];
                list.RemoveAt(fromIndex);
                list.Insert(toIndex, item);
                snapshot = null;
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, toIndex, fromIndex));
        }

        public bool Remove(T item)
        {
            int index = -1;

            lock (lockObject)
            {
                index = list.IndexOf(item);
                if (index < 0)
                    return false;

                list.RemoveAt(index);
                snapshot = null;
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            return true;
        }

        public void RemoveAt(int index)
        {
            T item;

            lock (lockObject)
            {
                item = list[index];

                list.RemoveAt(index);
                snapshot = null;
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        public int Count => EnsureSnapshot().Length;

        public T[] Snapshot => EnsureSnapshot();        

        public bool IsReadOnly => ((ICollection<T>)list).IsReadOnly;

        public T this[int index] 
        { 
            get => GetItem(index);
            set => SetItem(index, value); 
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
