using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCameraSettings.Models
{
    public class StreamSettings
    {
        [JsonProperty("resolution")]
        public string Resolution { get; set; }

        [JsonProperty("fps")]
        public int FPS { get; set; }

        [JsonProperty("bitrate")]
        public int Bitrate { get; set; }

        [JsonProperty("bitrate_mode")]
        public string BitrateMode { get; set; }

        [JsonProperty("custom_bitrate")]
        public int CustomBitrate { get; set; }

        [JsonProperty("video_encode_type")]
        public string VideoEncodeType { get; set; }

        [JsonProperty("video_encode_level")]
        public string VideoEncodeLevel { get; set; }

        [JsonProperty("bitrate_control")]
        public string BitrateControl { get; set; }

        [JsonProperty("video_quality")]
        public string VideoQuality { get; set; }

        [JsonProperty("audio")]
        public bool Audio { get; set; }

        [JsonProperty("i_frame_interval")]
        public int IFrameInterval { get; set; }
    }
}
