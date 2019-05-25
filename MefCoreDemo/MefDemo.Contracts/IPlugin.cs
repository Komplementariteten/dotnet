using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MefDemo.Contracts
{
    public interface IPlugin
    {
        bool HasPluginSpecificConfiguration { get; }
        IEnumerable<string> Calculate(IConfiguration configuration);
        IConfiguration GetPluginConfiguration();
    }
}
