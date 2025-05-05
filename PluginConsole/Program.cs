using PluginConsole.Plugin;

const string directory = "Plugins";

var pluginWatcher = new PluginsManager(directory);

pluginWatcher.PluginLoaded += RefreshDisplay;
pluginWatcher.PluginUnloaded += RefreshDisplay;

while (true)
{
    ListPlugins(pluginWatcher);

    var key = Console.ReadKey();

    Console.WriteLine();

    if (key.KeyChar == 'q')
    {
        break;
    }
    else if (int.TryParse(key.KeyChar.ToString(), out var index))
    {
        Console.WriteLine();

        if (index >= 1 && index <= pluginWatcher.Plugins.Count)
        {
            pluginWatcher.Plugins[index - 1].Instance.Execute();
        }
        else
        {
            Console.WriteLine("Index out of range.");
        }

        Console.WriteLine("\n------\n");
    }
}

static void ListPlugins(PluginsManager pluginWatcher)
{
    if (pluginWatcher.Plugins.Count <= 0)
    {
        Console.WriteLine("No plugins available!");
    }
    else
    {
        for (int i = 0; i < pluginWatcher.Plugins.Count; i++)
        {
            Console.WriteLine("{0}. {1}", i + 1, pluginWatcher.Plugins[i].Instance.Name);
        }

        Console.Write("\nEnter option (q to quit): ");
    }
}

static void RefreshDisplay(object? sender, PluginInfo e)
{
    if (sender is not PluginsManager watcher)
        return;

    Console.Clear();
    ListPlugins(watcher);
}