using PluginConsole.Sdk;

namespace PluginConsole.Plugin.Hello
{
    public class HelloPlugin : IPlugin
    {
        public string Name => "Hello Plugin";

        public void Execute()
        {
            Console.WriteLine("Hello!  - From {0}", Name);
        }
    }
}
