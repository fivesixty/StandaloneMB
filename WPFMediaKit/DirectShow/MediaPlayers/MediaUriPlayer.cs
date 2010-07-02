#region Usings
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using WPFMediaKit.DirectShow.Interop;
using System.IO;
#endregion

namespace WPFMediaKit.DirectShow.MediaPlayers
{
    /// <summary>
    /// The MediaUriPlayer plays media files from a given Uri.
    /// </summary>
    public class MediaUriPlayer : MediaSeekingPlayer
    {
        /// <summary>
        /// The name of the default audio render.  This is the
        /// same on all versions of windows
        /// </summary>
        private const string DEFAULT_AUDIO_RENDERER_NAME = "Default DirectSound Device";

        /// <summary>
        /// Set the default audio renderer property backing
        /// </summary>
        private string m_audioRenderer = DEFAULT_AUDIO_RENDERER_NAME;

        // My Own stuff
        public FilterControl Filters = null;

        public int droppedFrames = 0;

#if DEBUG
        /// <summary>
        /// Used to view the graph in graphedit
        /// </summary>
        private DsROTEntry m_dsRotEntry;
#endif

        /// <summary>
        /// The DirectShow graph interface.  In this example
        /// We keep reference to this so we can dispose 
        /// of it later.
        /// </summary>
        private IGraphBuilder m_graph;

        /// <summary>
        /// The VMR9 video renderer
        /// </summary>
        private IBaseFilter m_renderer;

        /// <summary>
        /// The media Uri
        /// </summary>
        private Uri m_sourceUri;

        /// <summary>
        /// Gets or sets the Uri source of the media
        /// </summary>
        public Uri Source
        {
            get
            {
                VerifyAccess();
                return m_sourceUri;
            }
            set
            {
                VerifyAccess();
                m_sourceUri = value;
                OpenSource();
            }
        }

        /// <summary>
        /// The renderer type to use when
        /// rendering video
        /// </summary>
        public VideoRendererType VideoRenderer
        {
            get;set;
        }

        /// <summary>
        /// The name of the audio renderer device
        /// </summary>
        public string AudioRenderer
        {
            get
            {
                VerifyAccess();
                return m_audioRenderer;
            }
            set
            {
                VerifyAccess();

                if(string.IsNullOrEmpty(value))
                {
                    value = DEFAULT_AUDIO_RENDERER_NAME;
                }

                m_audioRenderer = value;
            }
        }

        /// <summary>
        /// Gets or sets if the media should play in loop
        /// or if it should just stop when the media is complete
        /// </summary>
        public bool Loop { get; set; }

        /// <summary>
        /// Is ran everytime a new media event occurs on the graph
        /// </summary>
        /// <param name="code">The Event code that occured</param>
        /// <param name="lparam1">The first event parameter sent by the graph</param>
        /// <param name="lparam2">The second event parameter sent by the graph</param>
        protected override void OnMediaEvent(EventCode code, IntPtr lparam1, IntPtr lparam2)
        {
            if(Loop)
            {
                switch(code)
                {
                    case EventCode.Complete:
                        MediaPosition = 0;
                        break;
                }
            }
            else
                /* Only run the base when we don't loop
                 * otherwise the default behavior is to
                 * fire a media ended event */
                base.OnMediaEvent(code, lparam1, lparam2);
        }

