using System.Runtime.Loader;
using PluginConsole.Sdk;

namespace PluginConsole.Plugin;

public class PluginInfo
{
    public PluginInfo(string path, IPlugin plugin, AssemblyLoadContext assembly)
    {
        Path = path;
        Instance = plugin;
        AssemblyContext = assembly;
    }

    public string Path { get; set; }

    public IPlugin Instance { get; set; }

    public AssemblyLoadContext AssemblyContext { get; set; }
}

public class PluginsManager
{
    private readonly string _directory;
    private readonly FileSystemWatcher _watcher;

    public event EventHandler<PluginInfo>? PluginLoaded;
    public event EventHandler<PluginInfo>? PluginUnloaded;

    private const string Filter = "*.Plugin.*.dll";

    public PluginsManager(string directory)
    {
        _directory = directory;

        Directory.CreateDirectory(_directory);

        _watcher = new FileSystemWatcher()
        {
            Filter = Filter,
            Path = _directory,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
        };

        _watcher.Created += LoadPlugin;
        _watcher.Deleted += UnloadPlugin;

        foreach (var dll in Directory.EnumerateFiles(directory, Filter, SearchOption.AllDirectories))
        {
            var pluginInfo = GetPluginInfo(dll);

            if (pluginInfo != null)
            {
                Plugins.Add(pluginInfo);
            }
        }
    }

    public List<PluginInfo> Plugins { get; private set; } = new();

    private async void LoadPlugin(object sender, FileSystemEventArgs e)
    {
        // Brief delay for io operations to complete.
        await Task.Delay(200);

        if (!File.Exists(e.FullPath))
            return;

        if (Plugins.Any(p => p.Path == e.FullPath))
            return;

        try
        {
            var pluginInfo = GetPluginInfo(e.FullPath);

            if (pluginInfo == null)
                return;

            Plugins.Add(pluginInfo);

            PluginLoaded?.Invoke(this, pluginInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private void UnloadPlugin(object sender, FileSystemEventArgs e)
    {
        var pluginInfo = Plugins.FirstOrDefault(p => p.Path == e.FullPath);

        if (pluginInfo == null)
            return;

        pluginInfo.AssemblyContext.Unload();
        Plugins.Remove(pluginInfo);

        PluginUnloaded?.Invoke(this, pluginInfo);
    }

    private PluginInfo? GetPluginInfo(string fullPath)
    {
        using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);

        var context = new AssemblyLoadContext(Path.GetFileNameWithoutExtension(fullPath), isCollectible: true);
        var assembly = context.LoadFromStream(stream);

        foreach (var type in assembly.GetTypes())
        {
            if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            {
                if (Activator.CreateInstance(type) is IPlugin plugin)
                {
                    return new PluginInfo(fullPath, plugin, context);
                }
            }
        }

        context.Unload();
        return null;
    }
}
