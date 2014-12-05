#region

using MusicBeePlugin.AndroidRemote.Data;
using MusicBeePlugin.AndroidRemote.Utilities;
using MusicBeePlugin.Rest.ServiceModel.Type;
using NLog;
using ServiceStack.Common.Web;
using ServiceStack.OrmLite;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

#endregion

namespace MusicBeePlugin.Modules
{
    /// <summary>
    ///     Class SyncModule.
    ///     Hosts the functionality responsible for the library sync operations.
    /// </summary>
    public class LibraryModule
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Plugin.MusicBeeApiInterface _api;
        private readonly CacheHelper _cHelper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LibraryModule" /> class.
        /// </summary>
        public LibraryModule(Plugin.MusicBeeApiInterface api, CacheHelper cHelper)
        {
            _api = api;
            _cHelper = cHelper;
        }

        /// <summary>
        ///     Checks for changes in the library and updates the cache.
        /// </summary>
        /// <param name="cachedFiles">The cached files.</param>
        /// <param name="lastSync">The last synchronization date.</param>
        public void SyncCheckForChanges(string[] cachedFiles, DateTime lastSync)
        {
            string[] newFiles = { };
            string[] deletedFiles = { };
            string[] updatedFiles = { };

            _api.Library_GetSyncDelta(cachedFiles, lastSync, Plugin.LibraryCategory.Music,
                ref newFiles, ref updatedFiles, ref deletedFiles);
        }


        public PaginatedResponse GetAllCovers(int offset, int limit)
        {
            using (var db = _cHelper.GetDbConnection())
            {
                var covers = db.Select<LibraryCover>();
                var paginated = PaginatedResponse.GetPaginatedData(limit, offset, covers);
                return paginated;
            }
        }

        public LibraryCover GetLibraryCover(int id, bool includeImage = false)
        {
            try
            {
                using (var db = _cHelper.GetDbConnection())
                {
                    var cover = db.GetById<LibraryCover>(id);
                    return cover;
                }
            }
            catch (Exception)
            {
                throw HttpError.NotFound("Cover resource with id {0} does not exist".Fmt(id));
            }
        }

        /// <summary>
        ///     Builds the cache. Creates an association of SHA1 hashes and file paths on the local
        ///     filesystem and then updates the internal SQLite database.
        /// </summary>
        public void BuildCache()
        {
            string[] files = { };
            _api.Library_QueryFilesEx(String.Empty, ref files);
            using (var db = _cHelper.GetDbConnection())
            using (var trans = db.OpenTransaction())
            {
                db.SaveAll(GetArtistData());
                db.SaveAll(GetGenreData());
                db.SaveAll(GetAlbumData());
                var artists = db.Select<LibraryArtist>();
                var genres = db.Select<LibraryGenre>();
                var albums = db.Select<LibraryAlbum>();
                foreach (var file in files)
                {
                    Plugin.MetaDataType[] types =
                    {
                        Plugin.MetaDataType.Artist,
                        Plugin.MetaDataType.AlbumArtist,
                        Plugin.MetaDataType.Album,
                        Plugin.MetaDataType.Genre,
                        Plugin.MetaDataType.TrackTitle,
                        Plugin.MetaDataType.Year,
                        Plugin.MetaDataType.TrackNo
                    };

                    var i = 0;
                    string[] tags = { };
                    _api.Library_GetFileTags(file, types, ref tags);

                    var artist = tags[i++];
                    var albumArtist = tags[i++];
                    var album = tags[i++];
                    var genre = tags[i++];
                    var title = tags[i++];
                    var year = tags[i++];
                    var trackNo = tags[i];

                    int iTrack;
                    int.TryParse(trackNo, out iTrack);

                    var oGenre = genres.SingleOrDefault(q => q.Name == genre);
                    var oArtist = artists.SingleOrDefault(q => q.Name == artist);
                    var oAlbumArtist = artists.SingleOrDefault(q => q.Name == albumArtist);
                    var oAlbum = albums.SingleOrDefault(q => q.Name == album);

                    if (oAlbum != null && oAlbumArtist != null)
                    {
                        oAlbum.ArtistId = oAlbumArtist.Id;
                    }

                    var track = new LibraryTrack
                    {
                        Title = title,
                        Year = year,
                        Index = iTrack,
                        GenreId = oGenre != null ? oGenre.Id : -1,
                        AlbumArtistId = oAlbumArtist != null ? oAlbumArtist.Id : -1,
                        ArtistId = oArtist != null ? oArtist.Id : -1,
                        AlbumId = oAlbum != null ? oAlbum.Id : -1,
                        Path = file
                    };
                    db.Save(track);
                }
                db.UpdateAll(albums);
                trans.Commit();
            }
        }

