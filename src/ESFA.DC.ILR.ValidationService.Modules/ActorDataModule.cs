﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Data.Population;
using ESFA.DC.ILR.ValidationService.Data.Population.Interface;

namespace ESFA.DC.ILR.ValidationService.Modules
{
    public class ActorDataModule : BaseDataModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ActorPreValidationPopulationService>().As<IPreValidationPopulationService<IMessage>>()
                .InstancePerLifetimeScope();
            base.Load(builder);
        }
    }
}
