using System;
using System.Diagnostics;
using System.Collections.Generic;


namespace viadflib.TravelTime
{
    public class SimplePriorityQueue<T>
    {
        int total_size;
        SortedDictionary<double, Queue<T>> storage;

        public SimplePriorityQueue()
        {
            this.storage = new SortedDictionary<double, Queue<T>>();
            this.total_size = 0;
        }

        public bool IsEmpty()
        {
            return (total_size == 0);
        }

        public T Dequeue()
        {
            if (IsEmpty())
            {
                throw new Exception("Please check that priorityQueue is not empty before dequeing");
            }

            foreach (Queue<T> q in storage.Values)
            {
                // we use a sorted dictionary
                if (q.Count > 0)
                {
                    total_size--;
                    return q.Dequeue();
                }
            }

            Debug.Assert(false, "not supposed to reach here. problem with changing total_size");

            return default(T); // not supposed to reach here.
        }

        // same as above, except for peek.

        public T Peek()
        {
            if (IsEmpty())
                throw new Exception("Please check that priorityQueue is not empty before dequeing");

            foreach (Queue<T> q in storage.Values)
            {
                if (q.Count > 0)
                    return q.Peek();
            }

            Debug.Assert(false, "not supposed to reach here. problem with changing total_size");

            return default(T); // not supposed to reach here.
        }

        public int Count {
            get { return total_size; }
        }

        public void Remove(T item, double prio)
        {
            var queue = storage[prio];
            int queueCount = queue.Count;

            if (queueCount == 1)
            {
                total_size--;
            }
            else
            {
                Queue<T> newQueue = new Queue<T>();
                for (int i = 0; i < queueCount; i++)
                {
                    var element = queue.Dequeue();
                    if (!element.Equals(item))
                    {
                        newQueue.Enqueue(element);
                    }
                    else
                    {
                        total_size--;
                    }
                }
                storage[prio] = newQueue;
            }
        }

        public void Enqueue(T item, double prio)
        {
            if (!storage.ContainsKey(prio))
            {
                storage.Add(prio, new Queue<T>());
                Enqueue(item, prio);
                // run again

            }
            else
            {
                storage[prio].Enqueue(item);
                total_size++;
            }
        }
    }
}
