using HarmonyLib;
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

    //Patcher
    private Harmony harmony;

    public override void Enable()
    {
        Instance = this;
        harmony = new(Name);
        harmony.PatchAll();
        CustomHandlersManager.RegisterEventsHandler(labApiHandler);
    }

    public override void Disable()
    {
        Instance = null;
        harmony.UnpatchAll(Name);
        harmony = null;
        CustomHandlersManager.UnregisterEventsHandler(labApiHandler);
    }
}
