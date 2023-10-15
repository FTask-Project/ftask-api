using System.ComponentModel.DataAnnotations;

namespace FTask.Repository.Entity
{
    public class Attachment
    {
        [Key]
        public int AttachmentId { get; set; }
        public string Url { get; set; } = "Undefined";


        public Task? Task { get; set; }
        public int TaskId { get; set; }

        public bool Deleted { get; set; } = false;
    }
}
