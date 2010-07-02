using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using WPFMediaKit.DirectShow.MediaPlayers;

namespace WPFMediaKit.DirectShow.Controls
{
    /// <summary>
    /// The MediaElementBase is the base WPF control for
    /// making custom media players.  The MediaElement uses the
    /// D3DRenderer class for rendering video
    /// </summary>
    public abstract class MediaElementBase : D3DRenderer
    {
        #region Routed Events
        #region MediaOpened

        public static readonly RoutedEvent MediaOpenedEvent = EventManager.RegisterRoutedEvent("MediaOpened",
                                                                                               RoutingStrategy.Bubble,
                                                                                               typeof(RoutedEventHandler
                                                                                                   ),
                                                                                               typeof(MediaElementBase));

        /// <summary>
        /// Fires when media has successfully been opened
        /// </summary>
        public event RoutedEventHandler MediaOpened
        {
            add { AddHandler(MediaOpenedEvent, value); }
            remove { RemoveHandler(MediaOpenedEvent, value); }
        }

        #endregion

        #region MediaClosed

        public static readonly RoutedEvent MediaClosedEvent = EventManager.RegisterRoutedEvent("MediaClosed",
                                                                                               RoutingStrategy.Bubble,
                                                                                               typeof(RoutedEventHandler
                                                                                                   ),
                                                                                               typeof(MediaElementBase));

        /// <summary>
        /// Fires when media has been closed
        /// </summary>
        public event RoutedEventHandler MediaClosed
        {
            add { AddHandler(MediaClosedEvent, value); }
            remove { RemoveHandler(MediaClosedEvent, value); }
        }

        #endregion

        #region MediaEnded

        public static readonly RoutedEvent MediaEndedEvent = EventManager.RegisterRoutedEvent("MediaEnded",
                                                                                              RoutingStrategy.Bubble,
                                                                                              typeof(RoutedEventHandler),
                                                                                              typeof(MediaElementBase));

        /// <summary>
        /// Fires when media has completed playing
        /// </summary>
        public event RoutedEventHandler MediaEnded
        {
            add { AddHandler(MediaEndedEvent, value); }
            remove { RemoveHandler(MediaEndedEvent, value); }
        }

        #endregion
        #endregion

        #region Dependency Properties
        #region UnloadedBehavior

        public static readonly DependencyProperty UnloadedBehaviorProperty =
            DependencyProperty.Register("UnloadedBehavior", typeof(MediaState), typeof(MediaElementBase),
                                        new FrameworkPropertyMetadata(MediaState.Close));

        /// <summary>
        /// Defines the behavior of the control when it is unloaded
        /// </summary>
        public MediaState UnloadedBehavior
        {
            get { return (MediaState)GetValue(UnloadedBehaviorProperty); }
            set { SetValue(UnloadedBehaviorProperty, value); }
        }

        #endregion

        #region LoadedBehavior

        public static readonly DependencyProperty LoadedBehaviorProperty =
            DependencyProperty.Register("LoadedBehavior", typeof(MediaState), typeof(MediaElementBase),
                                        new FrameworkPropertyMetadata(MediaState.Play));

        /// <summary>
        /// Defines the behavior of the control when it is loaded
        /// </summary>
        public MediaState LoadedBehavior
        {
            get { return (MediaState)GetValue(LoadedBehaviorProperty); }
            set { SetValue(LoadedBehaviorProperty, value); }
        }

        #endregion

        #region Volume

        public static readonly DependencyProperty VolumeProperty =
            DependencyProperty.Register("Volume", typeof(double), typeof(MediaElementBase),
                new FrameworkPropertyMetadata(1.0d,
                    new PropertyChangedCallback(OnVolumeChanged)));

