﻿using MusicBeeRemoteCore.Remote.Enumerations;

namespace MusicBeeRemoteCore.Core.ApiAdapters
{
    public interface IPlayerStateAdapter
    {
        ShuffleState GetShuffleState();

        Repeat GetRepeatMode();

        bool ScrobblingEnabled();

        bool PlayNext();

        bool PlayPrevious();

        bool StopPlayback();

        bool PlayPause();
    }
}