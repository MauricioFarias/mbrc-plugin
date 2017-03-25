﻿using System.Collections.Generic;
using LiteDB;
using MusicBeePlugin.AndroidRemote.Model.Entities;
using MusicBeePlugin.AndroidRemote.Settings;

namespace MusicBeePlugin.AndroidRemote.Core
{
    public interface IRepository<T>
    {
        void AddAll(IEnumerable<T> items);
        void RemoveAll();
        IEnumerable<T> GetRange(int offset, int limit);
    }

    internal interface ITrackRepository : IRepository<Track>
    {
    }

    class TrackRepository : ITrackRepository
    {
        private readonly IStorageLocationProvider _storageProvider;

        public TrackRepository(IStorageLocationProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public void AddAll(IEnumerable<Track> items)
        {
            using (var db = new LiteDatabase(_storageProvider.CacheLocation()))
            {
                var collection = db.GetCollection<Track>("tracks");
                using (var transaction = db.BeginTrans())
                {
                    foreach (var track in items)
                    {
                        collection.Insert(track);
                    }

                    transaction.Commit();
                }
            }
        }

        public void RemoveAll()
        {
            using (var db = new LiteDatabase(_storageProvider.CacheLocation()))
            {
                db.DropCollection("tracks");
            }
        }

        public IEnumerable<Track> GetRange(int offset, int limit)
        {
            using (var db = new LiteDatabase(_storageProvider.CacheLocation()))
            {
                var collection = db.GetCollection<Track>("tracks");
                return collection.Find(Query.All(), offset, limit);
            }
        }
    }
}