// Created by Laxale 19.04.2018
//
//

using Freengy.Common.Models;
using Freengy.WebService.Models;


namespace Freengy.WebService.Extensions 
{
    internal static class AccountExtensions 
    {
        public static ComplexUserAccount ToComplex(this UserAccountModel simpleAccount) 
        {
            var complex = CreateFrom(simpleAccount);

            return complex;
        }

        public static ComplexUserAccount Copy(this ComplexUserAccount otherAccount) 
        {
            ComplexUserAccount complex = CreateFrom((UserAccountModel) otherAccount);

            return complex;
        }


        private static ComplexUserAccount CreateFrom(UserAccountModel otherAccount) 
        {
            var complex = new ComplexUserAccount
            {
                Id = otherAccount.Id,
                UniqueId = otherAccount.UniqueId,
                Name = otherAccount.Name,
                LastLogInTime = otherAccount.LastLogInTime,
                Privilege = otherAccount.Privilege,
                Level = otherAccount.Level,
                RegistrationTime = otherAccount.RegistrationTime,
            };
            return complex;
        }
    }
}