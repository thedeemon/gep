/*
 * PriorityQueue.cs - A simple binary heap priority queue generic collection.
 * 
 * Copyright � 2007, Jim Mischel.
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace gep
{
    /// <summary>
    /// Represents an item stored in a priority queue.
    /// </summary>
    /// <typeparam name="TValue">The type of object in the queue.</typeparam>
    /// <typeparam name="int">The type of the priority field.</typeparam>
    struct PriorityQueueItem<TValue>
    {
        private TValue value;
        private int priority;

        public PriorityQueueItem(TValue val, int pri)
        {
            this.value = val;
            this.priority = pri;
        }

        /// <summary>
        /// Gets the value of this PriorityQueueItem.
        /// </summary>
        public TValue Value
        {
            get { return value; }
        }

        /// <summary>
        /// Gets the priority associated with this PriorityQueueItem.
        /// </summary>
        public int Priority
        {
            get { return priority; }
        }
    }


    /// <summary>
    /// Represents a binary heap priority queue.
    /// </summary>
    /// <typeparam name="TValue">The type of object in the queue.</typeparam>
    /// <typeparam name="int">The type of the priority field.</typeparam>
    [Serializable]
    [ComVisible(false)]
    class PriorityQueue<TValue> : ICollection,
        IEnumerable<PriorityQueueItem<TValue>>
    {
        private PriorityQueueItem<TValue>[] items;

		private const Int32 DefaultCapacity = 16;
        private Int32 capacity;
        private Int32 numItems;

        //private Comparison<int> compareFunc;

        /// <summary>
        /// Initializes a new instance of the PriorityQueue class that is empty,
        /// has the default initial capacity, and uses the default IComparer.
        /// </summary>
        public PriorityQueue()
            : this(DefaultCapacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PriorityQueue class that is empty,
        /// has the specified initial capacity, and uses the default IComparer.
        /// </summary>
        /// <param name="initialCapacity">Desired initial capacity.</param>
        public PriorityQueue(Int32 initialCapacity)
        {
            Init(initialCapacity);
        }

        // Initializes the queue
        private void Init(int initialCapacity)
        {
            numItems = 0;
            SetCapacity(initialCapacity);
        }

        /// <summary>
        /// Gets the number of items that are currently in the queue.
        /// </summary>
		public int Count
		{
			get { return numItems; }
		}

        /// <summary>
        /// Gets or sets the queue's capacity.
        /// </summary>
		public int Capacity
		{
			get { return items.Length; }
			set { SetCapacity(value); }
		}

        // Set the queue's capacity.
		private void SetCapacity(int newCapacity)
		{
			int newCap = newCapacity;
			if (newCap < DefaultCapacity)
				newCap = DefaultCapacity;

			// throw exception if newCapacity < numItems
			if (newCap < numItems)
				throw new ArgumentOutOfRangeException("newCapacity", "New capacity is less than Count");

            this.capacity = newCap;
			if (items == null)
			{
                // Initial allocation.
                items = new PriorityQueueItem<TValue>[newCap];
				return;
			}

            // Resize the array.
            Array.Resize(ref items, newCap);
		}

        /// <summary>
        /// Adds an object to the queue, in order by priority.
        /// </summary>
        /// <param name="value">The object to be added.</param>
        /// <param name="priority">Priority of the object to be added.</param>
        public void Enqueue(TValue value, int priority)
        {
	        if (numItems == capacity)
	        {
		        // need to increase capacity
		        // grow by 50 percent
		        SetCapacity((3*Capacity)/2);
	        }

            // Create the new item
            PriorityQueueItem<TValue> newItem = 
                new PriorityQueueItem<TValue>(value, priority);
            int i = numItems;
	        ++numItems;

            // and insert it into the heap.
	        while ((i > 0) && (items[i/2].Priority > newItem.Priority))
	        {
		        items[i] = items[i/2];
		        i /= 2;
	        }
	        items[i] = newItem;
        }

        // Remove a node at a particular position in the queue.
		private PriorityQueueItem<TValue> RemoveAt (Int32 index)
		{
            // remove an item from the heap
			PriorityQueueItem<TValue> o = items[index];
			PriorityQueueItem<TValue> tmp = items[numItems-1];
            items[--numItems] = default(PriorityQueueItem<TValue>) ;
			if (numItems > 0)
			{
				int i = index;
				int j = i+1;
				while (i < Count/2)
				{
					if ((j < Count-1) && (items[j].Priority > items[j+1].Priority))
					{
						j++;
					}
					if (items[j].Priority >= tmp.Priority)
					{
						break;
					}
					items[i] = items[j];
					i = j;
					j *= 2;
				}
				items[i] = tmp;
			}
			return o;
		}

        /// <summary>
        /// Removes and returns the item with the highest priority from the queue.
        /// </summary>
        /// <returns>The object that is removed from the beginning of the queue.</returns>
		public PriorityQueueItem<TValue> Dequeue()
		{
			if (Count == 0)
				throw new InvalidOperationException("The queue is empty");
			return RemoveAt(0);
		}

        /// <summary>
        /// Removes the item with the specified value from the queue.
        /// The passed equality comparison is used.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        /// <param name="comp">An object that implements the IEqualityComparer interface
        /// for the type of item in the collection.</param>
        public void Remove(TValue item, IEqualityComparer comparer)
        {
            // need to find the PriorityQueueItem that has the Data value of o
            for (int index = 0; index < numItems; ++index)
            {
                if (comparer.Equals(item, items[index].Value))
                {
                    RemoveAt(index);
                    return;
                }
            }
            throw new ApplicationException("The specified itemm is not in the queue.");
        }

        /// <summary>
        /// Removes the item with the specified value from the queue.
        /// The default type comparison function is used.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
		public void Remove(TValue item)
		{
            Remove(item, EqualityComparer<TValue>.Default);
		}

        /// <summary>
        /// Returns the object at the beginning of the priority queue without removing it.
        /// </summary>
        /// <returns>The object at the beginning of the queue.</returns>
		public PriorityQueueItem<TValue> Peek()
		{
			if (Count == 0)
				throw new InvalidOperationException("The queue is empty");
			return items[0];
		}

		/// <summary>
		/// Removes all objects from the queue.
		/// </summary>
		public void Clear()
		{
			numItems = 0;
			TrimExcess();
		}

        /// <summary>
        /// Sets the capacity to the actual number of elements in the Queue,
        /// if that number is less than 90 percent of current capacity. 
        /// </summary>
        public void TrimExcess()
        {
            if (numItems < (0.9 * capacity))
                SetCapacity(numItems);
        }

		/// <summary>
		/// Determines whether an element is in the queue.
		/// </summary>
		/// <param name="o">The object to locate in the queue.</param>
		/// <returns>True if item found in the queue.  False otherwise.</returns>
		public bool Contains(TValue item)
		{
			foreach (PriorityQueueItem<TValue> qItem in items)
			{
				if (qItem.Value.Equals(item))
					return true;
			}
			return false;
		}

        /// <summary>
        /// Copies the elements of the ICollection to an Array, starting at a particular Array index. 
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from ICollection.
        /// The Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(PriorityQueueItem<TValue>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex", "arrayIndex is less than 0.");
            if (array.Rank > 1)
                throw new ArgumentException("array is multidimensional.");
            if (arrayIndex >= array.Length)
                throw new ArgumentException("arrayIndex is equal to or greater than the length of the array.");
            if (numItems > (array.Length - arrayIndex))
                throw new ArgumentException("The number of elements in the source ICollection is greater than the available space from arrayIndex to the end of the destination array.");

            Array.Copy(items, 0, array, arrayIndex, numItems);
        }

        /// <summary>
        /// Copies the queue elements to a new array. 
        /// </summary>
        /// <returns>A new array containing elements copied from the Queue.</returns>
        public PriorityQueueItem<TValue>[] ToArray()
        {
            PriorityQueueItem<TValue>[] newItems = 
                new PriorityQueueItem<TValue>[numItems];
            Array.Copy(items, newItems, numItems);
            return newItems;
        }

        #region ICollection Members

        /// <summary>
        /// Copies the elements of the ICollection to an Array, starting at a particular Array index. 
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from ICollection.
        /// The Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        void ICollection.CopyTo(Array array, int index)
        {
            this.CopyTo((PriorityQueueItem<TValue>[])array, index);
        }

        /// <summary>
        /// Gets a value indicating whether access to the ICollection is synchronized (thread safe). 
        /// </summary>
        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the ICollection. 
        /// </summary>
        object ICollection.SyncRoot
        {
            get { return items.SyncRoot; }
        }

        #endregion

        #region IEnumerable<PriorityQueueItem<TValue,int>> Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection. 
        /// </summary>
        /// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<PriorityQueueItem<TValue>> GetEnumerator()
        {
            for (int i = 0; i < numItems; i++)
            {
                yield return items[i];
            }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
