// -------------------------------------------------------------------------------
// 
// This file is part of the FluidKit project: http://www.codeplex.com/fluidkit
// 
// Copyright (c) 2008, The FluidKit community 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
// 
// * Redistributions of source code must retain the above copyright notice, this 
// list of conditions and the following disclaimer.
// 
// * Redistributions in binary form must reproduce the above copyright notice, this 
// list of conditions and the following disclaimer in the documentation and/or 
// other materials provided with the distribution.
// 
// * Neither the name of FluidKit nor the names of its contributors may be used to 
// endorse or promote products derived from this software without specific prior 
// written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR 
// ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON 
// ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
// -------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using FluidKit.Controls;
using StandaloneMB;
using WPFMediaKit.DirectShow.MediaPlayers;


namespace FluidKit.Showcase.ElementFlow
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : INotifyPropertyChanged
    {

        #region Private Properties

        private Controls.ElementFlow _elementFlow;
		private Random _randomizer = new Random();
		private MediaCollection _dataSource;
        private int level = 0;

        #endregion

        #region Binding Variables

        public event PropertyChangedEventHandler PropertyChanged;

        private MediaItem _currentitem;
        public MediaItem currentItem {
            get { return _currentitem; }
            set
            {
                _currentitem = value;
                backdropURI = value.backdropImage;
                progress = (_elementFlow.SelectedIndex + 1).ToString() + " | " + _dataSource.Count.ToString();
                onPropertyChanged("currentItem", "progress", "backdropURI", "title");
            }
        }

        public String progress { get; set; }
        public String backdropURI { get; set; }
        public String title { get; set; }

        public String videoVisible { get; set; }
        public int vidColSpan { get; set; }
        public int vidRowSpan { get; set; }

        public double UIElementOpacity
        {
            get { return (double)GetValue(UIElementOpacityProperty); }
            set
            {
                SetValue(UIElementOpacityProperty, value);
                onPropertyChanged("UIElementOpacity");
            }
        } 

        public static readonly DependencyProperty UIElementOpacityProperty =
            DependencyProperty.Register("UIElementOpacity", typeof(double), typeof(Window1), new UIPropertyMetadata(0.0));

        private Storyboard inactiveAnimation = null;

        private String _videoURL;
        public String videoURL
        {
            get { return _videoURL; }
            set
            {
                _videoURL = value;
                onPropertyChanged("videoURL");
            }
        }

        private Boolean _videoPlaying;
        public Boolean videoPlaying
        {
            get { return _videoPlaying; }
            set
            {
                _videoPlaying = value;
                onPropertyChanged("videoPlaying");
            }
        }

        void onPropertyChanged(params string[] propertyNames)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                foreach (var pn in propertyNames)
                {
                    handler(this, new PropertyChangedEventArgs(pn));
                }
            }
        }

        #endregion

        #region Initialisation

        public Window1()
		{
            try
            {
			    InitializeComponent();
            }
            catch (Exception)
            {
                File.WriteAllText("debug.txt", "Initialisation failed. Do you have the latest .Net Framework? (3.5 SP1?)");
                this.Close();
            }
            
            backdropURI = "/Images/backdrop.jpg";
            title = "Loading...";
            videoVisible = "Hidden";
            videoPlaying = true;
            vidColSpan = 1;
            vidRowSpan = 1;

            //VideoControl.Get

			Loaded += Window1_Loaded;
		}

		private void Window1_Loaded(object sender, RoutedEventArgs e)
		{
            //videoURL = "Q:\\Anime\\Black Lagoon [EngSub]\\Season 101x01 - The Black Lagoon.mkv";

			// Get reference to ElementFlow
			DependencyObject obj = VisualTreeHelper.GetChild(_itemsControl, 0);
			while ((obj is Controls.ElementFlow) == false)
			{
				obj = VisualTreeHelper.GetChild(obj, 0);
			}

			_elementFlow = obj as Controls.ElementFlow;
            _elementFlow.SelectedIndexChanged += EFSelectedIndexChanged;
            _elementFlow.SelectedIndex = 0;
            _elementFlow.ElementHeight = Folder.elementH;
            _elementFlow.ElementWidth = Folder.elementW;

            // This makes binding work ???
            window.DataContext = this;

            _dataSource = FindResource("TestDataSource") as MediaCollection;

            this.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate()
                    {
                        InitialLoad();
                    }
                ));

		}

        public void InitialLoad()
        {
            String conf = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\vfpath.txt";
            try
            {
                StreamReader sr = new StreamReader(conf);
                conf = sr.ReadLine();
                sr.Close();

                VirtualFolders vf = new VirtualFolders(conf);

                AddItems(vf._folders, vf._folders[0]);
            }
            catch
            {
                this.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                        delegate()
                        {
                            Console.WriteLine("NO VF PATH?");
                            window.Close();
                        }
                    ));
            }
        }

        private void AddItems(MediaCollection mc, MediaItem selected)
        {
            foreach (MediaItem mi in mc)
            {
                _dataSource.Add(mi);
                if (mi == selected)
                {
                    _elementFlow.SelectedIndex = _dataSource.IndexOf(mi);
                    currentItem = mi;
                }
            }
        }

        #endregion

        #region Event Handlers

        private void EFSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_dataSource == null)
                return;
            currentItem = _dataSource[(sender as FluidKit.Controls.ElementFlow).SelectedIndex];
        }

		protected override void OnKeyDown(KeyEventArgs e)
		{
            if (videoVisible == "Hidden")
            {
                if (e.Key == Key.Escape && level == 0)
                {
                    this.Close();
                }
                else if (e.Key == Key.Escape || e.Key == Key.Back)
                {
                    UpLevel();
                }
                else if (e.Key == Key.Enter)
                {
                    DownLevel();
                }
                else if (e.Key == Key.Left)
                {
                    MoveLeft();
                }
                else if (e.Key == Key.Right)
                {
                    MoveRight();
                }
            }
            else
            {
                if (e.Key == Key.Escape)
                {
                    ToggleVideoControlMinMax();
                }
            }

		}

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                MoveLeft();
            }
            else if (e.Delta > 0)
            {
                MoveRight();
            }
        }

        private void ChangeSelectedIndex(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            _elementFlow.SelectedIndex = (int)args.NewValue;
        }

        private void VideoControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                ToggleVideoControlMinMax();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            VideoControl.Pause();
            videoPlaying = false;
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            VideoControl.Play();
            videoPlaying = true;
            
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            videoPlaying = false;
            ExitVideo();
        }

        private void VideoMinMax_Click(object sender, RoutedEventArgs e)
        {
            ToggleVideoControlMinMax();
        }

        private void Beginning_Click(object sender, RoutedEventArgs e)
        {
            VideoControl.Pause();
            VideoControl.MediaPosition = 0;
            //VideoControl.Position = new TimeSpan(0, 0, 0);
            VideoControl.Play();
        }

        private void MinMax_Click(object sender, RoutedEventArgs e)
        {
            if (window.WindowState == WindowState.Maximized)
                window.WindowState = WindowState.Normal;
            else
                window.WindowState = WindowState.Maximized;
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            window.WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

        }

        private void StreamSelect_Click(object sender, RoutedEventArgs e)
        {
            //if (UriElement)
            ((FilterStream)(((MenuItem)sender).Tag)).Enable();
        }

        private void window_MouseMove(object sender, MouseEventArgs e)
        {
            if (inactiveAnimation != null)
            {
                if (inactiveAnimation.GetCurrentTime() < new TimeSpan(0, 0, 0, 0, 500))
                {
                    return;
                }
                else
                {
                    inactiveAnimation.Stop();
                }
            }
            inactiveAnimation = new Storyboard();

            inactiveAnimation.Completed += delegate
            {
                this.Cursor = Cursors.None;
            };

            this.Cursor = Cursors.Arrow;

            DoubleAnimation show = new DoubleAnimation(this.UIElementOpacity, 1.0, new Duration(new TimeSpan(0, 0, 0, 0, 500)));
            Storyboard.SetTarget(show, this);
            Storyboard.SetTargetProperty(show, new PropertyPath(UIElementOpacityProperty));
            inactiveAnimation.Children.Add(show);

            DoubleAnimation fade = new DoubleAnimation(1.0, 0.0, new Duration(new TimeSpan(0, 0, 1)));
            fade.BeginTime = new TimeSpan(0, 0, 3);
            Storyboard.SetTarget(fade, this);
            Storyboard.SetTargetProperty(fade, new PropertyPath(UIElementOpacityProperty));
            inactiveAnimation.Children.Add(fade);

            inactiveAnimation.Begin();

        }

        private void VideoControl_Loaded(object sender, RoutedEventArgs e)
        {
            videoPlaying = true;
            BuildVideoContextMenu();
        }

        private void ElementFlow_MouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource.GetType().Name.ToString() == "ModelUIElement3D")
            {
                if (_elementFlow.IndexOf((ModelUIElement3D)e.OriginalSource) == _elementFlow.SelectedIndex)
                    DownLevel();
            }
        }

        #endregion

        #region Navigation

        private void UpLevel()
        {
            Season sea;
            Series ser;
            Folder fol;
            switch (level)
            {
                case 1:  // on a folder, going up to virtualfolders
                    if (currentItem.GetType().Name == "Series")
                        fol = ((Series)currentItem)._folder;
                    else
                        fol = ((Movie)currentItem)._folder;
                    _elementFlow.SelectedIndex = 0;
                    _dataSource.Clear();
                    _elementFlow.ElementHeight = Folder.elementH;
                    _elementFlow.ElementWidth = Folder.elementW;
                    AddItems(fol._vfs._folders, fol);
                    level = 0;
                    break;
                case 2:  // On a series (seasons) going up to a folder (series)
                    ser = ((Season)currentItem)._series;
                    fol = ser._folder;
                    _elementFlow.SelectedIndex = 0;
                    _dataSource.Clear();
                    _elementFlow.ElementHeight = Series.elementH;
                    _elementFlow.ElementWidth = Series.elementW;
                    AddItems(fol.getContents(), ser);
                    _elementFlow.ResortAlpha();
                    level = 1;
                    break;
                case 3:  // On a season (episodes) going up to a series (seasons)
                    sea = ((Episode)currentItem)._season;
                    ser = sea._series;
                    _elementFlow.SelectedIndex = 0;
                    _dataSource.Clear();
                    _elementFlow.ElementHeight = Season.elementH;
                    _elementFlow.ElementWidth = Season.elementW;
                    AddItems(ser.Seasons, sea);
                    _elementFlow.ResortAlpha();
                    level = 2;
                    break;
            }
        }

        private void DownLevel()
        {
            Season sea;
            Series ser;
            Movie mov;
            Folder fol;
            switch (level)
            {
                case 0:
                    fol = (Folder)currentItem;
                    _elementFlow.SelectedIndex = 0;
                    _dataSource.Clear();
                    _elementFlow.ElementHeight = Series.elementH;
                    _elementFlow.ElementWidth = Series.elementW;
                    AddItems(fol._contents, null);
                    level = 1;
                    this.currentItem = _dataSource[0];
                    _elementFlow.ResortAlpha();
                    onPropertyChanged("currentItem");
                    break;
                case 1:
                    if (currentItem.GetType().Name == "Series")  // On a folder, going into a series (seasons).
                    {
                        ser = (Series)currentItem;

                        if (ser.Seasons.Count == 0)
                            break;

                        _elementFlow.SelectedIndex = 0;
                        _dataSource.Clear();
                        _elementFlow.ElementHeight = Season.elementH;
                        _elementFlow.ElementWidth = Season.elementW;
                        AddItems(ser.Seasons, null);
                        level = 2;
                        this.currentItem = _dataSource[0];
                        _elementFlow.ResortAlpha();
                        onPropertyChanged("currentItem");
                    }
                    else if (currentItem.GetType().Name == "Movie")
                    {
                        mov = (Movie) currentItem;
                        videoURL = mov.videoURL;
                    }
                    break;
                case 2:  // On a series, going into a season (episodes)
                    sea = (Season)currentItem;
                    _elementFlow.SelectedIndex = 0;
                    _dataSource.Clear();
                    _elementFlow.ElementHeight = Episode.elementH;
                    _elementFlow.ElementWidth = Episode.elementW;
                    AddItems(sea.Episodes, null);
                    level = 3;
                    if (_dataSource.Count > 0)
                        this.currentItem = _dataSource[0];
                    _elementFlow.ResortAlpha();
                    _itemsControl.Items.Refresh();
                    onPropertyChanged("currentItem");
                    break;
                case 3:
                    videoURL = currentItem.videoURL;
                    break;
            }
        }

        private void MoveLeft()
        {
            if (_elementFlow.SelectedIndex > 0)
                _elementFlow.SelectedIndex--;
        }

        private void MoveRight()
        {
            if (_elementFlow.SelectedIndex < (_elementFlow.Children.Count - 1))
                _elementFlow.SelectedIndex++;
        }

        #endregion

        #region UI

        private void ToggleVideoControlMinMax()
        {
            VideoControl.Pause();
            System.Diagnostics.Process player = new System.Diagnostics.Process();
            player.EnableRaisingEvents = false;
            player.StartInfo.FileName = "mpchc";
            player.StartInfo.Arguments = "\"" + videoURL + "\" /start " + (VideoControl.MediaPosition / 10000).ToString() + " /fullscreen /play /close";
            Console.WriteLine("mpchc " + player.StartInfo.Arguments);
            player.Start();
            return;

            if (videoVisible == "Visible")
            {
                vidColSpan = 1;
                vidRowSpan = 1;
                videoVisible = "Hidden";
            }
            else if (VideoControl.HasVideo == true)
            {
                vidColSpan = 3;
                vidRowSpan = 5;
                videoVisible = "Visible";
            }
            onPropertyChanged("vidColSpan", "vidRowSpan", "videoVisible");
        }

        private void ExitVideo()
        {
            VideoControl.Stop();
            VideoControl.Close();
            vidColSpan = 1;
            vidRowSpan = 1;
            videoVisible = "Hidden";
            onPropertyChanged("vidColSpan", "vidRowSpan", "videoVisible");
        }

        private void BuildVideoContextMenu()
        {
            //if (UriElement)
            VideoControl.Filters.Refresh();
            ItemCollection cmi = VideoControl.ContextMenu.Items;
            cmi.Clear();
            
            MenuItem mi;

            mi = new MenuItem();
            mi.Header = "Play";
            mi.Click += Play_Click;
            cmi.Add(mi);

            mi = new MenuItem();
            mi.Header = "Pause";
            mi.Click += Pause_Click;
            cmi.Add(mi);

            mi = new MenuItem();
            mi.Header = "Stop";
            mi.Click += Stop_Click;
            cmi.Add(mi);

            mi = new MenuItem();
            mi.Header = "Min / Max";
            mi.Click += VideoMinMax_Click;
            cmi.Add(mi);
            
            //if (UriElement)

            // Filters submenu
            cmi.Add(new Separator { });
            mi = new MenuItem();
            mi.Header = "Tracks";
            Filter splitter = VideoControl.Filters.getFilter(videoURL);
            if (splitter.Count == 0)
                mi.IsEnabled = false;

            int previd = 0;

            foreach (FilterStream s in splitter.Streams)
            {
                MenuItem smi = new MenuItem();
                smi.Header = s.Name;
                smi.Tag = s;
                smi.Click += new RoutedEventHandler(StreamSelect_Click);
                smi.IsCheckable = true;
                smi.IsChecked = s.Enabled;
                if (s.GroupID != previd)
                    mi.Items.Add(new Separator { });
                previd = s.GroupID;
                mi.Items.Add(smi);
            }

            cmi.Add(mi);

            // Filters submenu
            cmi.Add(new Separator { });
            mi = new MenuItem();
            mi.Header = "Filters";
            if (VideoControl.Filters.Count == 0)
                mi.IsEnabled = false;

            previd = 0;

            foreach (Filter f in VideoControl.Filters.Filters)
            {
                MenuItem fi = new MenuItem();
                fi.Header = f.GetFilterInfo.achName;
                fi.Tag = f;

                foreach (FilterStream s in f.Streams)
                {
                    MenuItem smi = new MenuItem();
                    smi.Header = s.Name;
                    smi.Tag = s;
                    smi.Click += new RoutedEventHandler(StreamSelect_Click);
                    smi.IsCheckable = true;
                    smi.IsChecked = s.Enabled;
                    if (s.GroupID != previd)
                        fi.Items.Add(new Separator { });
                    previd = s.GroupID;
                    fi.Items.Add(smi);
                }

                mi.Items.Add(fi);
            }

            cmi.Add(mi);
        }

        #endregion

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        //    int sv = (int)ProgressSlider.Value;
        //    sv = (int)(VideoControl.NaturalDuration.TimeSpan.TotalMilliseconds * (((double)sv) / 1000));
        //    Console.WriteLine(sv);
        //    VideoControl.Position = new TimeSpan(0, 0, 0, 0, sv);
        }

        private void VideoControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            BuildVideoContextMenu();
        }

    }
}