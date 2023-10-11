using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.ModelBinding
{
    internal class NestedObejctBinding : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var data = bindingContext.HttpContext.Request.Form["TaskLecturers"];
            var test = data.FirstOrDefault();
            return Task.CompletedTask;

        }
    }
}
