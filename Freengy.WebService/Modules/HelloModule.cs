// Created by Laxale 17.04.2018
//
//

using System;
using Freengy.WebService.Helpers;
using Nancy;
using Nancy.Security;


namespace Freengy.WebService.Modules 
{
    /// <summary>
    /// Module for responding simple 'hello' requests.
    /// </summary>
    public class HelloModule : NancyModule 
    {
        public HelloModule() 
        {
            $"Created { nameof(HelloModule) }".WriteToConsole();

            //this.RequiresHttps();
            Get[Subroutes.Hello] = OnHelloRequest;
        }


        private dynamic OnHelloRequest(dynamic arg) 
        {
            "Got hello request".WriteToConsole();

            return "Hello! This is Freengy WebService. Awesome and alive";
        }
    }
}