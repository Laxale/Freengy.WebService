// Created by Laxale 07.05.2018
//
//

using System;
using System.Collections.Generic;

using Freengy.WebService.Models;


namespace Freengy.WebService.Helpers 
{
    /// <summary>
    /// Comparator for a <see cref="FriendshipModel"/>.
    /// </summary>
    internal class FriendshipComparer : IEqualityComparer<FriendshipModel> 
    {
        /// <summary>Determines whether the specified objects are equal.</summary>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        /// <param name="first">The first object of type <see cref="FriendshipModel" /> to compare.</param>
        /// <param name="second">The second object of type <see cref="FriendshipModel" /> to compare.</param>
        public bool Equals(FriendshipModel first, FriendshipModel second) 
        {
            if(first == null || second == null) throw new InvalidOperationException("Modles must not be null");

            return first.Id == second.Id;
        }

        /// <summary>Returns a hash code for the specified object.</summary>
        /// <returns>A hash code for the specified object.</returns>
        /// <param name="model">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
        /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="model" /> is a reference type and <paramref name="model" /> is null.</exception>
        public int GetHashCode(FriendshipModel model) 
        {
            return model.Id.GetHashCode();
        }
    }
}