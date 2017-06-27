
using System;
using System.Linq;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;


namespace viadflib.TravelTime
{

    public class PriorityQueue<T>
    {
        int total_size;
        SortedDictionary<double, Queue<T>> storage;

        public PriorityQueue()
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
            foreach (var keyValuePair in storage)
            {
                // we use a sorted dictionary
                if (keyValuePair.Value.Count > 0)
                {
                    total_size--;

                    T value = keyValuePair.Value.Dequeue();

                    // if empty remove the queue from storage
                    if (keyValuePair.Value.Count == 0)
                    {
                        storage.Remove(keyValuePair.Key);
                    }

                    return value;
                }
            }

            throw new Exception("Please check that priorityQueue is not empty before dequeing");
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

            throw new Exception("Please check that priorityQueue is not empty before dequeing");
        }

        public int Count
        {
            get { return total_size; }
        }

        public void Remove(T item, double prioKey)
        {
            var queue = storage[prioKey];
            int queueCount = queue.Count;

            if (queueCount == 1)
            {
                total_size--;
                storage.Remove(prioKey);
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
                storage[prioKey] = newQueue;
            }
        }

        public void Enqueue(T item, double prio)
        {
            if (!storage.ContainsKey(prio))
            {
                var queue = new Queue<T>();
                storage.Add(prio, queue);
                queue.Enqueue(item);
            }
            else
            {
                storage[prio].Enqueue(item);
            }
            total_size++;
        }
    }
}
