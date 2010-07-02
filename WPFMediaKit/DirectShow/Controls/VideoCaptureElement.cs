using System;
using System.Windows;
using WPFMediaKit.DirectShow.MediaPlayers;

namespace WPFMediaKit.DirectShow.Controls
{
    /// <summary>
    /// The VideoCaptureElement is a WPF control that
    /// displays video from a capture device, such as 
    /// a web cam.
    /// </summary>
    public class VideoCaptureElement : MediaElementBase
    {
        #region DesiredPixelWidth

        public static readonly DependencyProperty DesiredPixelWidthProperty =
            DependencyProperty.Register("DesiredPixelWidth", typeof(int), typeof(VideoCaptureElement),
                new FrameworkPropertyMetadata(0));

        public int DesiredPixelWidth
        {
            get { return (int)GetValue(DesiredPixelWidthProperty); }
            set { SetValue(DesiredPixelWidthProperty, value); }
        }

        #endregion

        #region DesiredPixelHeight

        public static readonly DependencyProperty DesiredPixelHeightProperty =
            DependencyProperty.Register("DesiredPixelHeight", typeof(int), typeof(VideoCaptureElement),
                new FrameworkPropertyMetadata(0));

        public int DesiredPixelHeight
        {
            get { return (int)GetValue(DesiredPixelHeightProperty); }
            set { SetValue(DesiredPixelHeightProperty, value); }
        }

        #endregion

        #region FPS

        public static readonly DependencyProperty FPSProperty =
            DependencyProperty.Register("FPS", typeof(int), typeof(VideoCaptureElement),
                new FrameworkPropertyMetadata(30));

        public int FPS
        {
            get { return (int)GetValue(FPSProperty); }
            set { SetValue(FPSProperty, value); }
        }

        #endregion

        #region VideoCaptureSource

        public static readonly DependencyProperty VideoCaptureSourceProperty =
            DependencyProperty.Register("VideoCaptureSource", typeof(string), typeof(VideoCaptureElement),
                new FrameworkPropertyMetadata("",
                    new PropertyChangedCallback(OnVideoCaptureSourceChanged)));

        private bool m_sourceChanged;

        public string VideoCaptureSource
        {
            get { return (string)GetValue(VideoCaptureSourceProperty); }
            set { SetValue(VideoCaptureSourceProperty, value); }
        }

        private static void OnVideoCaptureSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VideoCaptureElement)d).OnVideoCaptureSourceChanged(e);
        }

        protected virtual void OnVideoCaptureSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            m_sourceChanged = true;

            if (!IsInitialized)
            {
                /* If this element is not initialized
                 * run the UnloadedBehavior */
                ExecuteMediaState(UnloadedBehavior);
            }
            else
            {
                /* If this element is loaded,
                 * run the LoadedBehavior */
                ExecuteMediaState(LoadedBehavior);
            }
        }

        #endregion

        protected VideoCapturePlayer VideoCapturePlayer
        {
            get
            {
                return MediaPlayerBase as VideoCapturePlayer;
            }
        }

        /// <summary>
        /// Sets the parameters to the video capture player
        /// </summary>
        private void SetParameters()
        {
            int height = DesiredPixelHeight;
            int width = DesiredPixelWidth;
            int fps = FPS;

            VideoCapturePlayer.Dispatcher.BeginInvoke((Action) delegate
            {
                VideoCapturePlayer.FPS = fps;
                VideoCapturePlayer.DesiredWidth = width;
                VideoCapturePlayer.DesiredHeight = height;
            });
        }

        /// <summary>
        /// Decides if the Uri source should be
        /// changed, if so, it changes it with the player.
        /// </summary>
        private void DecideChangeSource()
        {
            if (m_sourceChanged)
            {
                string videoSource = VideoCaptureSource;
                VideoCapturePlayer.Dispatcher.BeginInvoke((Action)delegate
                {
                    VideoCapturePlayer.VideoCaptureSource = videoSource;
                });
                m_sourceChanged = false;
            }
        }

        /// <summary>
        /// The Play method is overrided so we can
        /// set the video capture source to the media
        /// </summary>
        public override void Play()
        {
            SetParameters();
            DecideChangeSource();
            base.Play();
        }

        /// <summary>
        /// The Pause method is overrided so we can
        /// set the video capture source to the media
        /// </summary>
        public override void Pause()
        {
            SetParameters();
            DecideChangeSource();
            base.Pause();
        }
        
        protected override MediaPlayerBase OnRequestMediaPlayer()
        {
            return VideoCapturePlayer.CreateMediaUriPlayer();
        }
    }
}
