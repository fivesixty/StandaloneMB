using System;
using System.Windows;
using System.Windows.Threading;
using WPFMediaKit.DirectShow.MediaPlayers;
using System.Collections;

namespace WPFMediaKit.DirectShow.Controls
{
    /// <summary>
    /// The MediaUriElement is a WPF control that plays media of a given
    /// Uri. The Uri can be a file path or a Url to media.  The MediaUriElement
    /// inherits from the MediaSeekingElement, so where available, seeking is
    /// also supported.
    /// </summary>
    public class MediaUriElement : MediaSeekingElement
    {
        /// <summary>
        /// Flag to detect if the source Uri has changed.
        /// We do not set the Uri source on the player on the 
        /// DP PropertyChangedCallback so we can ensure the 
        /// media player is all setup before we open the media
        /// </summary>
        private bool m_sourceChanged;

        /// <summary>
        /// The current MediaUriPlayer
        /// </summary>
        protected MediaUriPlayer MediaUriPlayer
        {
            get
            {
                return MediaPlayerBase as MediaUriPlayer;
            }
        }

        #region VideoRenderer

        public static readonly DependencyProperty VideoRendererProperty =
            DependencyProperty.Register("VideoRenderer", typeof(VideoRendererType), typeof(MediaUriElement),
                new FrameworkPropertyMetadata(VideoRendererType.VideoMixingRenderer9,
                    new PropertyChangedCallback(OnVideoRendererChanged)));

        public VideoRendererType VideoRenderer
        {
            get { return (VideoRendererType)GetValue(VideoRendererProperty); }
            set { SetValue(VideoRendererProperty, value); }
        }

        private static void OnVideoRendererChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MediaUriElement)d).OnVideoRendererChanged(e);
        }

        protected virtual void OnVideoRendererChanged(DependencyPropertyChangedEventArgs e)
        {
            MediaUriPlayer.VideoRenderer = (VideoRendererType)e.NewValue;
        }

        #endregion

        #region AudioRenderer

        public static readonly DependencyProperty AudioRendererProperty =
            DependencyProperty.Register("AudioRenderer", typeof(string), typeof(MediaUriElement),
                new FrameworkPropertyMetadata("Default DirectSound Device",
                    new PropertyChangedCallback(OnAudioRendererChanged)));

        /// <summary>
        /// The name of the audio renderer device to use
        /// </summary>
        public string AudioRenderer
        {
            get { return (string)GetValue(AudioRendererProperty); }
            set { SetValue(AudioRendererProperty, value); }
        }

        private static void OnAudioRendererChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MediaUriElement)d).OnAudioRendererChanged(e);
        }

        protected virtual void OnAudioRendererChanged(DependencyPropertyChangedEventArgs e)
        {
            var audioDevice = (string)e.NewValue;

            MediaUriPlayer.Dispatcher.BeginInvoke((Action) delegate
            {
                /* Sets the audio device to use with the player */
                MediaUriPlayer.AudioRenderer = audioDevice;
            });
        }

        #endregion
        
        #region Source

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(Uri), typeof(MediaUriElement),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnSourceChanged)));

        /// <summary>
        /// The Uri source to the media.  This can be a file path or a
        /// URL source
        /// </summary>
        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MediaUriElement)d).OnSourceChanged(e);
        }

        protected void OnSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            /* Set our flag so we know what to do with
             * the media later on */
            m_sourceChanged = true;

            if(!IsInitialized)
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

        #region Loop

        public static readonly DependencyProperty LoopProperty =
            DependencyProperty.Register("Loop", typeof(bool), typeof(MediaUriElement),
                new FrameworkPropertyMetadata(false,
                    new PropertyChangedCallback(OnLoopChanged)));

        /// <summary>
        /// Gets or sets whether the media should return to the begining
        /// once the end has reached
        /// </summary>
        public bool Loop
        {
            get { return (bool)GetValue(LoopProperty); }
            set { SetValue(LoopProperty, value); }
        }

        private static void OnLoopChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MediaUriElement)d).OnLoopChanged(e);
        }

        protected virtual void OnLoopChanged(DependencyPropertyChangedEventArgs e)
        {
            MediaPlayerBase.Dispatcher.BeginInvoke((Action) delegate
            {
                MediaUriPlayer.Loop = (bool) e.NewValue;
            });
        }

        #endregion

        /// <summary>
        /// Decides if the Uri source should be
        /// changed, if so, it changes it with the player. I know
        /// this method is funky and you think I did it wrong or messy,
        /// but really there was some weird stuff going on and just 
        /// take my work on it.
        /// </summary>
        /// <param name="executedAction">
        /// The method to execute if the
        /// source needs to be changed on the player
        /// </param>
        /// <returns>Returns true if the source change was handled</returns>
        private bool DecideChangeSource(Action executedAction)
        {
            if (m_sourceChanged)
            {
                var source = Source;
                var rendererType = VideoRenderer;

                MediaPlayerBase.Dispatcher.BeginInvoke((Action)delegate
                {
                    /* Set the renderer type */
                    MediaUriPlayer.VideoRenderer = rendererType;

                    /* Set the source type */
                    MediaUriPlayer.Source = source;

                    /* Execute our executed action delegate */
                    Dispatcher.BeginInvoke(executedAction);
                }, DispatcherPriority.Send);
                m_sourceChanged = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// The Play method is overrided so we can
        /// set the source to the media
        /// </summary>
        public override void Play()
        {
            if (DecideChangeSource(base.Play))
                return;

            base.Play();
        }

        /// <summary>
        /// The Pause method is overrided so we can
        /// set the source to the media
        /// </summary>
        public override void Pause()
        {
            if (DecideChangeSource(base.Pause))
                return;

            base.Pause();
        }

        /// <summary>
        /// Gets the instance of the media player to initialize
        /// our base classes with
        /// </summary>
        protected override MediaPlayerBase OnRequestMediaPlayer()
        {
            return MediaUriPlayer.CreateMediaUriPlayer();
        }

        public FilterControl Filters
        {
            get
            {
                return MediaUriPlayer.Filters;
            }
        }

    }
}
