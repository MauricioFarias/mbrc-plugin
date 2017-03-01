﻿using System.Runtime.Serialization;
using ServiceStack.Text;

namespace MusicBeePlugin.AndroidRemote.Model.Entities
{
    [DataContract]
    public class SocketMessage
    {
        public SocketMessage(string context, object data)
        {
            Context = context;
            Data = data;
        }

        public SocketMessage(JsonObject jsonObject)
        {
            Context = jsonObject.Get("context");

            var messageData = jsonObject.Get("data");
            if (messageData == null)
            {
                Data = "";
            }
            else
            {
                if (messageData.Contains("{") && messageData.Contains("}"))
                {
                    Data = jsonObject.Object("data");
                }
                else
                {
                    Data = messageData;
                }
            }
        }

        public SocketMessage()
        {
            
        }

        [DataMember(Name = "context")]
        public string Context { get; set; }

        [DataMember(Name = "data")]
        public object Data { get; set; }

        public string ToJsonString()
        {
            return JsonSerializer.SerializeToString(this);
        }

        public override string ToString()
        {
            return JsonSerializer.SerializeToString(this);
        }
    }
}