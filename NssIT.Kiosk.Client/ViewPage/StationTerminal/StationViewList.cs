using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.StationTerminal
{
    public class StationViewList : INotifyCollectionChanged, IEnumerable
    {
        private List<StationViewRow> _lstItems
          = new List<StationViewRow>();


        public List<StationViewRow> ListItems
        {
            get
            {
                return _lstItems;
            }
        }

        public void Add(StationViewRow item)
        {
            this._lstItems.Add(item);
            this.OnNotifyCollectionChanged(
              new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, item));
        }

        public void Remove(StationViewRow item, int relatedRecordIndex)
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
                StationViewRow itemRemov = this._lstItems[0];
                Remove(itemRemov, 0);
            }
        }

        // ... other actions for the collection ...

        public StationViewRow this[Int32 index]
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
        public List<StationViewRow>.Enumerator GetEnumerator()
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