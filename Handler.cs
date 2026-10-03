using CustomPlayerEffects;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApiExtensions.Extensions;
using MEC;
using PlayerRoles;
using UnityEngine;

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
        if (!PluginMain.Instance.Config.EnableGrace)
            return;
        safePlayers.Clear();
    }

    public override void OnPlayerSpawned(PlayerSpawnedEventArgs ev)
    {
        if (!PluginMain.Instance.Config.EnableGrace)
            return;

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
            if (ev.Player.DisarmedBy == ev.Attacker)
            {
                CL.Info("Disarmer attacking");
                return;
            }

            if (ev.Player.DisarmedBy == null)
                return;

            CL.Info("Disarmed player attacked, reducing damage");

            float damage = ev.DamageHandler.GetDamageValue();
            float distance = Vector3.Distance(ev.Player.Position, ev.Player.DisarmedBy.Position);
            float finalDamage = Mathf.Clamp(damage * (distance / PluginMain.Instance.Config.DisarmMaxDistance), damage * PluginMain.Instance.Config.CuffDamageResistance, damage);
            CL.Info($"Damage {damage} | Distance {distance} | FinalDamage {finalDamage}");

            ev.DamageHandler.SetDamageValue(finalDamage);
        }
        else
        {
            if (!PluginMain.Instance.Config.EnableGrace)
                return;
            if (Round.Duration.Minutes > PluginMain.Instance.Config.GracePeriod)
                return;
            if (!safePlayers.Contains(ev.Player.UserId.ToString()))
                return;

            CL.Info("Safe player attacked, reducing damage");
            ev.DamageHandler.SetDamageValue(ev.DamageHandler.GetDamageValue() * PluginMain.Instance.Config.GraceDamageResistance);
        }
    }

    public override void OnPlayerDroppingItem(PlayerDroppingItemEventArgs ev)
    {
        CL.Info($"Player {ev.Player.Nickname} dropping item {ev.Item.Type}");
        if (ev.Item.Type == ItemType.SCP1344 && ev.Player.IsDisarmed)
        {
            ev.IsAllowed = false;
        }
        base.OnPlayerDroppingItem(ev);
    }

    public override void OnPlayerUncuffing(PlayerUncuffingEventArgs ev)
    {
        if (ev.Target == null)
            return;
        if (!ev.Target.IsDisarmed)
            return;
        if (ev.Player == null)
            return;
        if (ev.Target.DisarmedBy == null)
        {
            //NotOwnedDetaineeInteractedWith(ev);
            return;
        }
        if (ev.Target.DisarmedBy == ev.Player)
            return;
        if (Vector3.Distance(ev.Target.DisarmedBy.Position, ev.Target.Position) > PluginMain.Instance.Config.UncuffMaxDistance)
            return;
        if (ev.Target.DisarmedBy.Faction == ev.Player.Faction)
        {
            HintFrameworkHub.HintSystem.ShowHint(ev.Player, $"<i>You cannot uncuff someone detained by your team whilst their cuffer (<b>{ev.Target.DisarmedBy.Nickname}</b>) is near-by</i>", 5f);
            ev.IsAllowed = false;
            base.OnPlayerUncuffing(ev);
        }
    }

    public override void OnPlayerCuffed(PlayerCuffedEventArgs ev)
    {
        Timing.CallDelayed(1f, () =>
        {
            ev.Target.DisableEffect<SeveredEyes>();
            ev.Target.DisableEffect<Blindness>();
        });
    }

    private void NotOwnedDetaineeInteractedWith(PlayerUncuffingEventArgs ev)
    {
        ev.Target.DisarmedBy = ev.Player;
        HintFrameworkHub.HintSystem.ShowHint(ev.Player, $"<i>You have gained ownership of <b>{ev.Target.Nickname}</b> as your detainee</i>", 5f);
        HintFrameworkHub.HintSystem.ShowHint(ev.Target, $"<i><b>{ev.Player.Nickname}</b> has gained ownership of you as a detainee</i>", 5f);
    }

    public override void OnPlayerDying(PlayerDyingEventArgs ev) => RemoveFromGrace(ev.Player);

    public override void OnPlayerChangedRole(PlayerChangedRoleEventArgs ev)
    {
        if (ev.NewRole.RoleTypeId == RoleTypeId.None)
            return;
        if (ev.NewRole.RoleTypeId == RoleTypeId.Destroyed)
            return;

        RemoveFromGrace(ev.Player);
    }

    private void RemoveFromGrace(Player player)
    {
        if (!PluginMain.Instance.Config.EnableGrace)
            return;
        if (player == null)
            return;
        if (!safePlayers.Contains(player.UserId.ToString()))
            return;

        safePlayers.Remove(player.UserId.ToString());
    }
}
