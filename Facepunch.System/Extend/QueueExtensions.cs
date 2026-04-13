using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Facepunch.Extend
{
    public static class QueueEx
    {
        public static void EnqueueRange<T>( this Queue<T> queue, IEnumerable<T> items)
        {
            foreach(var item in items)
            {
                queue.Enqueue(item);
            }
        }
    }
}
