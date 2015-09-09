using System;
using System.Collections;
using System.Collections.Generic;

namespace gep
{
    abstract class TempHistoryItem : IComparable<TempHistoryItem>
    {
        protected HistoryItem hi;
        public HistoryItem HItem { get { return hi; }}

        protected int order;
        public int Order { get{ return order; }}

        public abstract int CalcOrder();

        public static IEnumerable<HistoryItem> hitems(IEnumerable<TempHistoryItem> thitems)
        {
            foreach (TempHistoryItem item in thitems)
                yield return item.HItem;
        }

        public int CompareTo(TempHistoryItem other)
        {
            return order - other.Order;
        }
    }

    class THIAddFilter : TempHistoryItem
    {
        Filter filter;

        public THIAddFilter(HIAddFilter _hi, Filter f)
        {
            filter = f;
            hi = _hi;
        }

        public override int CalcOrder()
        {
            return order = filter.Stage*10000;
        }
    }

    class THIConnect : TempHistoryItem
    {
        PinConnection con;

        public THIConnect(HIConnect _hi, PinConnection _con)
        {
            hi = _hi;
            con = _con;
        }

        public override int CalcOrder()
        {
            return order = con.pins[1].Filter.Stage*10000 + (con.pins[1].Num+1)*100 + con.pins[0].Num+1;
        }
    }
}