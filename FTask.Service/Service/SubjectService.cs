using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Service.Validation;
using FTask.Service.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace FTask.Service.IService
{
    internal class SubjectService : ISubjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        public SubjectService(IUnitOfWork unitOfWork, ICheckQuantityTaken checkQuantityTaken)
        {
            _unitOfWork = unitOfWork;
            _checkQuantityTaken = checkQuantityTaken;
        }

        public async Task<Subject?> GetSubjectById(int id)
        {
            return await _unitOfWork.SubjectRepository.FindAsync(id);
        }

        public async Task<IEnumerable<Subject>> GetSubjectAllSubject(int page, int quantity)
        {
            if (page == 0)
            {
                page = 1;
            }
            quantity = _checkQuantityTaken.check(quantity);
            return await _unitOfWork.SubjectRepository
                .FindAll().Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                .Take(quantity).ToArrayAsync();
        }

        public async Task<IEnumerable<Subject>> GetSubjectFromDepartment(int departmentId)
        {
            //var department = await _unitOfWork.SubjectRepository.FindAsync(departmentId);

            //if (department is null)
            //{
            //    throw new Exception(
            //        new ServiceResponse
            //        {
            //            IsSuccess = false,
            //            Message = "Department not found"
            //        }.ToString()
            //    );


            return await _unitOfWork.SubjectRepository
                         .Get(subject => subject.DepartmentId == departmentId)
                         .ToArrayAsync();
                //.Where(subject => subject.DepartmentId == departmentId)
                //.ToArrayAsync();
        }

        public async Task<ServiceResponse> CreateNewSubject(Subject subjectEntity)
        {
            try
            {
                var isExist = await _unitOfWork.SubjectRepository
                    .Get(subject => subject.SubjectCode == subjectEntity.SubjectCode 
                    || subject.SubjectName == subject.SubjectName)
                    .FirstOrDefaultAsync();
                if (isExist is not null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Subject code or subject name already exist"
                    };
                }

                await _unitOfWork.SubjectRepository.AddAsync(subjectEntity);
                var result = await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = true,
                        Message = "Subject was created successfully",
                        Id = subjectEntity.SubjectId.ToString()
                    };
                }
                else
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Create new subject failed"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Some error happend",
                    Errors = new List<string>() { ex.Message }
                };
            }
        }

        public async Task<ServiceResponse> UpdateSubject(Subject subjectEntity)
        {
            try
            {
                var isExist = await _unitOfWork.SubjectRepository
                    .Get(subject => subject.SubjectCode == subjectEntity.SubjectCode
                    || subject.SubjectName == subject.SubjectName)
                    .FirstOrDefaultAsync();
                if (isExist is null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Subject code or subject name not exist"
                    };
                }

                _unitOfWork.SubjectRepository.Update(subjectEntity);
                var result = await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = true,
                        Message = "Subject was updated successfully",
                        Id = subjectEntity.SubjectId.ToString(),
                    };
                }
                else
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Update subject failed"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Some error happend",
                    Errors = new List<string>() { ex.Message }
                };
            }
        }

        public async Task<ServiceResponse> DeleteSubject(Subject subjectEntity)
        {
            try
            {
                var isExist = await _unitOfWork.SubjectRepository
                    .Get(subject => subject.SubjectCode == subjectEntity.SubjectCode
                    || subject.SubjectName == subject.SubjectName)
                    .FirstOrDefaultAsync();
                if (isExist is null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Subject code or subject name not exist"
                    };
                }

                _unitOfWork.SubjectRepository.Remove(subjectEntity);
                var result = await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = true,
                        Message = "Subject was deleted successfully"
                    };
                }
                else
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Delete subject failed"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Some error happend",
                    Errors = new List<string>() { ex.Message }
                };
            }
        }
    }
}
