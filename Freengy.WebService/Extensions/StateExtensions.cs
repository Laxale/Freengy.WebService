// Created by Laxale 11.05.2018
//
//

using System.Text;
using System.Threading.Tasks;

using Freengy.Common.Models;
using Freengy.WebService.Models;


namespace Freengy.WebService.Extensions
{
    /// <summary>
    /// Содержит расширения для работы с <see cref="AccountStateModel"/>.
    /// </summary>
    internal static class StateExtensions 
    {
        /// <summary>
        /// Преобразовать сложную серверную модель в простую для отправки юзеру.
        /// </summary>
        /// <param name="complexModel"></param>
        /// <returns></returns>
        public static AccountStateModel ToSimple(this ComplexAccountState complexModel) 
        {
            var simpleModel = new AccountStateModel
            {
                Address = complexModel.Address,
                OnlineStatus = complexModel.OnlineStatus,
                AccountModel = complexModel.ComplexAccount.ToSimpleModel()
            };

            return simpleModel;
        }
    }
}