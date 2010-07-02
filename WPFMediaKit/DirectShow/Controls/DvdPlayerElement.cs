#region Usings
using System;
using System.Windows;
using System.Windows.Input;
using WPFMediaKit.DirectShow.MediaPlayers;
#endregion

namespace WPFMediaKit.DirectShow.Controls
{
    public class DvdPlayerElement : MediaSeekingElement
    {
        #region Local Instances
        /// <summary>
        /// Flag to detect if the source DVD directory has changed.
        /// We do not set the dvd directory on the player on the 
        /// DP PropertyChangedCallback so we can ensure the 
        /// dvd player is all setup before we open the dvd
        /// </summary>
        private bool m_dvdDirectoryChanged;
        #endregion

        public DvdPlayerElement()
        {
            RenderOnCompositionTargetRendering = true;
        }

        /// <summary>
        /// Fires when a DVD specific error occurs
        /// </summary>
        public event EventHandler<DVDErrorArgs> DvdError;

        private void InvokeDvdError(DvdError error)
        {
            var e = new DVDErrorArgs{Error = error};
            var dvdErrorHandler = DvdError;
            if(dvdErrorHandler != null) dvdErrorHandler(this, e);
        }

        #region Dependency Properties
        #region IsOverDvdButton

        private static readonly DependencyPropertyKey IsOverDvdButtonPropertyKey
            = DependencyProperty.RegisterReadOnly("IsOverDvdButton", typeof(bool), typeof(DvdPlayerElement),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsOverDvdButtonProperty
            = IsOverDvdButtonPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets if the mouse is over a DVD button.  This is a dependency property.
        /// </summary>
        public bool IsOverDvdButton
        {
            get { return (bool)GetValue(IsOverDvdButtonProperty); }
        }

        protected void SetIsOverDvdButton(bool value)
        {
            SetValue(IsOverDvdButtonPropertyKey, value);
        }

        #endregion

        #region PlayOnInsert

        public static readonly DependencyProperty PlayOnInsertProperty =
            DependencyProperty.Register("PlayOnInsert", typeof(bool), typeof(DvdPlayerElement),
                                        new FrameworkPropertyMetadata(true,
                                                                      new PropertyChangedCallback(OnPlayOnInsertChanged)));
        /// <summary>
        /// Gets or sets if the DVD automatically plays when a DVD is inserted into the computer.
        /// This is a dependency property.
        /// </summary>
        public bool PlayOnInsert
        {
            get { return (bool)GetValue(PlayOnInsertProperty); }
            set { SetValue(PlayOnInsertProperty, value); }
        }

        private static void OnPlayOnInsertChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DvdPlayerElement)d).OnPlayOnInsertChanged(e);
        }

        protected virtual void OnPlayOnInsertChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region DvdEjected

        public static readonly RoutedEvent DvdEjectedEvent = EventManager.RegisterRoutedEvent("DvdEjected",
                                                                                              RoutingStrategy.Bubble,
                                                                                              typeof(RoutedEventHandler),
                                                                                              typeof(DvdPlayerElement));
        /// <summary>
        /// This event is fired when a DVD is ejected from the computer.  This is a bubbled, routed event.
        /// </summary>
        public event RoutedEventHandler DvdEjected
        {
            add { AddHandler(DvdEjectedEvent, value); }
            remove { RemoveHandler(DvdEjectedEvent, value); }
        }

        #endregion

        #region DvdInserted

        public static readonly RoutedEvent DvdInsertedEvent = EventManager.RegisterRoutedEvent("DvdInserted",
                                                                                               RoutingStrategy.Bubble,
                                                                                               typeof(RoutedEventHandler),
                                                                                               typeof(DvdPlayerElement));
        /// <summary>
        /// Fires when a DVD is inserted into the computer.
        /// This is a bubbled, routed event.
        /// </summary>
        public event RoutedEventHandler DvdInserted
        {
            add { AddHandler(DvdInsertedEvent, value); }
            remove { RemoveHandler(DvdInsertedEvent, value); }
        }

        #endregion

        #region CurrentDvdTime

        private static readonly DependencyPropertyKey CurrentDvdTimePropertyKey
            = DependencyProperty.RegisterReadOnly("CurrentDvdTime", typeof(TimeSpan), typeof(DvdPlayerElement),
                new FrameworkPropertyMetadata(TimeSpan.Zero));

        public static readonly DependencyProperty CurrentDvdTimeProperty
            = CurrentDvdTimePropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the current time the DVD playback is at.  This is a read-only,
        /// dependency property.
        /// </summary>
        public TimeSpan CurrentDvdTime
        {
            get { return (TimeSpan)GetValue(CurrentDvdTimeProperty); }
        }

        protected void SetCurrentDvdTime(TimeSpan value)
        {
            SetValue(CurrentDvdTimePropertyKey, value);
        }

        #endregion

        #region DvdDirectory

        public static readonly DependencyProperty DvdDirectoryProperty =
            DependencyProperty.Register("DvdDirectory", typeof(string), typeof(DvdPlayerElement),
                new FrameworkPropertyMetadata("",
                    new PropertyChangedCallback(OnDvdDirectoryChanged)));

        /// <summary>
        /// Gets or sets the directory the DVD is located at (ie D:\VIDEO_TS).  If this is empty or null,
        /// then DirectShow will try to play the first DVD found in the computer.
        /// This is a dependency property.
        /// </summary>
        public string DvdDirectory
        {
            get { return (string)GetValue(DvdDirectoryProperty); }
            set { SetValue(DvdDirectoryProperty, value); }
        }

        private static void OnDvdDirectoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DvdPlayerElement)d).OnDvdDirectoryChanged(e);
        }
        
        protected virtual void OnDvdDirectoryChanged(DependencyPropertyChangedEventArgs e)
        {
            /* Set our flag so we know what to do with
            * the media later on */
            m_dvdDirectoryChanged = true;

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
        #endregion

        #region Public Methods 
        /// <summary>
        /// The SelectAngle method sets the new angle when the DVD Navigator is in an angle block
        /// </summary>
        /// <param name="angle">Value of the new angle, which must be from 1 through 9</param>
        public void SelectAngle(int angle)
        {
            DvdPlayer.Dispatcher.BeginInvoke((Action)(() => DvdPlayer.SelectAngle(angle)));
        }

        /// <summary>
        /// Returns the display from a submenu to its parent menu.
        /// </summary>
        public void ReturnFromSubmenu()
        {
            DvdPlayer.Dispatcher.BeginInvoke((Action)(() => DvdPlayer.ReturnFromSubmenu()));
        }

        /// <summary>
        /// Selects the specified relative button (upper, lower, right, left)
        /// </summary>
        /// <param name="button">Value indicating the button to select</param>
        public void SelectRelativeButton(DvdRelativeButtonEnum button)
        {
            DvdPlayer.Dispatcher.BeginInvoke((Action)(() => DvdPlayer.SelectRelativeButton(button)));
        }

        /// <summary>
        /// Leaves a menu and resumes playback.
        /// </summary>
        public void Resume()
        {
            DvdPlayer.Dispatcher.BeginInvoke((Action)(() => DvdPlayer.Resume()));
        }

        /// <summary>
        /// Plays the DVD forward at a specific speed
        /// </summary>
        /// <param name="speed">The speed multiplier to play back.</param>
        public void PlayForwards(double speed)
        {
            DvdPlayer.Dispatcher.BeginInvoke((Action)(() => DvdPlayer.PlayForwards(speed)));
        }

        /// <summary>
        /// Plays the DVD backwards at a specific speed
        /// </summary>
        /// <param name="speed">The speed multiplier to play back</param>
        public void PlayBackwards(double speed)
        {
            DvdPlayer.Dispatcher.BeginInvoke((Action)(() => DvdPlayer.PlayBackwards(speed)));
        }

        /// <summary>
        /// Play a title
        /// </summary>
        /// <param name="titleIndex">The index of the title to play back</param>
        public void PlayTitle(int titleIndex)
        {
            DvdPlayer.Dispatcher.BeginInvoke((Action)(() => DvdPlayer.PlayTitle(titleIndex)));
        }

        /// <summary>
        /// Plays the next chapter in the volume.
        /// </summary>
        public void PlayNextChapter()
        {
            DvdPlayer.Dispatcher.BeginInvoke((Action)(() => DvdPlayer.PlayNextChapter()));
        }

        /// <summary>
        /// Plays the previous chapter in the volume.
        /// </summary>
        public void PlayPreviousChapter()
        {
            DvdPlayer.Dispatcher.BeginInvoke((Action)(() => DvdPlayer.PlayPreviousChapter()));
        }

        /// <summary>
        /// Goes to the root menu of the DVD.
        /// </summary>
        public void GotoRootMenu()
        {
            DvdPlayer.Dispatcher.BeginInvoke((Action)(() => DvdPlayer.GotoRootMenu()));
        }

        /// <summary>
        /// Goes to the title menu of the DVD
        /// </summary>
        public void GotoTitleMenu()
        {
            DvdPlayer.Dispatcher.BeginInvoke((Action)(() => DvdPlayer.GotoTitleMenu()));
        }

        /// <summary>
        /// The Play method is overrided so we can
        /// set the source to the media
        /// </summary>
        public override void Play()
        {
            DecideDVDDirectoryChange();
            base.Play();
        }

        /// <summary>
        /// The Pause method is overrided so we can
        /// set the source to the media
        /// </summary>
        public override void Pause()
        {
            DecideDVDDirectoryChange();
            base.Pause();
        }
        #endregion

        #region Protected Methods
        protected DvdPlayer DvdPlayer
        {
            get { return MediaPlayerBase as DvdPlayer; }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            /* Get the position of the mouse over the video image */
            Point position = e.GetPosition(VideoImage);

            /* Calculate the ratio of where the mouse is, to the actual width of the video. */
            double widthMultiplier = position.X/VideoImage.ActualWidth;

            /* Calculate the ratio of where the mouse is, to the actual height of the video */
            double heightMultiplier = position.Y/VideoImage.ActualHeight;

            DvdPlayer.Dispatcher.BeginInvoke((Action)(() => DvdPlayer.SelectAtPosition(widthMultiplier, heightMultiplier)));
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(VideoImage);

            /* Calculate the ratio of where the mouse is, to the actual width of the video. */
            double widthMultiplier = position.X / VideoImage.ActualWidth;

            /* Calculate the ratio of where the mouse is, to the actual height of the video */
            double heightMultiplier = position.Y / VideoImage.ActualHeight;

            DvdPlayer.Dispatcher.BeginInvoke((Action)(() => DvdPlayer.ActivateAtPosition(widthMultiplier, heightMultiplier)));

            base.OnMouseLeftButtonDown(e);
        }

        protected override MediaPlayerBase OnRequestMediaPlayer()
        {
            /* Initialize the DVD player and hook into it's events */
            DvdPlayer player = DvdPlayer.CreateDvdPlayer();
            player.OnDvdEjected += DvdPlayer_OnDvdEjected;
            player.OnDvdInserted += DvdPlayer_OnDvdInserted;
            player.OnOverDvdButton += DvdPlayer_OnOverDvdButton;
            player.OnDvdTime += DvdPlayer_OnDvdTime;
            player.OnDvdError += DvdPlayer_OnDvdError;
            return player;
        }

        #endregion

        #region Private Methods

        private void DvdPlayer_OnDvdError(object sender, DVDErrorArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => InvokeDvdError(e.Error)));
        }

        /// <summary>
        /// The handler for when a new DVD is hit.  The event is fired by the DVDPlayer class.
        /// </summary>
        private void DvdPlayer_OnDvdTime(object sender, DVDTimeEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => SetCurrentDvdTime(e.DvdTime)));
        }

        /// <summary>
        /// The handler for when the mouse is over a DVD button.  This event is fired by the DVD Player class.
        /// </summary>
        private void DvdPlayer_OnOverDvdButton(object sender, OverDvdButtonEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => SetIsOverDvdButton(e.IsOverDvdButton)));
        }

        /// <summary>
        /// Fires when a new DVD is inserted into a DVD player on the computer.
        /// </summary>
        private void DvdPlayer_OnDvdInserted(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                RaiseEvent(new RoutedEventArgs(DvdInsertedEvent));

                if (PlayOnInsert)
                    Play();
            });
        }

        /// <summary>
        /// Fires when the DVD is ejected from the computer.
        /// </summary>
        private void DvdPlayer_OnDvdEjected(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => RaiseEvent(new RoutedEventArgs(DvdEjectedEvent))));
        }

        /// <summary>
        /// Decides if the DVD directory should be
        /// changed, if so, it changes it with the player.
        /// </summary>
        private void DecideDVDDirectoryChange()
        {
            if(m_dvdDirectoryChanged)
            {
                string dvdDirectory = DvdDirectory;
                MediaPlayerBase.Dispatcher.BeginInvoke((Action)delegate
                {
                    DvdPlayer.DvdDirectory = dvdDirectory;
                });
                m_dvdDirectoryChanged = false;
            }
        }
        #endregion
    }
}