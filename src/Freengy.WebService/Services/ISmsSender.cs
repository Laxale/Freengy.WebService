using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freengy.WebService.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
