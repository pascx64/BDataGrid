using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BDataGrid.Library
{

    public class BDataGridItem<TItem>
        where TItem : class
    {
        public BDataGridItem()
        {
            Children = new BDataGridItemChildrenCollection<TItem>((TItem)(object)this);
        }

        public TItem? Parent { get; set; }

        public BDataGridItemChildrenCollection<TItem> Children { get; }
    }
    public class BDataGridItemChildrenCollection<TItem> : ICollection<BDataGridItem<TItem>>
        where TItem : class
    {
        private TItem CurrentRow { get; }

        internal BDataGridItemChildrenCollection(TItem row)
        {
            CurrentRow = row;
        }

        private HashSet<BDataGridItem<TItem>> InternalCollection = new HashSet<BDataGridItem<TItem>>();

        public int Count => InternalCollection.Count;

        public bool IsReadOnly => false;

        public void Add(BDataGridItem<TItem> item)
        {
            item.Parent = CurrentRow;
            InternalCollection.Add(item);
        }

        public void Clear()
        {
            foreach (var item in InternalCollection)
                item.Parent = null;
            InternalCollection.Clear();
        }

        public bool Contains(BDataGridItem<TItem> item)
        {
            return InternalCollection.Contains(item);
        }

        public void CopyTo(BDataGridItem<TItem>[] array, int arrayIndex)
        {
            InternalCollection.CopyTo(array, arrayIndex);
        }

        public IEnumerator<BDataGridItem<TItem>> GetEnumerator()
        {
            return InternalCollection.GetEnumerator();
        }

        public bool Remove(BDataGridItem<TItem> item)
        {
            if (InternalCollection.Remove(item))
            {
                item.Parent = null;
                return true;
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return InternalCollection.GetEnumerator();
        }
    }
}
