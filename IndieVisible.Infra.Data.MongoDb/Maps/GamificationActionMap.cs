﻿using IndieVisible.Domain.Models;
using MongoDB.Bson.Serialization;

namespace IndieVisible.Infra.Data.MongoDb.Maps
{
    public static class GamificationActionMap
    {
        public static void Configure()
        {
            BsonClassMap.RegisterClassMap<GamificationAction>(map =>
            {
                map.AutoMap();
            });
        }
    }
}