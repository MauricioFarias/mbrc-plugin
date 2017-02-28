﻿using MusicBeePlugin.AndroidRemote.Model.Entities;

namespace MusicBeePlugin.AndroidRemote.Commands.Requests
{
    using Interfaces;
    using Networking;

    internal class RequestPlayer:ICommand
    {
        public void Dispose()
        {
            
        }

        public void Execute(IEvent eEvent)
        {
            SocketServer.Instance.Send(new SocketMessage(Constants.Player, "MusicBee").ToJsonString(), eEvent.ClientId);
        }
    }
}
