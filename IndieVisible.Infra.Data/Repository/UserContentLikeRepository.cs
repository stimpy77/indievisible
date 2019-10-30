﻿using IndieVisible.Domain.Interfaces.Repository;
using IndieVisible.Domain.Models;
using IndieVisible.Infra.Data.Context;

namespace IndieVisible.Infra.Data.Repository
{
    public class UserContentLikeRepository : Repository<UserContentLike>, IUserContentLikeRepositorySql
    {
        public UserContentLikeRepository(IndieVisibleContext context) : base(context)
        {

        }
    }
}
