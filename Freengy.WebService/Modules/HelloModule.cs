﻿// Created by Laxale 17.04.2018
//
//

using System;

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
            //this.RequiresHttps();
            Get[Subroutes.Hello] = OnHelloRequest;
        }


        private dynamic OnHelloRequest(dynamic arg) 
        {
            return "Hello! This is Freengy WebService. Awesome and alive";
        }
    }
}