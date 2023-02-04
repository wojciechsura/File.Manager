using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.Dependencies
{
    public static class Container
    {
        public static IContainer Instance { get; private set; }

        public static void BuildContainer(Action<ContainerBuilder> buildActions)
        {
            if (Instance != null)
                throw new InvalidOperationException("Container can be built only once!");

            var builder = new ContainerBuilder();
            buildActions(builder);
            builder.RegisterSource(new Autofac.Features.ResolveAnything.AnyConcreteTypeNotAlreadyRegisteredSource());
            Instance = builder.Build();
        }
    }
}
