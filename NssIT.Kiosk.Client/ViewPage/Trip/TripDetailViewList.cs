using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Trip
{
    public class TripDetailViewList : INotifyCollectionChanged, IEnumerable
    {
        private List<TripDetailViewRow> _lstItems
          = new List<TripDetailViewRow>();

        public List<TripDetailViewRow> Items { get => _lstItems;  }

        public void Add(TripDetailViewRow item)
        {
            this._lstItems.Add(item);
            this.OnNotifyCollectionChanged(
              new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, item));
        }

        public void Remove(TripDetailViewRow item, int relatedRecordIndex)
        {
            this._lstItems.Remove(item);
            this.OnNotifyCollectionChanged(
              new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove, item, relatedRecordIndex));
        }

        public void Clear()
        {
            while (this._lstItems.Count > 0)
            {
                TripDetailViewRow itemRemov = this._lstItems[0];
                Remove(itemRemov, 0);
            }
        }

        // ... other actions for the collection ...

        public TripDetailViewRow this[Int32 index]
        {
            get
            {
                return this._lstItems[index];
            }
        }

        #region INotifyCollectionChanged
        private void OnNotifyCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this, args);
            }
        }
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion INotifyCollectionChanged

        #region IEnumerable
        public List<TripDetailViewRow>.Enumerator GetEnumerator()
        {
            return this._lstItems.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)this.GetEnumerator();
        }
        #endregion IEnumerable
    }
}
