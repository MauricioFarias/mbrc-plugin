namespace MusicBeePlugin.AndroidRemote.Commands.Requests
{
    using Interfaces;

    internal class RequestNowPlayingTrackRemoval:ICommand
    {
        public void Dispose()
        {
        }

        public void Execute(IEvent eEvent)
        {
            int index;
            if (int.TryParse(eEvent.DataToString(), out index))
            {
                Plugin.Instance.NowPlayingListRemoveTrack(index, eEvent.ClientId);    
            }
        }
    }
}