using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Repository.Entity
{
    public class Attachment
    {
        [Key]
        public int AttachmentId { get; set; }
        public string Url { get; set; } = "Undefined";
        

        public Task? Task { get; set; }
        public int TaskId { get; set; }
    }
}
