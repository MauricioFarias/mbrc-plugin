﻿using TinyMessenger;

namespace MusicBeePlugin.AndroidRemote.Interfaces
{
    public interface IEvent : ITinyMessage
    {
        object Data { get; }
        string Type { get; }
        string ConnectionId { get; }
        string ClientId { get; }
        string DataToString();
    }
}
