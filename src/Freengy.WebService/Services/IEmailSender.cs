// Created by Laxale 01.12.2016
//
//


namespace Freengy.WebService.Services
{
    using System.Threading.Tasks;

    public interface IEmailSender 
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}