using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using WPFMediaKit.DirectShow.Interop;

namespace WPFMediaKit.DirectShow.MediaPlayers
{
    /// <summary>
    /// A Player that plays video from a video capture device.
    /// </summary>
    public class VideoCapturePlayer : MediaPlayerBase
    {
        /// <summary>
        /// The video capture pixel height
        /// </summary>
        private int m_desiredHeight = 240;

        /// <summary>
        /// The video capture pixel width
        /// </summary>
        private int m_desiredWidth = 320;

        /// <summary>
        /// The video capture's frames per second
        /// </summary>
        private int m_fps = 30;

        /// <summary>
        /// Our DirectShow filter graph
        /// </summary>
        private IGraphBuilder m_graph;

        /// <summary>
        /// The DirectShow video renderer
        /// </summary>
        private IBaseFilter m_renderer;

        /// <summary>
        /// The capture device filter
        /// </summary>
        private IBaseFilter m_captureDevice;

        /// <summary>
        /// The name of the video capture source device
        /// </summary>
        private string m_videoCaptureSource;

        /// <summary>
        /// The name of the video capture source to use
        /// </summary>
        public string VideoCaptureSource
        {
            get
            {
                VerifyAccess();
                return m_videoCaptureSource;
            }
            set
            {
                VerifyAccess();
                m_videoCaptureSource = value;

                /* Free our unmanaged resources when
                 * the source changes */
                FreeResources();
            }
        }

        /// <summary>
        /// The frames per-second to play
        /// the capture device back at
        /// </summary>
        public int FPS
        {
            get
            {
                VerifyAccess();
                return m_fps;
            }
            set
            {
                VerifyAccess();

                /* We support only a minimum of
                 * one frame per second */
                if (value < 1)
                    value = 1;

                m_fps = value;
            }
        }

        /// <summary>
        /// The desired pixel width of the video
        /// </summary>
        public int DesiredWidth
        {
            get
            {
                VerifyAccess();
                return m_desiredWidth;
            }
            set
            {
                VerifyAccess();
                m_desiredWidth = value;
            }
        }

        /// <summary>
        /// The desired pixel height of the video
        /// </summary>
        public int DesiredHeight
        {
            get
            {
                VerifyAccess();
                return m_desiredHeight;
            }
            set
            {
                VerifyAccess();
                m_desiredHeight = value;
            }
        }

        /// <summary>
        /// Plays the video capture device
        /// </summary>
        public override void Play()
        {
            VerifyAccess();

            if(m_graph == null)
                SetupGraph();

            base.Play();
        }

        /// <summary>
        /// Pauses the video capture device
        /// </summary>
        public override void Pause()
        {
            VerifyAccess();

            if(m_graph == null)
                SetupGraph();

            base.Pause();
        }

        /// <summary>
        /// Configures the DirectShow graph to play the selected video capture
        /// device with the selected parameters
        /// </summary>
        private void SetupGraph()
        {
            /* Clean up any messes left behind */
            FreeResources();

            try
            {
                /* Create a new graph */
                m_graph = (IGraphBuilder)new FilterGraph();

                /* Create a capture graph builder to help 
                 * with rendering a capture graph */
                var captureGraph = (ICaptureGraphBuilder2) new CaptureGraphBuilder2();

                /* Set our filter graph to the capture graph */
                int hr = captureGraph.SetFiltergraph(m_graph);
                DsError.ThrowExceptionForHR(hr);

                /* Add our capture device source to the graph */
                m_captureDevice = AddFilterByName(m_graph, 
                                                  FilterCategory.VideoInputDevice,
                                                  VideoCaptureSource);

                /* If we have a null capture device, we have an issue */
                if (m_captureDevice == null)
                    throw new Exception("Capture device " + VideoCaptureSource + " not found or could not be created");

                /* Configure the video output pin with our paramters */
                SetVideoCaptureParameters(captureGraph, m_captureDevice);

                /* Creates a video renderer and register the allocator with the base class */
                m_renderer = CreateVideoRenderer(VideoRendererType.VideoMixingRenderer9, m_graph);

                /* Intelligently connect the pins in the graph to the renderer */
                hr = captureGraph.RenderStream(PinCategory.Capture,
                                               MediaType.Video,
                                               m_captureDevice,
                                               null,
                                               m_renderer);

                DsError.ThrowExceptionForHR(hr);

                /* Register the filter graph 
                 * with the base classes */
                SetupFilterGraph(m_graph);

                /* Sets the NaturalVideoWidth/Height */
                SetNativePixelSizes(m_renderer);

                HasVideo = true;

                /* Make sure we Release() this COM reference */
                Marshal.ReleaseComObject(captureGraph);
            }
            catch(Exception ex)
            {
                /* Something got fuct up */
                FreeResources();
                InvokeMediaFailed(new MediaFailedEventArgs(ex.Message, ex));
            }

            /* Success */
            InvokeMediaOpened();
        }

