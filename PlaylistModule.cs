using System;
using System.Collections.Generic;
using System.Linq;
using MusicBeePlugin.AndroidRemote.Data;
using MusicBeePlugin.AndroidRemote.Entities;
using MusicBeePlugin.AndroidRemote.Enumerations;
using MusicBeePlugin.AndroidRemote.Networking;
using MusicBeePlugin.AndroidRemote.Utilities;

namespace MusicBeePlugin
{
    public class PlaylistModule : Messenger
    {
        private Plugin.MusicBeeApiInterface api;
        private String mStoragePath;

        public PlaylistModule(Plugin.MusicBeeApiInterface api, string mStoragePath)
        {
            this.api = api;
            this.mStoragePath = mStoragePath;
        }

        /// <summary>
        /// The function checks the MusicBee api and gets all the available playlist urls.
        /// </summary>
        public void GetAvailablePlaylists()
        {
            api.Playlist_QueryPlaylists();
            string path;
            var availablePlaylists = new List<Playlist>();
            var ch = new CacheHelper(mStoragePath);
            while (true)
            {
                string[] files = { };
                path = api.Playlist_QueryGetNextPlaylist();
                if (String.IsNullOrEmpty(path)) break;
                var name = api.Playlist_GetName(path);
                api.Playlist_QueryFilesEx(path, ref files);
                var playlist = new Playlist(name, files.Count(), Utilities.Sha1Hash(path), path);
                availablePlaylists.Add(playlist);
                ch.CachePlaylists(availablePlaylists);
            }

            SendSocketMessage(Constants.PlaylistList, Constants.Reply, availablePlaylists);
        }

        /// <summary>
        /// Given the url of a playlist and the id of a client the method sends a message to the specified client
        /// including the tracks in the specified playlist.
        /// </summary>
        /// <param name="hash">sha1 hash identifying the playlist</param>
        /// <param name="clientId">The id of the client</param>
        public void GetTracksForPlaylist(string hash, string clientId)
        {

            string[] trackUrlList = {};
            var ch = new CacheHelper(mStoragePath);
            var playlist = ch.GetPlaylistByHash(hash);

            if (!api.Playlist_QueryFilesEx(playlist.path, ref trackUrlList))
            {
                return;
            }

            List<Track> playlistTracks = new List<Track>();

            foreach (string trackUrl in trackUrlList)
            {
                string artist = api.Library_GetFileTag(trackUrl, Plugin.MetaDataType.Artist);
                string track = api.Library_GetFileTag(trackUrl, Plugin.MetaDataType.TrackTitle);
                Track curTrack = new Track(artist, track, trackUrl);
                playlistTracks.Add(curTrack);
            }

            SendSocketMessage(Constants.PlaylistGetFiles, Constants.Reply, playlistTracks);
        }

        /// <summary>
        /// Given the url of a playlist it plays the specified playlist.
        /// </summary>
        /// <param name="url">The playlist url</param>
        public void RequestPlaylistPlayNow(string url)
        {
            SendSocketMessage(Constants.PlaylistPlayNow, Constants.Reply, api.Playlist_PlayNow(url));
        }

        /// <summary>
        /// Given the url of the playlist and the index of a index it removes the specified index,
        /// from the playlist.
        /// </summary>
        /// <param name="url">The url of th playlist</param>
        /// <param name="index">The index of the index to remove</param>
        public void RequestPlaylistTrackRemove(string url,int index)
        {
            bool success = api.Playlist_RemoveAt(url, index);
            SendSocketMessage(Constants.PlaylistRemove, Constants.Reply, success);
        }

        public void RequestPlaylistCreate(string client, string name, MetaTag tag, string query, string[] files)
        {
            if (tag != MetaTag.title)
            {
                //files = _plugin.GetUrlsForTag(tag, query);
            }
            string url = api.Playlist_CreatePlaylist(String.Empty, name, files);
            SendSocketMessage(Constants.PlaylistCreate, Constants.Reply, url);
        }

        public void RequestPlaylistMove(string clientId,string src, int from, int to)
        {
            bool success;
            int[] aFrom = { @from };
            int dIn;
            if (@from > to)
            {
                dIn = to - 1;
            }
            else
            {
                dIn = to;
            }

            success = api.Playlist_MoveFiles(src, aFrom, dIn);

            var reply = new
            {
                success,
                @from,
                to
            };

            SendSocketMessage(Constants.PlaylistMove, Constants.Reply, reply, clientId);
        }
    }
}