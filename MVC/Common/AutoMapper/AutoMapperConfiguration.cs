/*
 * Uman Configuration v1.0.2 (http://www.dogmasolutions.com.ar)
 * ========================================================================
 * Copyright 2007-2014 Dogma Solutions.
 * ========================================================================
 * Configuración del AutoMapper para la conversión automnatica de entidades. 
*/
using AutoMapper;
using MVC.Areas.Security.Models;
using MVC.Security.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC.Security.Common.AutoMapper
{
    public static class AutoMapperConfiguration
    {
        public static void Configure()
        {
            Mapper.CreateMap<AspNetGeneralConfiguration, ConfigurationViewModel>();
            Mapper.CreateMap<ConfigurationViewModel, AspNetGeneralConfiguration>();
        }
    }
}