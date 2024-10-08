﻿using Newtonsoft.Json;

namespace TheMarauderMap.Entities
{
    public class Session
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "uid")]
        public string SessionId { get; set; }

        public string Accesstoken { get; set; }

        public string UserId { get; set; }

        public DateTimeOffset CreationTime { get; set; }

        public DateTimeOffset ExpiryTime { get; set; }

        public override string ToString()
        {
            return $"{Id} : {SessionId} : {Accesstoken} : {UserId} : {CreationTime} : {ExpiryTime}";
        }
    }
}