        private IEnumerable<LibraryArtist> GetArtistData()
        {
            var list = new List<LibraryArtist>();
            if (_api.Library_QueryLookupTable("artist", "count", null))
            {
                list.AddRange(
                    _api.Library_QueryGetLookupTableValue(null)
                        .Split(new[] { "\0\0" }, StringSplitOptions.None)
                        .Select(artist => new LibraryArtist(artist.Split(new[] { '\0' })[0])));
            }
            _api.Library_QueryLookupTable(null, null, null);
            return list;
        }

        private IEnumerable<LibraryGenre> GetGenreData()
        {
            var list = new List<LibraryGenre>();
            if (_api.Library_QueryLookupTable("genre", "count", null))
            {
                list.AddRange(
                    _api.Library_QueryGetLookupTableValue(null)
                        .Split(new[] { "\0\0" }, StringSplitOptions.None)
                        .Select(artist => new LibraryGenre(artist.Split(new[] { '\0' })[0])));
            }
            _api.Library_QueryLookupTable(null, null, null);
            return list;
        }


        private IEnumerable<LibraryAlbum> GetAlbumData()
        {
            var list = new List<LibraryAlbum>();
            if (_api.Library_QueryLookupTable("album", "count", null))
            {
                list.AddRange(
                    _api.Library_QueryGetLookupTableValue(null)
                        .Split(new[] { "\0\0" }, StringSplitOptions.None)
                        .Select(artist => new LibraryAlbum
                        {
                            Name = artist.Split(new[] { '\0' })[0]
                        }));
            }
            _api.Library_QueryLookupTable(null, null, null);
            return list;
        }

        /// <summary>
        ///     Builds the cover cache per album.
        ///     This method is faster because it calls the GetArtworkUrl method for the first track of each album,
        ///     however it might miss a number of covers;
        /// </summary>
        public void BuildCoverCachePerAlbum()
        {
            using (var db = _cHelper.GetDbConnection())
            using (var trans = db.OpenTransaction())
            {
                var allTrack = db.Select<LibraryTrack>();
                var map = new Dictionary<string, LibraryAlbum>();
                var albums = db.Select<LibraryAlbum>();

                foreach (var lTrack in allTrack)
                {
                    var path = lTrack.Path;
                    var id = _api.Library_GetFileTag(path, Plugin.MetaDataType.AlbumId);
                    LibraryAlbum ab;
                    if (!map.TryGetValue(id, out ab))
                    {
                        ab = albums.SingleOrDefault(q => q.Id == lTrack.AlbumId) ?? new LibraryAlbum();
                        ab.AlbumId = id;
                        map.Add(id, ab);
                    }
                    var trackId = _api.Library_GetFileTag(path, Plugin.MetaDataType.TrackNo);
                    var track = new LibraryTrack
                    {
                        Path = path,
                        Index = !string.IsNullOrEmpty(trackId) ? int.Parse(trackId, NumberStyles.Any) : 0
                    };
                    ab.TrackList.Add(track);
                }

                var list = new List<LibraryAlbum>(map.Values);

                foreach (var albumEntry in list)
                {
                    albumEntry.TrackList.Sort();
                    var path = albumEntry.TrackList[0].Path;
                    var coverUrl = _api.Library_GetArtworkUrl(path, -1);
                    var coverHash = Utilities.StoreCoverToCache(coverUrl);

                    if (String.IsNullOrEmpty(coverHash))
                    {
                        continue;
                    }

                    var cover = new LibraryCover
                    {
                        Hash = coverHash
                    };

                    db.Save(cover);
                    albumEntry.CoverId = (int)db.GetLastInsertId();
                }

                db.UpdateAll(list);
                trans.Commit();
            }
        }

