using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.IO;

namespace StandaloneMB
{
    public class Episode : MediaItem
    {

        protected FileInfo EpisodeFile;
        protected XPathDocument EpisodeXml;
        protected XPathNavigator EpisodeNav;

        new public static int elementH = 400;
        new public static int elementW = 300;

        public Season _season;

        public Episode(FileInfo file, Season seasonparent) : base(file.Directory)
        {
            String xmlpath = file.Directory.FullName + "/metadata/" + Path.GetFileNameWithoutExtension(file.FullName) + ".xml";
            if (!File.Exists(xmlpath))
            {
                Valid = false;
                return;
            }
            EpisodeXml = new XPathDocument(xmlpath);
            EpisodeNav = EpisodeXml.CreateNavigator();
            EpisodeFile = file;

            _season = seasonparent;

            transX = 200;
            transY = 100;

            backdropImage = _season.backdropImage;

            XPathNodeIterator nodes = EpisodeNav.Select("//EpisodeID");
            nodes.MoveNext();
            folderImage = file.Directory.FullName + "/metadata/" + nodes.Current.Value + ".jpg";

            if (!File.Exists(folderImage))
                folderImage = "/Images/nothumb.jpg";
            title = this.asTitle();

            videoURL = EpisodeFile.FullName;

            LoadImage(folderImage);
        }

        public string asTitle()
        {
            String ret;

            XPathNodeIterator nodes = EpisodeNav.Select("//EpisodeNumber");
            nodes.MoveNext();
            ret = nodes.Current.Value;

            nodes = EpisodeNav.Select("//EpisodeName");
            nodes.MoveNext();
            return ret + " - " + nodes.Current.Value;
        }

        public override string ToString()
        {
 	         return EpisodeFile.FullName;
        }

        public override string getOverview()
        {
            XPathNodeIterator nodes = EpisodeNav.Select("//Overview");
            nodes.MoveNext();
            return nodes.Current.Value;
        }
    }
}