        /// <summary>
        /// Sets the capture parameters for the video capture device
        /// </summary>
        private void SetVideoCaptureParameters(ICaptureGraphBuilder2 capGraph, IBaseFilter captureFilter)
        {
            /* The stream config interface */
            object streamConfig;

            /* Get the stream's configuration interface */
            int hr = capGraph.FindInterface(PinCategory.Capture, 
                                            MediaType.Video, 
                                            captureFilter, 
                                            typeof(IAMStreamConfig).GUID, 
                                            out streamConfig);

            DsError.ThrowExceptionForHR(hr);

            var videoStreamConfig = streamConfig as IAMStreamConfig;

            /* If QueryInterface fails... */
            if(videoStreamConfig == null)
            {
                throw new Exception("Failed to get IAMStreamConfig");
            }

            /* The media type of the video */
            AMMediaType media;

            /* Get the AMMediaType for the video out pin */
            hr = videoStreamConfig.GetFormat(out media);
            DsError.ThrowExceptionForHR(hr);

            /* Make the VIDEOINFOHEADER 'readable' */
            var videoInfo = new VideoInfoHeader();
            Marshal.PtrToStructure(media.formatPtr, videoInfo);

            /* Setup the VIDEOINFOHEADER with the parameters we want */
            videoInfo.AvgTimePerFrame = DSHOW_ONE_SECOND_UNIT/FPS;
            videoInfo.BmiHeader.Width = DesiredWidth;
            videoInfo.BmiHeader.Height = DesiredHeight;

            /* Copy the data back to unmanaged memory */
            Marshal.StructureToPtr(videoInfo, media.formatPtr, false);

            /* Set the format */
            hr = videoStreamConfig.SetFormat(media);

            /* We don't want any memory leaks, do we? */
            DsUtils.FreeAMMediaType(media);

            /* Wait to free AMMediaType before we throw any errors */
            DsError.ThrowExceptionForHR(hr);
        }

        protected override void FreeResources()
        {
            /* We run the StopInternal() to avoid any 
             * Dispatcher VeryifyAccess() issues */
            StopInternal();

            /* Let's clean up the base 
             * class's stuff first */
            base.FreeResources();

            if(m_renderer != null)
            {
                Marshal.FinalReleaseComObject(m_renderer);
                m_renderer = null;
            }
            if (m_captureDevice != null)
            {
                Marshal.FinalReleaseComObject(m_captureDevice);
                m_captureDevice = null;
            }
            if(m_graph != null)
            {
                Marshal.FinalReleaseComObject(m_graph);
                m_graph = null;

                InvokeMediaClosed(new EventArgs());
            }
        }

        /// <summary>
        /// Creates a new VideoCapturePlayer running on it's own Dispatcher
        /// </summary>
        public static VideoCapturePlayer CreateMediaUriPlayer()
        {
            VideoCapturePlayer player = null;

            /* The reset event will block our thread while
             * we create an intialize the player */
            var reset = new ManualResetEvent(false);

            /* We need to create a new thread for our Dispatcher */
            var t = new Thread((ThreadStart)delegate
            {
                player = new VideoCapturePlayer();
                
                /* Make our thread name a little unique */
                Thread.CurrentThread.Name = string.Format("MediaUriPlayer Graph {0}",
                                                          player.GraphInstanceId);
                /* We queue up a method to execute
                 * when the Dispatcher is ran. 
                 * This will wake up the calling thread
                 * that has been blocked by the reset event */
                player.Dispatcher.Invoke((Action)(() => reset.Set()));

                Dispatcher.Run();
            })
            {
                IsBackground = true
            };

            t.SetApartmentState(ApartmentState.MTA);

            /* Starts the thread and creates the object */
            t.Start();

            /* We wait until our object is created and
             * the new Dispatcher is running */
            reset.WaitOne();

            return player;
        }
    }
}