// Created by Laxale 19.04.2018
//
//

using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;

using Freengy.Common.Models;
using Freengy.WebService.Models;


namespace Freengy.WebService.Configuration 
{
    /// <summary>
    /// ORM configuration for a <see cref="ComplexUserAccount"/>.
    /// </summary>
    internal class ComplexUserConfiguration : EntityTypeConfiguration<ComplexUserAccount> 
    {
        public ComplexUserConfiguration() 
        {
            ToTable($"{nameof(UserAccount)}s");
        }
    }
}