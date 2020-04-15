using Autofac;
using System.IO.Abstractions;

namespace CaptainHook.Cli
{
    public class CoreModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FileSystem>().As<IFileSystem>();
        }
    }
}
