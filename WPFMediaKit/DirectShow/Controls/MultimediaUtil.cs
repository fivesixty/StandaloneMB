using System;
using System.Collections.Generic;
using WPFMediaKit.DirectShow.Interop;

namespace WPFMediaKit.DirectShow.Controls
{
    public class MultimediaUtil
    {
        #region Audio Renderer Methods
        /// <summary>
        /// The private cache of the audio renderer names
        /// </summary>
        private static string[] m_audioRendererNames;

        /// <summary>
        /// An array of audio renderer device names
        /// on the current system
        /// </summary>
        public static string[] AudioRendererNames
        {
            get
            {
                if(m_audioRendererNames == null)
                {
                    m_audioRendererNames = GetDeviceNames(FilterCategory.AudioRendererCategory);
                }
                return m_audioRendererNames;
            }
        }
        #endregion

        #region Video Input Devices
        /// <summary>
        /// The private cache of the video input names
        /// </summary>
        private static string[] m_videoInputNames;

        /// <summary>
        /// An array of video input device names
        /// on the current system
        /// </summary>
        public static string[] VideoInputNames
        {
            get
            {
                if (m_videoInputNames == null)
                {
                    m_videoInputNames = GetDeviceNames(FilterCategory.VideoInputDevice);
                }
                return m_videoInputNames;
            }
        }

        #endregion

        /// <summary>
        /// Helper method to get all the device names
        /// </summary>
        private static string[] GetDeviceNames(Guid deviceCategory)
        {
            var names = new List<string>();

            /* Queries for all devices on the system that match
             * the category we supply */
            var devices = DsDevice.GetDevicesOfCat(deviceCategory);

            /* Enumerate all the queried devices and extract their names */
            foreach (var device in devices)
            {
                names.Add(device.Name);
            }

            return names.ToArray();
        }
    }
}
