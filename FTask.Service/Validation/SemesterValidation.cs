using FTask.Repository.Entity;
using FTask.Repository.IRepository;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.EntityFrameworkCore;

namespace FTask.Service.Validation
{
    public interface ISemesterValidation
    {
        Task<ServiceResponse<Semester>> ValidateSemester(DateTime startDate, DateTime endDate, ISemesterRepository semesterRepository);
    }

    internal class SemesterValidation : ISemesterValidation
    {
        private static TimeSpan MINIMUM_DURATION = TimeSpan.FromDays(30);
        private static TimeSpan MAXIMUM_DURATION = TimeSpan.FromDays(90);

        public async Task<ServiceResponse<Semester>> ValidateSemester(DateTime startDate, DateTime endDate, ISemesterRepository semesterRepository)
        {
            if (endDate < startDate)
            {
                return new ServiceResponse<Semester>
                {
                    IsSuccess = false,
                    Message = "Failed to create new semester",
                    Errors = new string[1] { "End date must be greater than start date" }
                };
            }

            var checkSemester = await semesterRepository.Get(s => s.EndDate > startDate && s.StartDate < startDate).FirstOrDefaultAsync();
            if (checkSemester is not null)
            {
                return new ServiceResponse<Semester>
                {
                    IsSuccess = false,
                    Message = "Failed to create new semester",
                    Errors = new string[1] { "The new semester can not begin during another semester" }
                };
            }

            var duration = (endDate - startDate).TotalDays;
            if (duration < MINIMUM_DURATION.Days || duration > MAXIMUM_DURATION.Days)
            {
                return new ServiceResponse<Semester>
                {
                    IsSuccess = false,
                    Message = "Failed to create new semester",
                    Errors = new string[1] { $"The duration of a semester must be larger than {MINIMUM_DURATION.Days} days and less than {MAXIMUM_DURATION.Days} days" }
                };
            }

            return new ServiceResponse<Semester>
            {
                IsSuccess = true
            };
        }
    }
}
