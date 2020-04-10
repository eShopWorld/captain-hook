using Autofac;
using Microsoft.Extensions.FileProviders;
using System.IO.Abstractions;

namespace CaptainHook.Cli
{
    public class CoreModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PhysicalFileProvider>().As<IFileProvider>();
            builder.RegisterType<FileSystem>().As<IFileSystem>();
        }
    }
}
