using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.BLL.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string email, string code);
        Task SendPasswordResetEmailAsync(string email, string resetLink, string code);
    }

}
