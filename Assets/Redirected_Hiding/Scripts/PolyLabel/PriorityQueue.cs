// taken from: http://www.blackwasp.co.uk/PriorityQueue.aspx
// license: http://www.blackwasp.co.uk/Copyright.aspx
// as of 21.11.2022:
/*Free-of-Charge Sample Source Code

The source code that is provided on a free-of-charge basis to support articles is Copyright ©2006-2022 BlackWasp. This includes the source code embedded within the text of an article and the source code provided via the download link provided within an article. You may use the source code as part of any personal, open source or commercial project provided that:

    you do not provide the source code as part of an article or other post on a web site.
    you do not use the source code for any unlawful purpose.
    you indemnify and hold harmless BlackWasp and its successors and associates from any and all liability from any use of the source code.
    you ensure that these conditions apply to any further redistribution of your software.
    you agree to, and abide by, the terms of use (http://www.blackwasp.co.uk/TermsOfUse.aspx).
    the specific download does not include a different license. In such cases, the downloaded license will apply.
*/

using System;
using System.Collections.Generic;
using System.Linq;

using System.Collections;


namespace PriorityQueue
{
    public class PriorityQueue<TPriority, TItem>
    {
        readonly SortedDictionary<TPriority, Queue<TItem>> _subqueues;

        public PriorityQueue(IComparer<TPriority> priorityComparer)
        {
            _subqueues = new SortedDictionary<TPriority, Queue<TItem>>(priorityComparer);
        }

        public PriorityQueue() : this(Comparer<TPriority>.Default) { }

        public void Enqueue(TPriority priority, TItem item)
        {
            if (!_subqueues.ContainsKey(priority))
            {
                AddQueueOfPriority(priority);
            }

            _subqueues[priority].Enqueue(item);
        }

        private void AddQueueOfPriority(TPriority priority)
        {
            _subqueues.Add(priority, new Queue<TItem>());
        }

        public TItem Peek()
        {
            if (HasItems)
                return _subqueues.First().Value.Peek();
            else
                throw new InvalidOperationException("The queue is empty");
        }

        public bool HasItems
        {
            get { return _subqueues.Any(); }
        }

        public TItem Dequeue()
        {
            if (_subqueues.Any())
                return DequeueFromHighPriorityQueue();
            else
                throw new InvalidOperationException("The queue is empty");
        }

        private TItem DequeueFromHighPriorityQueue()
        {
            KeyValuePair<TPriority, Queue<TItem>> first = _subqueues.First();
            TItem nextItem = first.Value.Dequeue();
            if (!first.Value.Any())
            {
                _subqueues.Remove(first.Key);
            }
            return nextItem;
        }

        public int Count
        {
            get { return _subqueues.Sum(q => q.Value.Count); }
        }
    }
}
