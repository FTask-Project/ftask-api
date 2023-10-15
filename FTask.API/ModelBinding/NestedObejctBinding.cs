using Microsoft.AspNetCore.Mvc.ModelBinding;

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
