﻿namespace MusicBeeRemoteData.Repository.Interfaces
{
    using System.Collections.Generic;

    using MusicBeeRemoteData.Entities;

    /// <summary>
    /// The TrackRepository interface.
    /// </summary>
    public interface ITrackRepository : IRepository<LibraryTrack>
    {
        /// <summary>
        /// Gets the tracks for an album by the album id.
        /// </summary>
        /// <param name="id">
        /// The id of the album.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/> of tracks with the specified album id.
        /// </returns>
        IList<LibraryTrack> GetTracksByAlbumId(long id);

        string GetFirstAlbumTrackPathById(long id);

        string[] GetTrackPathsByArtistId(long id);

        string[] GetTrackPathsByGenreId(long id);
    }
}