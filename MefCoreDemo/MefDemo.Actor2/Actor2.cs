using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using MefDemo.Contracts;

namespace MefDemo.Actor2
{
    [Export(typeof(IPlugin))]
    public class Actor2 : IPlugin
    {
        public bool HasPluginSpecificConfiguration => false;

        public IEnumerable<string> Calculate(IConfiguration configuration)
        {
            return new List<string>() { "hhhh#", "jjlk" };
        }

        public IConfiguration GetPluginConfiguration()
        {
            return null;
        }
    }
}
