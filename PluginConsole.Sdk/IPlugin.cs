namespace PluginConsole.Sdk;

public interface IPlugin
{
    string Name { get; }

    void Execute();
}
