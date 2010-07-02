using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;

namespace StandaloneMB
{
    public class Movie : MediaItem
    {
        protected XPathDocument MovieXml;
        protected XPathNavigator MovieNav;

        public Folder _folder { get; set; }

        public Movie(DirectoryInfo di, Folder f) : base(di)
        {
            MovieXml = new XPathDocument(DirInfo.GetFiles("mymovies.xml").First().OpenText());
            MovieNav = MovieXml.CreateNavigator();

            _folder = f;

            title = this.ToString();

            Regex videos = new Regex(".*\\.(avi|mkv|mp4|mpg|mpeg|ogm|wmv|divx|dvr-ms)$");

            foreach (FileInfo fi in di.GetFiles())
            {
                if (videos.IsMatch(fi.Name))
                {
                    videoURL = fi.FullName;
                    break;
                }
            }

            LoadImage(backdropImage);
            LoadImage(folderImage);

        }

        public override String ToString()
        {
            XPathNodeIterator nodes = MovieNav.Select("//LocalTitle");
            nodes.MoveNext();
            return "" + nodes.Current.Value;
        }

        public override String getOverview()
        {
            XPathNodeIterator nodes = MovieNav.Select("//Description");
            nodes.MoveNext();
            return "" + nodes.Current.Value;
        }

    }
}
