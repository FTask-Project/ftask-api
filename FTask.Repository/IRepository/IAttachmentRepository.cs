﻿using FTask.Repository.Data;
using FTask.Repository.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Repository.IRepository
{
    public interface IAttachmentRepository : IBaseRepository<Attachment,int>
    {
    }
}
