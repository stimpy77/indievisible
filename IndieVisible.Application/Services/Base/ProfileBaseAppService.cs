﻿using AutoMapper;
using IndieVisible.Domain.Interfaces.Infrastructure;
using IndieVisible.Domain.Interfaces.Service;
using IndieVisible.Domain.Models;
using IndieVisible.Infra.Data.MongoDb.Interfaces;
using System;
using System.Linq;

namespace IndieVisible.Application.Services
{
    public abstract class ProfileBaseAppService : BaseAppService, IProfileBaseAppService
    {
        protected readonly IProfileDomainService profileDomainService;

        protected ProfileBaseAppService(IMapper mapper
            , IUnitOfWork unitOfWork
            , ICacheService cacheService
            , IProfileDomainService profileDomainService) : base(mapper, unitOfWork, cacheService)
        {
            this.profileDomainService = profileDomainService;
        }

        public void SetCache(Guid key, UserProfile value)
        {
            cacheService.Set<Guid, UserProfile>(key, value);
        }

        protected UserProfile GetFromCache(Guid key)
        {
            UserProfile fromCache = cacheService.Get<Guid, UserProfile>(key);

            return fromCache;
        }

        protected UserProfile GetCachedProfileByUserId(Guid userId)
        {
            UserProfile profile = cacheService.Get<Guid, UserProfile>(userId);

            if (profile == null)
            {
                UserProfile profileFromDb = profileDomainService.GetByUserId(userId).FirstOrDefault();

                if (profileFromDb != null)
                {
                    cacheService.Set(userId, profileFromDb);
                    profile = profileFromDb;
                }
            }

            return profile;
        }
    }
}