using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace StandaloneMB
{
    public class Series : MediaItem
    {

        protected XPathDocument SeriesXml;
        protected XPathNavigator SeriesNav;

        public MediaCollection Seasons { get; set; }
        public Folder _folder { get; set; }

        public Series(DirectoryInfo di, Folder f) : base(di)
        {
            SeriesXml = new XPathDocument(DirInfo.GetFiles("series.xml").First().OpenText());
            SeriesNav = SeriesXml.CreateNavigator();

            Seasons = new MediaCollection { };

            _folder = f;

            title = this.ToString();

            foreach (DirectoryInfo d in DirInfo.GetDirectories())
            {
                Seasons.Add(new Season(d, this));
            }
        }

        public override String ToString()
        {
            XPathNodeIterator nodes = SeriesNav.Select("//SeriesName");
            nodes.MoveNext();
            return "" + nodes.Current.Value;
        }

        public override String getOverview()
        {
            XPathNodeIterator nodes = SeriesNav.Select("//Overview");
            nodes.MoveNext();
            return "" + nodes.Current.Value;
        }

        public Int32 CountSeasons()
        {
            Int32 count = DirInfo.GetDirectories("Season *").Count();
            if (HasSpecials())
                count--;
            return count;
        }

        public bool HasSpecials()
        {
            return Directory.Exists(DirInfo.FullName + "/Season 0");
        }

    }
}
