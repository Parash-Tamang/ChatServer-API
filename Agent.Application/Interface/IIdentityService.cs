using Agent.Domain.Entities.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Application.Interface
{
   public interface IIdentityService
    {
        Task<bool> EmailExistAsync(string email);
    }
}
