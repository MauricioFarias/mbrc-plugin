﻿#region Dependencies

using MusicBeePlugin.Modules;
using MusicBeePlugin.Rest.ServiceModel;
using MusicBeePlugin.Rest.ServiceModel.Type;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace MusicBeePlugin.Rest.ServiceInterface
{
	internal class LibraryService : Service
	{
		private readonly LibraryModule _module;

		public LibraryService(LibraryModule module)
		{
			_module = module;
		}

		public PaginatedResponse<LibraryTrack> Get(GetLibraryTracks request)
		{
			return _module.GetAllTracks(request.Limit, request.Offset);
		}

		public LibraryTrack Get(GetLibraryTrack request)
		{
			return _module.GetTrackById(request.Id);
		}

		public PaginatedResponse<LibraryArtist> Get(GetLibraryArtists request)
		{
			return _module.GetAllArtists(request.Limit, request.Offset);
		}

		public LibraryArtist Get(GetLibraryArtist request)
		{
			return _module.GetArtistById(request.Id);
		}

		public PaginatedResponse<LibraryGenre> Get(GetLibraryGenres request)
		{
			return _module.GetAllGenres(request.Limit, request.Offset);
		}

		public PaginatedResponse<LibraryAlbum> Get(GetLibraryAlbums request)
		{
			return _module.GetAllAlbums(request.Limit, request.Offset);
		}

		public PaginatedResponse<LibraryCover> Get(GetLibraryCovers request)
		{
			return _module.GetAllCovers(request.Offset, request.Limit);
		}

		public LibraryCover Get(GetLibraryCover request)
		{
			return _module.GetLibraryCover(request.Id, true);
		}

		[AddHeader(ContentType = "image/jpeg")]
		public object Get(GetLibraryCoverData request)
		{
			return new HttpResult(_module.GetCoverData(request.Id), "image/jpeg");
		}

		public PaginatedTrackResponse Get(GetLibraryTrackChanges request)
		{
			var response = new PaginatedTrackResponse();
			var data = _module.GetChangesSince<LibraryTrack>(request.LastSync, request.Change);
			response.CreatePage(request.Limit, request.Offset, data);
			return response;
		}

		public PaginatedArtistResponse Get(GetLibraryArtistChanges request)
		{
			var response = new PaginatedArtistResponse();
			var data = _module.GetChangesSince<LibraryArtist>(request.LastSync, request.Change);
			response.CreatePage(request.Limit, request.Offset, data);
			return response;
		}

		public PaginatedAlbumResponse Get(GetLibraryAlbumChanges request)
		{
			var response = new PaginatedAlbumResponse();
			var data = _module.GetChangesSince<LibraryAlbum>(request.LastSync, request.Change);
			response.CreatePage(request.Limit, request.Offset, data);
			return response;
		}

		public PaginatedGenreResponse Get(GetLibraryGenreChanges request)
		{
			var response = new PaginatedGenreResponse();
			var data = _module.GetChangesSince<LibraryGenre>(request.LastSync, request.Change);
			response.CreatePage(request.Limit, request.Offset, data);
			return response;
		}

		public PaginatedCoverResponse Get(GetLibraryCoverChanges request)
		{
			var response = new PaginatedCoverResponse();
			var data = _module.GetChangesSince<LibraryCover>(request.LastSync, request.Change);
			response.CreatePage(request.Limit, request.Offset, data);
			return response;
		}
	}
}