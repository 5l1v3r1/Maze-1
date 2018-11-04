﻿using Autofac;
using Tasks.Infrastructure.Utilities;

namespace Tasks.Common.Server
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            
            builder.RegisterTaskDtos(ThisAssembly);
        }
    }
}