using System;
using System.Collections;
using WPFMediaKit.DirectShow.Interop;

namespace WPFMediaKit.DirectShow.MediaPlayers
{
    public class FilterControl
    {
        private IFilterGraph filterGraph;

        public ArrayList Filters { get; set; }
        public int Count
        {
            get
            {
                return Filters.Count;
            }
        }

        public FilterControl(IFilterGraph fg)
        {
            filterGraph = fg;
            Filters = new ArrayList { };

            getFilters();
        }

        public void Refresh()
        {
            // Refresh each of the streams information.
            foreach (Filter f in Filters)
                f.Refresh();
        }

        private void getFilters()
        {
            IEnumFilters enumFilters;
            filterGraph.EnumFilters(out enumFilters);

            IBaseFilter[] filters = new IBaseFilter[1];
            IntPtr fetched = IntPtr.Zero;

            while (enumFilters.Next(1, filters, fetched) == 0)
                Filters.Add(new Filter(filters[0]));
        }

        public Filter getFilter(string filterName)    {
            foreach (Filter f in Filters)
                if (f.GetFilterInfo.achName == filterName)
                    return f;
            return null;
        }
    }

    public class Filter
    {
        public IBaseFilter BaseFilter;
        public ArrayList Streams;
        public int Count;
        public FilterInfo GetFilterInfo;

        public Filter(IBaseFilter filter)
        {
            BaseFilter = filter;
            BaseFilter.QueryFilterInfo(out GetFilterInfo);
            Streams = new ArrayList { };

            getStreams();
        }

        private void getStreams()
        {
            IAMStreamSelect streamSelect = BaseFilter as IAMStreamSelect;

            if (streamSelect != null)
            {
                streamSelect.Count(out Count);

                for (int i = 0; i < Count; ++i)
                    Streams.Add(new FilterStream(i, streamSelect));
            }
            else
            {
                Count = 0;
            }
        }

        public void Refresh()
        {
            foreach (FilterStream s in Streams)
                s.Refresh(true);
        }
    }

    public class FilterStream {

        private IAMStreamSelect SS;
        
        public int ID;
        public Boolean Enabled { get; set; }
        public String Name { get; set; }
        public int GroupID { get; set; }

        public FilterStream(int id, IAMStreamSelect streams)
        {
            SS = streams;
            ID = id;

            Refresh(false);
        }

        // Fetch information on this stream.
        public void Refresh(bool onlyEnabled)    {
            AMMediaType MT;
            AMStreamSelectInfoFlags SSIF;
            int lcid = 0, grpid = 0;

            string wname = "";
            Object pObj, pUnk;

            SS.Info(ID, out MT, out SSIF, out lcid, out grpid, out wname, out pObj, out pUnk);

            // For some reason, refreshing text data causes corruption
            // so only refresh the enabled when being refreshed by FilterStreamControl.
            Enabled = (SSIF == AMStreamSelectInfoFlags.Enabled);
            if (!onlyEnabled)
            {
                Name = wname;
                GroupID = grpid;
            }
        }
        
        // Enable this stream.
        public void Enable()
        {
            SS.Enable(ID, AMStreamSelectEnableFlags.Enable);
        }
    }

}
