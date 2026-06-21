using LabApi.Events.CustomHandlers;
using LabApi.Loader.Features.Plugins;

namespace KadAntiKOS;

public class PluginMain : Plugin<Config>
{
    public static PluginMain Instance;

    public override string Name => "KadAntiKOS";
    public override string Description => "AntiKOS passives";
    public override string Author => "KadavaSmile";
    public override Version Version => new(0, 1);
    public override Version RequiredApiVersion => LabApi.Features.LabApiProperties.CurrentVersion;

    private readonly Handler labApiHandler = new();

    public override void Enable()
    {
        Instance = this;
        CustomHandlersManager.RegisterEventsHandler(labApiHandler);
    }

    public override void Disable()
    {
        Instance = null;
        CustomHandlersManager.UnregisterEventsHandler(labApiHandler);
    }
}
