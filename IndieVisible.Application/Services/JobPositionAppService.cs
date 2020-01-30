﻿using AutoMapper;
using IndieVisible.Application.Interfaces;
using IndieVisible.Application.ViewModels.Jobs;
using IndieVisible.Domain.Core.Enums;
using IndieVisible.Domain.Interfaces;
using IndieVisible.Domain.Interfaces.Infrastructure;
using IndieVisible.Domain.Interfaces.Service;
using IndieVisible.Domain.Models;
using IndieVisible.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IndieVisible.Application.Services
{
    public class JobPositionAppService : ProfileBaseAppService, IJobPositionAppService
    {
        private readonly IJobPositionDomainService jobPositionDomainService;

        public JobPositionAppService(IMapper mapper
            , IUnitOfWork unitOfWork
            , ICacheService cacheService
            , IProfileDomainService profileDomainService
            , IJobPositionDomainService jobPositionDomainService) : base(mapper, unitOfWork, cacheService, profileDomainService)
        {
            this.jobPositionDomainService = jobPositionDomainService;
        }

        #region ICrudAPpService

        public OperationResultVo<int> Count(Guid currentUserId)
        {
            try
            {
                int count = jobPositionDomainService.Count();

                return new OperationResultVo<int>(count);
            }
            catch (Exception ex)
            {
                return new OperationResultVo<int>(ex.Message);
            }
        }

        public OperationResultListVo<JobPositionViewModel> GetAll(Guid currentUserId)
        {
            try
            {
                IEnumerable<JobPosition> allModels = jobPositionDomainService.GetAll();

                IEnumerable<JobPositionViewModel> vms = mapper.Map<IEnumerable<JobPosition>, IEnumerable<JobPositionViewModel>>(allModels);

                return new OperationResultListVo<JobPositionViewModel>(vms);
            }
            catch (Exception ex)
            {
                return new OperationResultListVo<JobPositionViewModel>(ex.Message);
            }
        }

        public OperationResultVo<JobPositionViewModel> GetById(Guid currentUserId, Guid id)
        {
            try
            {
                JobPosition model = jobPositionDomainService.GetById(id);

                if (model == null)
                {
                    return new OperationResultVo<JobPositionViewModel>("JobPosition not found!");
                }

                JobPositionViewModel vm = mapper.Map<JobPositionViewModel>(model);

                foreach (JobApplicantViewModel applicant in vm.Applicants)
                {
                    UserProfile profile = GetCachedProfileByUserId(applicant.UserId);
                    applicant.Name = profile.Name;
                }

                vm.CurrentUserApplied = model.Applicants.Any(x => x.UserId == currentUserId);

                vm.Permissions.CanEdit = model.UserId == currentUserId;

                return new OperationResultVo<JobPositionViewModel>(vm);
            }
            catch (Exception ex)
            {
                return new OperationResultVo<JobPositionViewModel>(ex.Message);
            }
        }

        public OperationResultVo Remove(Guid currentUserId, Guid id)
        {
            try
            {
                // validate before

                jobPositionDomainService.Remove(id);

                unitOfWork.Commit();

                return new OperationResultVo(true);
            }
            catch (Exception ex)
            {
                return new OperationResultVo(ex.Message);
            }
        }

        public OperationResultVo<Guid> Save(Guid currentUserId, JobPositionViewModel viewModel)
        {
            int pointsEarned = 0;

            try
            {
                JobPosition model;

                JobPosition existing = jobPositionDomainService.GetById(viewModel.Id);
                if (existing != null)
                {
                    model = mapper.Map(viewModel, existing);
                }
                else
                {
                    model = mapper.Map<JobPosition>(viewModel);
                }

                if (viewModel.Id == Guid.Empty)
                {
                    jobPositionDomainService.Add(model);
                    viewModel.Id = model.Id;
                }
                else
                {
                    jobPositionDomainService.Update(model);
                }

                unitOfWork.Commit();

                viewModel.Id = model.Id;

                return new OperationResultVo<Guid>(model.Id, pointsEarned);
            }
            catch (Exception ex)
            {
                return new OperationResultVo<Guid>(ex.Message);
            }
        }
        #endregion

        public OperationResultVo GenerateNewTeam(Guid currentUserId)
        {
            try
            {
                JobPosition newJobPosition = jobPositionDomainService.GenerateNewJobPosition(currentUserId);

                JobPositionViewModel newVm = mapper.Map<JobPositionViewModel>(newJobPosition);

                return new OperationResultVo<JobPositionViewModel>(newVm);
            }
            catch (Exception ex)
            {
                return new OperationResultVo(ex.Message);
            }
        }

        public OperationResultVo GetAllAvailable()
        {
            try
            {
                var allModels = jobPositionDomainService.GetAllAvailable();

                IEnumerable<JobPositionViewModel> vms = mapper.Map<IEnumerable<JobPosition>, IEnumerable<JobPositionViewModel>>(allModels);

                return new OperationResultListVo<JobPositionViewModel>(vms);
            }
            catch (Exception ex)
            {
                return new OperationResultVo(ex.Message);
            }
        }

        public OperationResultVo GetAllMine(Guid currentUserId)
        {
            try
            {
                var allModels = jobPositionDomainService.GetByUserId(currentUserId);

                IEnumerable<JobPositionViewModel> vms = mapper.Map<IEnumerable<JobPosition>, IEnumerable<JobPositionViewModel>>(allModels);

                return new OperationResultListVo<JobPositionViewModel>(vms);
            }
            catch (Exception ex)
            {
                return new OperationResultVo(ex.Message);
            }
        }

        public OperationResultVo Apply(Guid currentUserId, Guid jobPositionId, string coverLetter)
        {
            try
            {
                int pointsEarned = 0;

                var jobPosition = jobPositionDomainService.GetById(jobPositionId);

                if (jobPosition == null)
                {
                    return new OperationResultVo("Unable to identify the job position you are applying for.");
                }

                var alreadyApplyed = jobPosition.Applicants.Any(x => x.UserId == currentUserId);
                if (alreadyApplyed)
                {
                    return new OperationResultVo("You already applyed for this job position.");
                }

                jobPositionDomainService.AddApplicant(currentUserId, jobPositionId, coverLetter);

                unitOfWork.Commit();

                return new OperationResultVo(pointsEarned);
            }
            catch (Exception ex)
            {
                return new OperationResultVo(ex.Message);
            }
        }

        public OperationResultVo ChangeStatus(Guid currentUserId, Guid jobPositionId, JobPositionStatus selectedStatus)
        {
            try
            {
                JobPosition jobPosition = jobPositionDomainService.GetById(jobPositionId);

                if (jobPosition == null)
                {
                    return new OperationResultVo("Idea not found!");
                }

                jobPosition.Status = selectedStatus;

                jobPositionDomainService.Update(jobPosition);

                unitOfWork.Commit();

                return new OperationResultVo(true);
            }
            catch (Exception ex)
            {
                return new OperationResultVo(ex.Message);
            }
        }
    }
}