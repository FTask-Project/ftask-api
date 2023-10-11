﻿using AutoMapper;
using FTask.Repository.Entity;
using FTask.Service.IService;
using FTask.Service.ViewModel;
using FTask.Service.ViewModel.RequestVM.CreateSubject;
using Microsoft.AspNetCore.Mvc;

namespace FTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectService _subjectService;
        private readonly IMapper _mapper;

        public SubjectController(
            ISubjectService subjectService,
            IMapper mapper
            )
        {
            _subjectService = subjectService;
            _mapper = mapper;
        }

        [HttpGet("{subjectId}", Name = nameof(GetSubjectById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SubjectResponseVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetSubjectById(int subjectId)
        {
            if (ModelState.IsValid)
            {
                var subjectResult = await _subjectService.GetSubjectById(subjectId);
                if (subjectResult is null)
                {
                    return NotFound("Not found");
                }
                return Ok(_mapper.Map<SubjectResponseVM>(subjectResult));
            }
            else
            {
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Invalid input",
                });
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SubjectResponseVM>))]
        public async Task<IActionResult> GetSubjects([FromQuery] int page, [FromQuery] int amount)
        {
            if (ModelState.IsValid)
            {
                var subjectList = await _subjectService.GetSubjectAllSubject(page, amount);
                return Ok(_mapper.Map<IEnumerable<SubjectResponseVM>>(subjectList));
            }
            else
            {
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Invalid input",
                });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SubjectResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectVM subject)
        {
            if (ModelState.IsValid)
            {
                var result = await _subjectService.CreateNewSubject(_mapper.Map<Subject>(subject));
                if (result.IsSuccess)
                {
                    var id = Int32.Parse(result.Id!);
                    var existedSubject = await _subjectService.GetSubjectById(id);
                    if (existedSubject is not null)
                    {
                        return CreatedAtAction(
                            nameof(GetSubjectById),
                            new { subjectId = id },
                            _mapper.Map<SubjectResponseVM>(existedSubject)
                        );
                    }
                    else
                    {
                        return BadRequest(new ServiceResponse
                        {
                            IsSuccess = false,
                            Message = "Create new subject failed"
                        });
                    }
                }
                else
                {
                    return BadRequest(new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Create new subject fail"
                    });
                }
            }
            else
            {
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Invalid input"
                });
            }
        }

        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
