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
    public class ObservableList<T> : IList<T>, IList, INotifyCollectionChanged
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

        // IList implementation -----------------------------------------------

        int IList.Add(object value)
        {
            Add((T)value);
            return list.Count - 1;
        }

        void IList.Clear()
        {
            this.Clear();
        }

        bool IList.Contains(object value)
        {
            return this.Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        void IList.Remove(object value)
        {
            Remove((T)value);
        }

        void IList.RemoveAt(int index)
        {
            RemoveAt(index);
        }

        bool IList.IsFixedSize => false;

        bool IList.IsReadOnly => false;

        object IList.this[int index] 
        { 
            get => this[index];
            set => this[index] = (T)value;
        }

        // ICollection implementation -----------------------------------------

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)list).CopyTo(array, index);
        }

        int ICollection.Count => Count;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => null;

        // Public properties --------------------------------------------------

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
