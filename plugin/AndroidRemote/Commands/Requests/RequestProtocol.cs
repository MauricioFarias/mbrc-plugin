﻿
using MusicBeePlugin.AndroidRemote.Entities;
using MusicBeePlugin.AndroidRemote.Interfaces;
using MusicBeePlugin.AndroidRemote.Networking;
using MusicBeePlugin.AndroidRemote.Utilities;

namespace MusicBeePlugin.AndroidRemote.Commands.Requests
{
    class RequestProtocol:ICommand
    {
        public void Dispose()
        {
            
        }

        public void Execute(IEvent eEvent)
        {
            float clientProtocolVersion;
            if (float.TryParse(eEvent.DataToString(), out clientProtocolVersion))
            {
                var client = Authenticator.Client(eEvent.ClientId);
                if (client != null)
                {
                    client.ClientProtocolVersion = clientProtocolVersion;
                }
            }
            SocketServer.Instance.Send(new SocketMessage(Constants.Protocol, Constants.Reply, Constants.ProtocolVersion).toJsonString(), eEvent.ClientId); 
        }
    }
}
