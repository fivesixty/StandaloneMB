using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Threading;
namespace StandaloneMB
{
    public class VirtualFolders
    {

        public MediaCollection _folders;
        DirectoryInfo _dir;

        public VirtualFolders(String path)   {
            _folders = new MediaCollection { };
            _dir = new DirectoryInfo(path);

            StreamReader sr;
            String img, fold;
            Folder fo;
            foreach (FileInfo f in _dir.GetFiles("*.vf"))
            {
                sr = new StreamReader(f.FullName);
                img = sr.ReadLine();
                fold = sr.ReadLine();
                sr.Close();

                img = img.Substring(7);
                fold = fold.Substring(8);

                fo = new Folder(fold, this);
                fo.folderImage = img;
                try
                {
                    fo.fetchContents();
                }
                catch
                {
                    continue;
                }
                _folders.Add(fo);
            }
        }

    }
}
