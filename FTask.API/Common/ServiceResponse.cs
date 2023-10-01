namespace FTask.API.Common
{
    public class ServiceResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public IEnumerable<string>? Errors { get; set; }
        //public bool? IsRestored { get; set; } = false;
    }
}