        /// <summary>
        ///     Builds the artist cover cache.
        ///     Method is really slow, due to multiple threads being called.
        ///     Should be better called on a low priority thread.
        /// </summary>
        public void BuildArtistCoverCache()
        {
            var artistList = new List<LibraryArtist>();
            if (_api.Library_QueryLookupTable("artist", "count", ""))
            {
                artistList.AddRange(
                    _api.Library_QueryGetLookupTableValue(null)
                        .Split(new[] { "\0\0" }, StringSplitOptions.None)
                        .Select(entry => entry.Split(new[] { '\0' }))
                        .Select(artistInfo => new LibraryArtist(artistInfo[0])));
            }

            _api.Library_QueryLookupTable(null, null, null);
            foreach (var entry in artistList)
            {
                string[] urls = { };
                var artist = entry.Name;
                _api.Library_GetArtistPictureUrls(artist, true, ref urls);
                if (urls.Length <= 0) continue;
                var hash = Utilities.CacheArtistImage(urls[0], artist);
                entry.ImageUrl = hash;
            }
        }

        public LibraryTrack GetTrackById(int id)
        {
            try
            {
                using (var db = _cHelper.GetDbConnection())
                {
                    return db.GetByIdOrDefault<LibraryTrack>(id);
                }
            }
            catch (Exception)
            {
                throw HttpError.NotFound("Track resource with id {0} does not exist".Fmt(id));
            }
        }

        /// <summary>
        ///     This method checks the state of the cache and is responsible for either
        ///     building the cache when empty of updating on start.
        /// </summary>
        public bool IsCacheEmpty()
        {
            var cached = GetCachedEntitiesCount<LibraryTrack>();
            return cached == 0;
        }


        public PaginatedResponse GetAllTracks(int limit, int offset)
        {
            using (var db = _cHelper.GetDbConnection())
            {
                var data = db.Select<LibraryTrack>(q => q.OrderBy(x => x.Id));
                var result = PaginatedResponse.GetPaginatedData(limit, offset, data);
                return result;
            }
        }


        public PaginatedResponse GetAllArtists(int limit, int offset)
        {
            using (var db = _cHelper.GetDbConnection())
            {
                var data = db.Select<LibraryArtist>(q => q.OrderBy(x => x.Id));
                return PaginatedResponse.GetPaginatedData(limit, offset, data);
            }
        }

        public LibraryArtist GetArtistById(int id)
        {
            using (var db = _cHelper.GetDbConnection())
            {
                try
                {
                    return db.GetById<LibraryArtist>(id);
                }
                catch
                {
                    throw HttpError.NotFound("Artist resource with id {0} does not exist".Fmt(id));
                }
            }
        }

        public PaginatedResponse GetAllGenres(int limit, int offset)
        {
            using (var db = _cHelper.GetDbConnection())
            {
                var data = db.Select<LibraryGenre>(q => q.OrderBy(x => x.Id));
                return PaginatedResponse.GetPaginatedData(limit, offset, data);
            }
        }

        public PaginatedResponse GetAllAlbums(int limit, int offset)
        {
            using (var db = _cHelper.GetDbConnection())
            {
                var data = db.Select<LibraryAlbum>(q => q.OrderBy(x => x.Id));
                return PaginatedResponse.GetPaginatedData(limit, offset, data);
            }
        }

        public Stream GetCoverData(int id)
        {
            var cover = GetLibraryCover(id);
            return Utilities.GetCoverStreamFromCache(cover.Hash);
        }

        public int GetCachedEntitiesCount<T>()
        {
            var total = 0;
            try
            {
                using (var db = _cHelper.GetDbConnection())
                {
                    total = db.Select<T>().Count;
                }
            }
            catch (Exception e)
            {
                Logger.Debug(e);
            }
            return total;
        }

    }
}