        /// <summary>
        /// Opens the media by initializing the DirectShow graph
        /// </summary>
        protected virtual void OpenSource()
        {
            string fileSource = m_sourceUri.OriginalString;

            /* Make sure we clean up any remaining mess */
            FreeResources();

            try
            {
                /* Creates the GraphBuilder COM object */
                m_graph = new FilterGraph() as IGraphBuilder;

                if (m_graph == null)
                    throw new Exception("Could not create a graph");

                m_renderer = CreateVideoRenderer(VideoRenderer, m_graph);

                /* Add our prefered audio renderer */
                InsertAudioRenderer(AudioRenderer);

                var filterGraph = m_graph as IFilterGraph2;  // Switched this down from IFilterGraph3
                if (filterGraph == null)
                    throw new Exception("Could not QueryInterface for the IFilterGraph3");

                IBaseFilter sourceFilter;

                /* Have DirectShow find the correct source filter for the Uri */
                int hr = filterGraph.AddSourceFilter(fileSource, fileSource, out sourceFilter);
                DsError.ThrowExceptionForHR(hr);

                // Check for subtitles.
                IPin ip;
                sourceFilter.FindPin("Subtitle", out ip);

                if (ip != null)
                {
                    /* Add DirectVobSub to the graph before rendering pins, so it's connected */
                    IBaseFilter dvs;
                    dvs = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(new DsGuid("{93A22E7A-5091-45EF-BA61-6DA26156A5D0}")));
                    m_graph.AddFilter(dvs, "DirectVobSub");
                }

                /* We will want to enum all the pins on the source filter */
                IEnumPins pinEnum;

                hr = sourceFilter.EnumPins(out pinEnum);
                DsError.ThrowExceptionForHR(hr);

                IntPtr fetched = IntPtr.Zero;
                IPin[] pins = { null };

                /* Counter for how many pins successfully rendered */
                int pinsRendered = 0;

                /* Loop over each pin of the source filter */
                while (pinEnum.Next(pins.Length, pins, fetched) == 0)
                {
                    if (filterGraph.RenderEx(pins[0], 
                                             AMRenderExFlags.RenderToExistingRenderers, 
                                             IntPtr.Zero) == 0)
                        pinsRendered++;
                }

                NewAllocatorFrame += new Action(MediaUriPlayer_NewAllocatorFrame);

                Marshal.ReleaseComObject(pinEnum);
                Marshal.ReleaseComObject(sourceFilter);

                if (pinsRendered == 0)
                    throw new Exception("Could not render any streams from the source Uri");

                

                Thread.CurrentThread.Priority = ThreadPriority.Normal;
#if DEBUG
                /* Adds the GB to the ROT so we can view
                 * it in graphedit */
                m_dsRotEntry = new DsROTEntry(m_graph);
#endif
                /* Configure the graph in the base class */
                SetupFilterGraph(m_graph);

                /* Sets the NaturalVideoWidth/Height */
                SetNativePixelSizes(m_renderer);

                // Populate the filters
                Filters = new FilterControl(m_graph);

                /* Remove and dispose of renderer if we do not
                 * have a video stream */
                if (!HasVideo)
                {
                    m_graph.RemoveFilter(m_renderer);

                    /* Tells the base class to unregister and
                     * free the custom allocator */
                    FreeCustomAllocator();

                    Marshal.FinalReleaseComObject(m_renderer);
                    m_renderer = null;
                }
            }
            catch (Exception ex)
            {
                /* This exection will happen usually if the media does
                 * not exist or could not open due to not having the
                 * proper filters installed */
                FreeResources();

                /* Fire our failed event */
                InvokeMediaFailed(new MediaFailedEventArgs(ex.Message, ex));
            }

            InvokeMediaOpened();
        }

        void MediaUriPlayer_NewAllocatorFrame()
        {
            int framesDropped;
            IQualProp quality = m_renderer as IQualProp;
            quality.get_FramesDroppedInRenderer(out framesDropped);
            if (framesDropped != droppedFrames)
            {
                Console.WriteLine("FRAME DROPPED, TOTAL: " + framesDropped.ToString());
                droppedFrames = framesDropped;
            }
        }

        

        /// <summary>
        /// Inserts the audio renderer by the name of
        /// the audio renderer that is passed
        /// </summary>
        protected virtual void InsertAudioRenderer(string audioDeviceName)
        {
            if(m_graph == null)
                return;

            AddFilterByName(m_graph, 
                            FilterCategory.AudioRendererCategory,
                            audioDeviceName);
        }

        /// <summary>
        /// Frees all unmanaged memory and resets the object back
        /// to its initial state
        /// </summary>
        protected override void FreeResources()
        {
            /* We run the StopInternal() to avoid any 
             * Dispatcher VeryifyAccess() issues because
             * this may be called from the GC */
            StopInternal();

            /* Let's clean up the base 
             * class's stuff first */
            base.FreeResources();

#if DEBUG
            /* Remove us from the ROT */
            if(m_dsRotEntry != null)
            {
                m_dsRotEntry.Dispose();
                m_dsRotEntry = null;
            }
#endif
            if(m_renderer != null)
            {
                Marshal.FinalReleaseComObject(m_renderer);
                m_renderer = null;
            }

            if(m_graph != null)
            {
                Marshal.ReleaseComObject(m_graph);
                m_graph = null;

                /* Only run the media closed if we have an
                 * initialized filter graph */
                InvokeMediaClosed(new EventArgs());
            }
        }

        /// <summary>
        /// Creates a new MediaUriPlayer, 
        /// running on it's own Dispatcher
        /// </summary>
        public static MediaUriPlayer CreateMediaUriPlayer()
        {
            MediaUriPlayer player = null;

            var reset = new ManualResetEvent(false);

            var t = new Thread((ThreadStart) delegate
            {
                player = new MediaUriPlayer();

                /* Make our thread name a little unique */
                Thread.CurrentThread.Name = string.Format("MediaUriPlayer Graph {0}", 
                                                          player.GraphInstanceId);

                /* We queue up a method to execute
                 * when the Dispatcher is ran. 
                 * This will wake up the calling thread. */
                player.Dispatcher.Invoke((Action) (() => reset.Set()));

                Dispatcher.Run();
            })
            {
                IsBackground = true
            };

            t.SetApartmentState(ApartmentState.MTA);

            /* Set this to low priority now and we'll
             * set it back to normal later on in hope
             * that our UI thread's priority stays higher */
            t.Priority = ThreadPriority.Lowest;

            /* Starts the thread and creates the object */
            t.Start();

            /* We wait until our object is created and
             * the new Dispatcher is running */
            reset.WaitOne();

            return player;
        }

    }
}