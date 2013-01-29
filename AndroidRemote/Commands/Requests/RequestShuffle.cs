﻿namespace MusicBeePlugin.AndroidRemote.Commands.Requests
{
    using MusicBeePlugin.AndroidRemote.Interfaces;
    using MusicBeePlugin.AndroidRemote.Utilities;

    class RequestShuffle: ICommand
    {
        public void Dispose()
        {
        }

        public void Execute(IEvent eEvent)
        {
            Plugin.Instance.RequestShuffleState(eEvent.Data.Equals("toggle")?StateAction.Toggle : StateAction.State);
        }
    }
}
