using System.IO.Abstractions;
using Autofac;

namespace Platform.Eda.Cli
{
    public class CoreModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FileSystem>().As<IFileSystem>();
        }
    }
}
