namespace MefCoreDemo
{
    using System;
    using System.Collections.Generic;
    using System.Composition;
    using MefDemo.Contracts;
    using System.Composition.Hosting;
    using System.IO;
    using System.Reflection;
    using System.Linq;

    public class ExportClass
    {
        [ImportMany] public IEnumerable<IPlugin> Processor { get; set; }

        public ExportClass()
        {
            ContainerConfiguration containerConfig = new ContainerConfiguration();
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var files = Directory.EnumerateFiles(path, "MefDemo.*.dll", SearchOption.TopDirectoryOnly);

            var container = new ContainerConfiguration();
            foreach (var assPath in files)
            {
                var asm = Assembly.LoadFrom(assPath);
                container.WithAssembly(asm);
            }
            using (var host = container.CreateContainer())
            {
                this.Processor = host.GetExports<IPlugin>();
            }
        }

        public void Process(IConfiguration cfg)
        {
            foreach(var plugin in this.Processor)
            {
                if (plugin.HasPluginSpecificConfiguration)
                {
                    cfg = plugin.GetPluginConfiguration();
                }
                Console.WriteLine(String.Join(" => ", plugin.Calculate(cfg)));

            }
        }
    }
}
