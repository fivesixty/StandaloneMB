using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Threading;

namespace StandaloneMB
{
    public class Folder : MediaItem
    {

        public MediaCollection _contents;
        private DirectoryInfo _dir;
        public VirtualFolders _vfs;

        new public static int elementH = 400;
        new public static int elementW = 190;

        public Folder(String fol, VirtualFolders vf) : base(new DirectoryInfo(fol))
        {
            _dir = DirInfo;
            _contents = new MediaCollection { };
            _vfs = vf;

            transX = 200;
            transY = 100;

            backdropImage = "/Images/backdrop.jpg";

            title = _dir.Name;
        }

        public void fetchContents()
        {
            MediaItem mi = null;
            foreach (DirectoryInfo d in _dir.GetDirectories())
            {
                if (File.Exists(d.FullName + "/series.xml"))
                {
                    mi = new Series(d, this);
                }
                else if (File.Exists(d.FullName + "/mymovies.xml"))
                {
                    mi = new Movie(d, this);
                }
                else
                {
                    continue;
                }

                _contents.Add(mi);
            }
        }

        public MediaCollection getContents()
        {
            return _contents;
        }

        public override String getOverview() {
            return "";
        }


    }
}
