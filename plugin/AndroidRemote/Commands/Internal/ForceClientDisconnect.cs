﻿namespace MusicBeePlugin.AndroidRemote.Commands.Internal
{
    using Interfaces;
    using Networking;

    internal class ForceClientDisconnect:ICommand
    {
        public void Dispose()
        {
            
        }

        public void Execute(IEvent eEvent)
        {
            SocketServer.Instance.KickClient(eEvent.ClientId);
        }
    }
}
