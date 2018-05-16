// Created by Laxale 06.05.2018
//
//

using System;
using System.Collections.Generic;

using Freengy.Common.Database;
using Freengy.Common.Models.Avatar;


namespace Freengy.WebService.Models 
{
    internal class ComplexUserAvatarModel : ChildComplexDbObject<ComplexUserAccount> 
    {
        public byte[] AvatarBlob { get; set; }

        public DateTime LastModified { get; set; }


        public override DbObject CreateFromProxy(DbObject dbProxy) 
        {
            throw new NotImplementedException();
        }

        public override ComplexDbObject PrepareMappedProps() 
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<string> GetIncludedPropNames() 
        {
            throw new NotImplementedException();
        }
    }
}