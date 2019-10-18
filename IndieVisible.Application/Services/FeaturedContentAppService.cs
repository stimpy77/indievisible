﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using IndieVisible.Application.Formatters;
using IndieVisible.Application.Interfaces;
using IndieVisible.Application.ViewModels.Content;
using IndieVisible.Application.ViewModels.FeaturedContent;
using IndieVisible.Application.ViewModels.Home;
using IndieVisible.Domain.Core.Enums;
using IndieVisible.Domain.Interfaces.Base;
using IndieVisible.Domain.Interfaces.Repository;
using IndieVisible.Domain.Models;
using IndieVisible.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IndieVisible.Application.Services
{
    public class FeaturedContentAppService : BaseAppService, IFeaturedContentAppService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFeaturedContentRepository _repository;
        private readonly IUserContentRepository _contentRepository;
        private readonly IUserContentLikeRepository _likeRepository;
        private readonly IUserContentCommentRepository _commentRepository;

        public FeaturedContentAppService(IMapper mapper, IUnitOfWork unitOfWork, IFeaturedContentRepository repository, IUserContentRepository contentRepository, IUserContentLikeRepository likeRepository, IUserContentCommentRepository commentRepository)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _repository = repository;
            _contentRepository = contentRepository;
            _likeRepository = likeRepository;
            _commentRepository = commentRepository;
        }

        #region ICrudAppService
        public OperationResultVo<int> Count(Guid currentUserId)
        {
            try
            {
                int count = _repository.GetAll().Count();

                return new OperationResultVo<int>(count);
            }
            catch (Exception ex)
            {
                return new OperationResultVo<int>(ex.Message);
            }
        }

        public OperationResultListVo<FeaturedContentViewModel> GetAll(Guid currentUserId)
        {
            try
            {
                IQueryable<FeaturedContent> allModels = _repository.GetAll();

                IEnumerable<FeaturedContentViewModel> vms = _mapper.Map<IEnumerable<FeaturedContent>, IEnumerable<FeaturedContentViewModel>>(allModels);

                return new OperationResultListVo<FeaturedContentViewModel>(vms);
            }
            catch (Exception ex)
            {
                return new OperationResultListVo<FeaturedContentViewModel>(ex.Message);
            }
        }

        public OperationResultVo<FeaturedContentViewModel> GetById(Guid currentUserId, Guid id)
        {
            try
            {
                FeaturedContent model = _repository.GetById(id);

                FeaturedContentViewModel vm = _mapper.Map<FeaturedContentViewModel>(model);

                return new OperationResultVo<FeaturedContentViewModel>(vm);
            }
            catch (Exception ex)
            {
                return new OperationResultVo<FeaturedContentViewModel>(ex.Message);
            }
        }

        public OperationResultVo<Guid> Save(Guid currentUserId, FeaturedContentViewModel viewModel)
        {
            try
            {
                FeaturedContent model;

                FeaturedContent existing = _repository.GetById(viewModel.Id);

                if (existing != null)
                {
                    model = _mapper.Map(viewModel, existing);
                }
                else
                {
                    model = _mapper.Map<FeaturedContent>(viewModel);
                }

                if (viewModel.Id == Guid.Empty)
                {
                    _repository.Add(model);
                    viewModel.Id = model.Id;
                }
                else
                {
                    _repository.Update(model);
                }

                _unitOfWork.Commit();

                return new OperationResultVo<Guid>(model.Id);
            }
            catch (Exception ex)
            {
                return new OperationResultVo<Guid>(ex.Message);
            }
        }

        public OperationResultVo Remove(Guid currentUserId, Guid id)
        {
            try
            {
                // validate before

                _repository.Remove(id);

                _unitOfWork.Commit();

                return new OperationResultVo(true);
            }
            catch (Exception ex)
            {
                return new OperationResultVo(ex.Message);
            }
        }
        #endregion


        public CarouselViewModel GetFeaturedNow()
        {
            IQueryable<FeaturedContent> allModels = _repository.GetAll()
                .Where(x => x.StartDate.Date <= DateTime.Today && (x.EndDate.Date == DateTime.MinValue || x.EndDate.Date > DateTime.Today));

            if (allModels.Any())
            {
                IEnumerable<FeaturedContentViewModel> vms = allModels.ProjectTo<FeaturedContentViewModel>(_mapper.ConfigurationProvider);

                CarouselViewModel model = new CarouselViewModel
                {
                    Items = vms.OrderByDescending(x => x.CreateDate).ToList()
                };

                return model;
            }
            else
            {
                CarouselViewModel fake = FakeData.FakeCarousel();

                return fake;
            }
        }

        public OperationResultVo<Guid> Add(Guid userId, Guid contentId, string title, string introduction)
        {
            try
            {
                FeaturedContent newFeaturedContent = new FeaturedContent
                {
                    UserContentId = contentId
                };

                UserContent content = _contentRepository.GetById(contentId);

                newFeaturedContent.Title = string.IsNullOrWhiteSpace(title) ? content.Title : title;
                newFeaturedContent.Introduction = string.IsNullOrWhiteSpace(introduction) ? content.Introduction : introduction;

                newFeaturedContent.ImageUrl = string.IsNullOrWhiteSpace(content.FeaturedImage) || content.FeaturedImage.Equals(Constants.DefaultFeaturedImage) ? Constants.DefaultFeaturedImage : UrlFormatter.Image(content.UserId, BlobType.FeaturedImage, content.FeaturedImage);

                newFeaturedContent.StartDate = DateTime.Now;
                newFeaturedContent.Active = true;
                newFeaturedContent.UserId = userId;

                _repository.Add(newFeaturedContent);

                _unitOfWork.Commit();

                return new OperationResultVo<Guid>(newFeaturedContent.Id);
            }
            catch (Exception ex)
            {
                return new OperationResultVo<Guid>(ex.Message);
            }
        }

        public IEnumerable<UserContentToBeFeaturedViewModel> GetContentToBeFeatured()
        {
            IQueryable<UserContent> finalList = _contentRepository.GetAll();

            List<UserContentToBeFeaturedViewModel> viewModels = finalList.ProjectTo<UserContentToBeFeaturedViewModel>(_mapper.ConfigurationProvider).ToList();

            foreach (UserContentToBeFeaturedViewModel item in viewModels)
            {
                FeaturedContent featuredNow = _repository.GetAll().FirstOrDefault(x => x.UserContentId == item.Id && x.StartDate.Date <= DateTime.Today && (x.EndDate.Date == DateTime.MinValue || x.EndDate.Date > DateTime.Today));

                if (featuredNow != null)
                {
                    item.CurrentFeatureId = featuredNow.Id;
                }


                item.IsFeatured = item.CurrentFeatureId.HasValue;

                item.AuthorName = string.IsNullOrWhiteSpace(item.AuthorName) ? "Unknown soul" : item.AuthorName;

                item.TitleCompliant = !string.IsNullOrWhiteSpace(item.Title) && item.Title.Length <= 25;

                item.IntroCompliant = !string.IsNullOrWhiteSpace(item.Introduction) && item.Introduction.Length <= 55;

                item.ContentCompliant = !string.IsNullOrWhiteSpace(item.Content) && item.Content.Length >= 800;

                item.IsArticle = !string.IsNullOrWhiteSpace(item.Title) && !string.IsNullOrWhiteSpace(item.Introduction);


                item.LikeCount = _likeRepository.GetAll().Count(x => x.ContentId == item.Id);

                item.CommentCount = _commentRepository.GetAll().Count(x => x.UserContentId == item.Id);
            }

            viewModels = viewModels.OrderByDescending(x => x.IsFeatured).ToList();

            return viewModels;
        }


        public OperationResultVo Unfeature(Guid id)
        {
            try
            {
                FeaturedContent existing = _repository.GetById(id);

                if (existing != null)
                {
                    existing.EndDate = DateTime.Now;

                    existing.Active = false;

                    _unitOfWork.Commit();
                }

                return new OperationResultVo(true);
            }
            catch (Exception ex)
            {
                return new OperationResultVo(ex.Message);
            }
        }
    }
}
