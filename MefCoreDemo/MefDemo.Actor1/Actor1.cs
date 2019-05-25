using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using MefDemo.Contracts;

namespace MefDemo.Actor1
{
    [Export(typeof(IPlugin))]
    public class Actor1 : IPlugin
    {
        public bool HasPluginSpecificConfiguration => true;

        public IEnumerable<string> Calculate(IConfiguration configuration)
        {
            var q = new Queue<string>();
            q.Enqueue("abc");
            q.Enqueue("mef");
            return q;
        }

        public IConfiguration GetPluginConfiguration()
        {
            return new Actor1Configuration();
        }
    }
}
