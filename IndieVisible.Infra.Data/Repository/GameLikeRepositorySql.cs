﻿using IndieVisible.Domain.Interfaces.Repository;
using IndieVisible.Domain.Models;
using IndieVisible.Infra.Data.Context;

namespace IndieVisible.Infra.Data.Repository
{
    public class GameLikeRepositorySql : Repository<GameLike>, IGameLikeRepositorySql
    {
        public GameLikeRepositorySql(IndieVisibleContext context) : base(context)
        {

        }
    }
}