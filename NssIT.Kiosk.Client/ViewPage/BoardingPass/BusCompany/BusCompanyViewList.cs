using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.BoardingPass.BusCompany
{
    public class BusCompanyViewList : INotifyCollectionChanged, IEnumerable
    {
        private List<BusCompanyViewRow> _lstItems
          = new List<BusCompanyViewRow>();

        public List<BusCompanyViewRow> Items { get => _lstItems; }

        public void Add(BusCompanyViewRow item)
        {
            this._lstItems.Add(item);
            this.OnNotifyCollectionChanged(
              new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, item));
        }

        public void Remove(BusCompanyViewRow item, int relatedRecordIndex)
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
                BusCompanyViewRow itemRemov = this._lstItems[0];
                Remove(itemRemov, 0);
            }
        }

        // ... other actions for the collection ...

        public BusCompanyViewRow this[Int32 index]
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
        public List<BusCompanyViewRow>.Enumerator GetEnumerator()
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
