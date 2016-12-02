// Created by Laxale 01.12.2016
//
//


namespace Freengy.WebService.Services 
{
    using System.Threading.Tasks;

    public interface ISmsSender 
    {
        Task SendSmsAsync(string number, string message);
    }
}