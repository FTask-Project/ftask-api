﻿using AutoMapper;
using Duende.IdentityServer.Extensions;
using FTask.Repository.Entity;
using FTask.Service.IService;
using FTask.Service.ViewModel;
using FTask.Service.ViewModel.RequestVM;
using FTask.Service.ViewModel.RequestVM.CreateTask;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ITaskService _taskService;

        public TaskController(IMapper mapper, ITaskService taskService)
        {
            _mapper = mapper;
            _taskService = taskService;
        }

        [HttpGet("{taskId}", Name = nameof(GetTaskById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TaskResponseVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetTaskById(int taskId)
        {
            if (ModelState.IsValid)
            {
                var result = await _taskService.GetTaskById(taskId);
                if(result is null)
                {
                    return NotFound("Not found");
                }
                return Ok(_mapper.Map<TaskResponseVM>(result));
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

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TaskResponseVM>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> GetTasks([FromQuery] int page, [FromQuery] int quantity)
        {
            if (ModelState.IsValid)
            {
                var result = await _taskService.GetTasks(page, quantity);
                return Ok(_mapper.Map<IEnumerable<TaskResponseVM>>(result));
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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TaskResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> CreateTask([FromForm] TaskVM resource)
        {
            if (ModelState.IsValid)
            {
                var data = new string("[" + HttpContext.Request.Form["TaskLecturers"].ToString() + "]");
                if (!data.IsNullOrEmpty())
                {
                    resource.TaskLecturers = JsonConvert.DeserializeObject<IEnumerable<TaskLecturerVM>>(data!) ?? new List<TaskLecturerVM>();
                }
                
                var result = await _taskService.CreateNewTask(resource);
                if (result.IsSuccess)
                {
                    int id = Int32.Parse(result.Id!);
                    var existedTask = await _taskService.GetTaskById(id);
                    if(existedTask is not null)
                    {
                        return CreatedAtAction(nameof(GetTaskById), new
                        {
                            taskId = id,
                        }, 
                        _mapper.Map<TaskResponseVM>(existedTask));
                    }
                    else
                    {
                        return BadRequest(new ServiceResponse
                        {
                            IsSuccess = false,
                            Message = "Create task failed",
                            Errors = new List<string> { "Error at create new task action method", "Created task not found" }
                        });
                    }
                }
                else
                {
                    return BadRequest(result);
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
    }
}