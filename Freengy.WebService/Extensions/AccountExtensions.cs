// Created by Laxale 19.04.2018
//
//

using Freengy.Common.Models;
using Freengy.Common.Models.Avatar;
using Freengy.WebService.Models;


namespace Freengy.WebService.Extensions 
{
    internal static class AccountExtensions 
    {
        public static UserAccountModel ToSimpleModel(this ComplexUserAccount complexAccount)
        {
            var simpleModel = new UserAccountModel
            {
                LastLogInTime = complexAccount.LastLogInTime,
                Id = complexAccount.Id,
                Name = complexAccount.Name,
                RegistrationTime = complexAccount.RegistrationTime,
                Expirience = complexAccount.Expirience,
                Privilege = complexAccount.Privilege
            };

            simpleModel.Albums.AddRange(complexAccount.Albums);

            return simpleModel;
        }

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

        public static UserAvatarModel ToSimple(this AvatarModel<ComplexUserAccount> complexAvatar) 
        {
            return new UserAvatarModel
            {
                Id = complexAvatar.Id,
                ParentId = complexAvatar.ParentId,
                ImageBlob = complexAvatar.ImageBlob,
                LastModified = complexAvatar.LastModified
            };
        }


        private static ComplexUserAccount CreateFrom(UserAccountModel otherAccount) 
        {
            var complex = new ComplexUserAccount
            {
                Id = otherAccount.Id,
                //UniqueId = otherAccount.UniqueId,
                Name = otherAccount.Name,
                LastLogInTime = otherAccount.LastLogInTime,
                Privilege = otherAccount.Privilege,
                Expirience = otherAccount.Expirience,
                RegistrationTime = otherAccount.RegistrationTime,
            };
            return complex;
        }
    }
}