        /// <summary>
        /// Gets or sets the audio volume.  Specifies the volume, as a 
        /// number from 0 to 1.  Full volume is 1, and 0 is silence.
        /// </summary>
        public double Volume
        {
            get { return (double)GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }

        private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MediaElementBase)d).OnVolumeChanged(e);
        }

        protected virtual void OnVolumeChanged(DependencyPropertyChangedEventArgs e)
        {
            MediaPlayerBase.Dispatcher.BeginInvoke((Action)delegate
            {
                MediaPlayerBase.Volume = (double)e.NewValue;
            });
        }

        #endregion

        #region Balance

        public static readonly DependencyProperty BalanceProperty =
            DependencyProperty.Register("Balance", typeof(double), typeof(MediaElementBase),
                new FrameworkPropertyMetadata((double)0,
                    new PropertyChangedCallback(OnBalanceChanged)));

        /// <summary>
        /// Gets or sets the balance on the audio.
        /// The value can range from -1 to 1. The value -1 means the right channel is attenuated by 100 dB 
        /// and is effectively silent. The value 1 means the left channel is silent. The neutral value is 0, 
        /// which means that both channels are at full volume. When one channel is attenuated, the other 
        /// remains at full volume.
        /// </summary>
        public double Balance
        {
            get { return (double)GetValue(BalanceProperty); }
            set { SetValue(BalanceProperty, value); }
        }

        private static void OnBalanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MediaElementBase)d).OnBalanceChanged(e);
        }

        protected virtual void OnBalanceChanged(DependencyPropertyChangedEventArgs e)
        {
            MediaPlayerBase.Dispatcher.BeginInvoke((Action)delegate
            {
                MediaPlayerBase.Balance = (double)e.NewValue;
            });
        }

        #endregion
        #endregion

        /// <summary>
        /// The backing field for the WindowOwner property
        /// </summary>
        private Window m_windowOwner;

        /// <summary>
        /// The current window the element belongs to
        /// </summary>
        protected Window WindowOwner
        {
            get
            {
                return m_windowOwner;
            }
        }

        /// <summary>
        /// Notifies when the media has failed and produced an exception
        /// </summary>
        public event EventHandler<MediaFailedEventArgs> MediaFailed;

        protected MediaElementBase()
        {
            /* Nothing wrong with a virtual method call
             * from a protected constructor, no? */
            InitializeMediaPlayer();
            Loaded += MediaElementBase_Loaded;
            Unloaded += MediaElementBase_Unloaded;
        }

        protected MediaPlayerBase MediaPlayerBase { get; set; }

        /// <summary>
        /// Initializes the media player, hooking into events
        /// and other general setup.
        /// </summary>
        protected virtual void InitializeMediaPlayer()
        {
            if (MediaPlayerBase != null)
                return;

            MediaPlayerBase = OnRequestMediaPlayer();

            if (MediaPlayerBase == null)
            {
                throw new Exception("OnRequestMediaPlayer cannot return null");
            }

            /* Hook into the normal .NET events */
            MediaPlayerBase.MediaOpened += OnMediaPlayerOpenedPrivate;
            MediaPlayerBase.MediaClosed += OnMediaPlayerClosedPrivate;
            MediaPlayerBase.MediaFailed += OnMediaPlayerFailedPrivate;
            MediaPlayerBase.MediaEnded += OnMediaPlayerEndedPrivate;

            /* These events fire when we get new D3Dsurfaces or frames */
            MediaPlayerBase.NewAllocatorFrame += OnMediaPlayerNewAllocatorFramePrivate;
            MediaPlayerBase.NewAllocatorSurface += OnMediaPlayerNewAllocatorSurfacePrivate;
        }

        #region Private Event Handlers
        private void OnMediaPlayerFailedPrivate(object sender, MediaFailedEventArgs e)
        {
            OnMediaPlayerFailed(e);
        }

        private void OnMediaPlayerNewAllocatorSurfacePrivate(object sender, IntPtr pSurface)
        {
            OnMediaPlayerNewAllocatorSurface(pSurface);
        }

        private void OnMediaPlayerNewAllocatorFramePrivate()
        {
            OnMediaPlayerNewAllocatorFrame();
        }

        private void OnMediaPlayerClosedPrivate()
        {
            OnMediaPlayerClosed();
        }

        private void OnMediaPlayerEndedPrivate()
        {
            OnMediaPlayerEnded();
        }

        private void OnMediaPlayerOpenedPrivate()
        {
            OnMediaPlayerOpened();
        }
        #endregion

        /// <summary>
        /// Fires the MediaFailed event
        /// </summary>
        /// <param name="e">The failed media arguments</param>
        protected void InvokeMediaFailed(MediaFailedEventArgs e)
        {
            EventHandler<MediaFailedEventArgs> mediaFailedHandler = MediaFailed;
            if (mediaFailedHandler != null) mediaFailedHandler(this, e);
        }

        /// <summary>
        /// Executes when a media operation failed
        /// </summary>
        /// <param name="e">The failed event arguments</param>
        protected virtual void OnMediaPlayerFailed(MediaFailedEventArgs e)
        {
            InvokeMediaFailed(e);
        }

        /// <summary>
        /// Is executes when a new D3D surfaces has been allocated
        /// </summary>
        /// <param name="pSurface">The pointer to the D3D surface</param>
        protected virtual void OnMediaPlayerNewAllocatorSurface(IntPtr pSurface)
        {
            SetBackBuffer(pSurface);
        }

        /// <summary>
        /// Called for every frame in media that has video
        /// </summary>
        protected virtual void OnMediaPlayerNewAllocatorFrame()
        {
            InvalidateVideoImage();
        }

        /// <summary>
        /// Called when the media has been closed
        /// </summary>
        protected virtual void OnMediaPlayerClosed()
        {
            Dispatcher.BeginInvoke((Action)(() => RaiseEvent(new RoutedEventArgs(MediaClosedEvent))));
        }

        /// <summary>
        /// Called when the media has ended
        /// </summary>
        protected virtual void OnMediaPlayerEnded()
        {
            Dispatcher.BeginInvoke((Action)(() => RaiseEvent(new RoutedEventArgs(MediaEndedEvent))));
        }

        /// <summary>
        /// Executed when media has successfully been opened.
        /// </summary>
        protected virtual void OnMediaPlayerOpened()
        {
            /* Safely grab out our values */
            bool hasVideo = MediaPlayerBase.HasVideo;
            int videoWidth = MediaPlayerBase.NaturalVideoWidth;
            int videoHeight = MediaPlayerBase.NaturalVideoHeight;
            double volume = 0;
            double balance = 0;

            Dispatcher.BeginInvoke((Action)delegate
            {
                /* If we have no video just black out the video
                 * area by releasing the D3D surface */
                if (!hasVideo)
                {
                    SetBackBuffer(IntPtr.Zero);
                }

                SetNaturalVideoWidth(videoWidth);
                SetNaturalVideoHeight(videoHeight);

                /* Set our dp values to match the media player */
                SetHasVideo(hasVideo);

                /* Get our DP values */
                volume = Volume;
                balance = Balance;
                
                /* Make sure our volume and balances are set */
                MediaPlayerBase.Dispatcher.BeginInvoke((Action)delegate
                {
                    MediaPlayerBase.Volume = volume;
                    MediaPlayerBase.Balance = balance;
                });

                RaiseEvent(new RoutedEventArgs(MediaOpenedEvent));
            });
        }

        /// <summary>
        /// Finds the root visual in the visual tree
        /// </summary>
        private static DependencyObject FindVisualTreeRoot(DependencyObject initial)
        {
            DependencyObject current = initial;
            DependencyObject result = initial;

            while (current != null)
            {
                result = current;
                if (current is Visual || current is Visual3D)
                {
                    current = VisualTreeHelper.GetParent(current);
                }
                else
                {
                    current = LogicalTreeHelper.GetParent(current);
                }
            }

            return result;
        }

        /// <summary>
        /// Hooks into events of the current window the element belongs to
        /// </summary>
        private void HookCurrentWPFWindow()
        {
            if (m_windowOwner == null)
                return;

            m_windowOwner.Dispatcher.BeginInvoke((Action)delegate
            {
                m_windowOwner.Closed += WindowOwner_Closed;
            });
        }

        /// <summary>
        /// Unhooks events that were hooked into by the HookCurrentWPFWindow call
        /// </summary>
        private void UnHookCurrentWPFWindow()
        {
            if (m_windowOwner == null)
                return;

            m_windowOwner.Dispatcher.BeginInvoke((Action)delegate
            {
                m_windowOwner.Closed -= WindowOwner_Closed;
            });
        }

        /// <summary>
        /// Fires when the owner window is closed.  Nothing will happen
        /// if the visual does not belong to the visual tree with a root
        /// of a WPF window
        /// </summary>
        private void WindowOwner_Closed(object sender, EventArgs e)
        {
            MediaPlayerBase.Dispatcher.BeginInvoke((Action)(() => MediaPlayerBase.Dispose()));
        }

        /// <summary>
        /// Local handler for the Loaded event
        /// </summary>
        private void MediaElementBase_Unloaded(object sender, RoutedEventArgs e)
        {
            UnHookCurrentWPFWindow();

            if (Application.Current == null)
                return;

            /* Use the dispatcher in case this thread is running
             * under a different thread than the other dispatcher */
            Application.Current.Dispatcher.BeginInvoke((Action)delegate
            {
                /* Hook into the main window so we can guarentee disposal 
                 * of the MediaPlayerBase */
                m_windowOwner = Application.Current.MainWindow;

                if (Application.Current.MainWindow == null)
                    return;

                /* Make sure to use our  local dispatcher */
                Dispatcher.BeginInvoke((Action)delegate
                {
                    HookCurrentWPFWindow();

                    OnUnloadedOverride();
                });
            });
        }

        /// <summary>
        /// Local handler for the Unloaded event
        /// </summary>
        private void MediaElementBase_Loaded(object sender, RoutedEventArgs e)
        {
            /* Look for the current Window */
            object root = FindVisualTreeRoot(this);

            if (root is Window)
            {
                UnHookCurrentWPFWindow();
                m_windowOwner = root as Window;
                HookCurrentWPFWindow();
            }

            OnLoadedOverride();
        }

        /// <summary>
        /// Runs when the Loaded event is fired and executes
        /// the LoadedBehavior
        /// </summary>
        protected virtual void OnLoadedOverride()
        {
            ExecuteMediaState(LoadedBehavior);
        }

        /// <summary>
        /// Runs when the Unloaded event is fired and executes
        /// the UnloadedBehavior
        /// </summary>
        protected virtual void OnUnloadedOverride()
        {
            ExecuteMediaState(UnloadedBehavior);
        }

        /// <summary>
        /// Executes the actions associated to a MediaState
        /// </summary>
        /// <param name="state">The MediaState to execute</param>
        protected void ExecuteMediaState(MediaState state)
        {
            switch (state)
            {
                case MediaState.Manual:
                    break;
                case MediaState.Play:
                    Play();
                    break;
                case MediaState.Stop:
                    Stop();
                    break;
                case MediaState.Close:
                    Close();
                    break;
                case MediaState.Pause:
                    Pause();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Plays the media
        /// </summary>
        public virtual void Play()
        {
            MediaPlayerBase.Dispatcher.BeginInvoke((Action)(() => MediaPlayerBase.Play()));
        }

        /// <summary>
        /// Pauses the media
        /// </summary>
        public virtual void Pause()
        {
            MediaPlayerBase.Dispatcher.BeginInvoke((Action)(() => MediaPlayerBase.Pause()));
        }

        /// <summary>
        /// Closes the media
        /// </summary>
        public virtual void Close()
        {
            SetBackBuffer(IntPtr.Zero);
            InvalidateVideoImage();
            MediaPlayerBase.Dispatcher.BeginInvoke((Action)(() => MediaPlayerBase.Close()));
        }

        /// <summary>
        /// Stops the media
        /// </summary>
        public virtual void Stop()
        {
            MediaPlayerBase.Dispatcher.BeginInvoke((Action)(() => MediaPlayerBase.Stop()));
        }

        /// <summary>
        /// Called when a MediaPlayerBase is required.
        /// </summary>
        /// <returns>This method must return a valid (not null) MediaPlayerBase</returns>
        protected virtual MediaPlayerBase OnRequestMediaPlayer()
        {
            return null;
        }
    }
}