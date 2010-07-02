using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace StandaloneMB
{
    public class Season : MediaItem
    {

        public MediaCollection Episodes;

        public Series _series;

        public Season(DirectoryInfo d, Series seriesparent)
            : base(d)
        {
            if (backdropImage == "/Images/nobackdrop.jpg")
                backdropImage = seriesparent.backdropImage;
            if (folderImage == "/Images/nofolder.jpg")
                folderImage = seriesparent.folderImage;

            title = d.Name;
            _series = seriesparent;

            Episodes = new MediaCollection { };

            Regex videos = new Regex(".*\\.(avi|mkv|mp4|mpg|mpeg|ogm|wmv|divx|dvr-ms)$");

            foreach (FileInfo f in d.GetFiles())
            {
                if (videos.IsMatch(f.Name))
                {
                    Episode ep = new Episode(f, this);
                    if (ep.Valid)
                        Episodes.Add(ep);
                }
            }

            LoadImage(backdropImage);
            LoadImage(folderImage);
        }

        public override string getOverview()
        {
            return "";
        }

    }
}
