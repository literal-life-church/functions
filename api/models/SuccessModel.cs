﻿using Newtonsoft.Json;

namespace LiteralLifeChurch.LiveStreamingApi.models
{
    public class SuccessModel
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}