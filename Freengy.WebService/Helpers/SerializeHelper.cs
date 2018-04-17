// Created by Laxale 17.04.2018
//
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Freengy.WebService.Models;

using Nancy;

using Newtonsoft.Json;


namespace Freengy.WebService.Helpers 
{
    internal class SerializeHelper 
    {
        public T DeserializeObject<T>(Stream inputJsonStream) where T : class , new () 
        {
            var serializer = new JsonSerializer();

            using (var streamReader = new StreamReader(inputJsonStream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                T deserialized = serializer.Deserialize<T>(jsonReader);

                return deserialized;
            }
        }
    }
}