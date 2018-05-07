// Created by Laxale 17.04.2018
//
//

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Freengy.Common.Database;


namespace Freengy.WebService.Models 
{
    /// <summary>
    /// Represents user's password data.
    /// </summary>
    internal class Password : ChildComplexDbObject<ComplexUserAccount> 
        //: DbObject
    {
        /// <summary>
        /// Gets or sets next single login action password salt.
        /// </summary>
        public string NextLoginSalt { get; set; }

        /// <summary>
        /// Gets or sets next single login action resulting password hash.
        /// </summary>
        public string NextPasswordHash { get; set; }


        /// <summary>Создать реальный объект из объекта-прокси EF.</summary>
        /// <param name="dbProxy">Прокси-объект, полученный из базы, который нужно превратить в реальный объект.</param>
        /// <returns>Реальный объект <see cref="T:Freengy.Common.Database.DbObject" />.</returns>
        public override DbObject CreateFromProxy(DbObject dbProxy) 
        {
            var passwordProxy = (Password)dbProxy;

            return new Password
            {
                Id = passwordProxy.Id,
                ParentId = passwordProxy.ParentId,
                NextLoginSalt = passwordProxy.NextLoginSalt,
                NextPasswordHash = passwordProxy.NextPasswordHash
            };
        }

        /// <summary>
        /// Заполнить актуальными данными зависимые свойства типа public <see cref="T:System.Collections.Generic.List`1" /> MyList { get; set; }.
        /// Обнулить навигационные свойства.
        /// </summary>
        /// <returns>Ссылка на сам <see cref="T:Freengy.Common.Database.ComplexDbObject" /> с заполненными мап-пропертями и обнулёнными навигационными.</returns>
        public override ComplexDbObject PrepareMappedProps() 
        {
            NavigationParent = null;

            return this;
        }

        /// <summary>
        /// Получить список названий вложенных пропертей класса (которые не простых типов данных).
        /// </summary>
        /// <returns>Список названий вложенных пропертей класса.</returns>
        protected override IEnumerable<string> GetIncludedPropNames()
        {
            throw new NotImplementedException();
        }
    }
}