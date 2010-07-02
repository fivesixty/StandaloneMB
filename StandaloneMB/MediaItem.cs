using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.ComponentModel;
using System.Threading;
using System.Drawing;
using System.Media;

namespace StandaloneMB
{
    abstract public class MediaItem
    {
        protected DirectoryInfo DirInfo;

        static private Queue<QueueItem<String>> ImageQueue = new Queue<QueueItem<String>>();

        public String folderImage { get; set; }
        public String backdropImage { get; set; }
        public String title { get; set; }
        public String videoURL { get; set; }
        public int transX { get; set; }
        public int transY { get; set; }
        public bool Valid { get; set; }

        public String Overview
        {
            get
            {
                return getOverview();
            }
        }
        public Boolean OverviewEmpty
        {
            get
            {
                return (Overview.Length == 0);
            }
        }

        public static int elementW = 200;
        public static int elementH = 600;

        public MediaItem(DirectoryInfo di)
        {
            DirInfo = di;

            folderImage = DirInfo.FullName + "\\folder.jpg";
            if (!File.Exists(folderImage))
                folderImage = "/Images/nofolder.jpg";
            backdropImage = DirInfo.FullName + "\\backdrop.jpg";
            if (!File.Exists(backdropImage))
                backdropImage = "/Images/nobackdrop.jpg";
            videoURL = "";
            transX = 200;
            transY = 150;
            Valid = true;

        }

        public DirectoryInfo getDirInfo()
        {
            return DirInfo;
        }

        public void LoadImage(String path)
        {

            return;

            if (File.Exists(path) /*&& path.StartsWith("\\")*/)
                QueuedBackgroundWorker.QueueWorkItem<String>(
            //    BackgroundWorkerHelper.DoWork<String>(
                    ImageQueue,
                    path,
                    new Action<String>(
                        delegate(String p)
                        {
                            Bitmap.FromFile(p);
                            return;
                        }
                    )
                );
        }

        public abstract String getOverview();
    }
}
