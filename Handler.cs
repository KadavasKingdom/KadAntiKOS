using InventorySystem.Disarming;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApiExtensions.Extensions;
using MEC;
using PlayerRoles;

namespace KadAntiKOS;

internal class Handler : CustomEventsHandler
{
    private readonly List<RoleTypeId> affectedRoles = new List<RoleTypeId>
    {
        RoleTypeId.Scientist,
        RoleTypeId.ClassD
    };

    private readonly List<ItemType> illegalItems = new List<ItemType>
    {
        ItemType.GunCrossvec,
        ItemType.GunE11SR,
        ItemType.GunFSP9,
        ItemType.GunRevolver,
        ItemType.GunAK,
        ItemType.GunLogicer,
        ItemType.GunA7,
        ItemType.GunCOM15,
        ItemType.GunCOM18,
        ItemType.GunCom45,
        ItemType.GunFRMG0,
        ItemType.GunSCP127,
        ItemType.GunShotgun,
        ItemType.GrenadeHE,
        ItemType.SCP1509,
        ItemType.Jailbird,
        ItemType.SCP018,
        ItemType.ParticleDisruptor
    };


    public List<string> safePlayers = [];

    public override void OnServerRoundStarted()
    {
        safePlayers.Clear();
    }

    public override void OnPlayerSpawned(PlayerSpawnedEventArgs ev)
    {
        Timing.CallDelayed(5f, () =>
        {
            if (ev.Player == null)
                return;
            if (Round.Duration.Minutes > PluginMain.Instance.Config.GracePeriod)
                return;
            if (!affectedRoles.Contains(ev.Player.Role))
                return;
            if (safePlayers.Contains(ev.Player.UserId.ToString()))
                return;
            if (ev.Player.Items.Any(i => illegalItems.Contains(i.Type)))
                return;

            CL.Info("Safe player added");
            safePlayers.Add(ev.Player.UserId.ToString());
        });
    }

    public override void OnPlayerPickedUpItem(PlayerPickedUpItemEventArgs ev)
    {
        if (ev.Item == null)
            return;
        if (!illegalItems.Contains(ev.Item.Type))
            return;

        RemoveFromGrace(ev.Player);
    }

    public override void OnPlayerHurting(PlayerHurtingEventArgs ev)
    {
        if (ev.Player == null)
            return;
        if (ev.Attacker == null)
            return;
        if (ev.Attacker.IsSCP)
            return;
        if (!affectedRoles.Contains(ev.Player.Role))
            return;

        if (ev.Player.IsDisarmed)
        {
            DisarmedPlayers.DisarmedEntry entry = new((uint)ev.Player.PlayerId, (uint)ev.Attacker.PlayerId);
            bool disarmerAttacking = DisarmedPlayers.Entries.Contains(entry);

            //This might not work, needs more testing
            if (disarmerAttacking)
            {
                CL.Info("Disarmer attacking");
                return;
            }

            CL.Info("Disarmed player attacked, reducing damage");
            ev.DamageHandler.SetDamageValue(ev.DamageHandler.GetDamageValue() * PluginMain.Instance.Config.CuffDamageResistance);
        }
        else
        {
            if (Round.Duration.Minutes > PluginMain.Instance.Config.GracePeriod)
                return;
            if (!safePlayers.Contains(ev.Player.UserId.ToString()))
                return;

            CL.Info("Safe player attacked, reducing damage");
            ev.DamageHandler.SetDamageValue(ev.DamageHandler.GetDamageValue() * PluginMain.Instance.Config.GraceDamageResistance);
        }
    }

    public override void OnPlayerDying(PlayerDyingEventArgs ev)
    {
        RemoveFromGrace(ev.Player);
    }

    public override void OnPlayerChangedRole(PlayerChangedRoleEventArgs ev)
    {
        RemoveFromGrace(ev.Player);
    }

    private void RemoveFromGrace(Player player)
    {
        CL.Info("Safe player removal attemtped");

        if (player == null)
            return;
        if (!safePlayers.Contains(player.UserId.ToString()))
            return;

        CL.Info("Safe player removed");

        safePlayers.Remove(player.UserId.ToString());
    }
}
