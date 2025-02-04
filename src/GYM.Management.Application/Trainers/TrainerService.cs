﻿using GYM.Management.Expenses;
using GYM.Management.Permissions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace GYM.Management.Trainers
{
    public class TrainerService : CrudAppService<
        Trainer,
        TrainerDto,
        Guid,
        PagedAndSortedResultRequestDto,
        TrainerCreateDto>,
    ITrainerService
    {
        public TrainerService(IRepository<Trainer, Guid> repository) : base(repository)
        {
			GetPolicyName = ManagementPermissions.Trainer.Default;
			GetListPolicyName = ManagementPermissions.Trainer.Default;
			CreatePolicyName = ManagementPermissions.Trainer.Create;
			UpdatePolicyName = ManagementPermissions.Trainer.Edit;
			DeletePolicyName = ManagementPermissions.Trainer.Delete;
		}

        public async Task DeleteAsyncById(Guid id)
        {
            var query = (await Repository.GetQueryableAsync()).Where(o=>o.Id==id).SelectMany(o => o.Members).Where(o=>o.IsDeleted==false);
            var result = await AsyncExecuter.CountAsync(query);
            if (result>0)
            {
                throw new UserFriendlyException("Bu antrenörün aktif olarak hizmet verdiği üye var öncelikle üye ile ilişkisini kesiniz",
                    "Bu antrenörün aktif olarak hizmet verdiği üye var öncelikle üye ile ilişkisini kesiniz");
            }
            await DeleteAsync(id);
        }
        
        public async Task<List<TrainerDto>> GetAllTrainerForMember()
        {
            var result = await Repository.GetListAsync();
            return  ObjectMapper.Map<List<Trainer>,List<TrainerDto>>(result);
        }

        public async Task PaySalary(Guid trainerId)
        {
            var trainer = await Repository.GetAsync(trainerId);
            trainer.lastSalaryDate = DateTime.UtcNow;
            trainer.Expenses.Add(new Expense
            {
                Amount = trainer.Salary,
                Description = "Maaş Ödemesi",
                ExpenseType = ExpenseType.Salary,
                Date = DateTime.UtcNow
            });
        }

        public async Task<List<TrainerDto>> GetPaymentDuoTrainer()
        {
            DateTime currentDate = DateTime.UtcNow;
            int currentMonth = currentDate.Month;
            int currentYear = currentDate.Year;
            var query = await Repository.GetQueryableAsync();
            query = query.Where(o => (o.lastSalaryDate != null && o.lastSalaryDate != DateTime.MinValue) && o.lastSalaryDate.Year < currentYear ||
            (o.lastSalaryDate.Year == currentYear && o.lastSalaryDate.Month < currentMonth)).Take(5);
            var result = await AsyncExecuter.ToListAsync(query);
            return ObjectMapper.Map<List<Trainer>, List<TrainerDto>>(result);
        }
    }
}
