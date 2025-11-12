using System.Diagnostics.CodeAnalysis;
using Content.Server.Administration.Logs;
using Content.Server.GameTicking;
using Content.Shared.GameTicking;
using Content.Server.Popups;
using Content.Server._NF.Bank;
using Content.Shared._NF.Bank.Components;
using Content.Shared.Cuffs.Components;
using Content.Shared.Database;
using Content.Shared.Medical;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Popups;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Content.Shared._NF.CCVar;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Content.Server.KillTracking;
using Content.Server.Shuttles.Systems;

namespace Content.Server.Rewards;

/// <summary>
/// Implements economy payouts for on-duty actions:
/// - Medical: pay the rescuer when a critically injured patient returns to Alive, with a per-target cooldown.
/// - Security: pay for killing hostile mobs and antagonists.
/// - Security: pay at round end for each antagonist in custody.
/// </summary>
public sealed class DutyRewardSystem : EntitySystem
{
    [Dependency] private readonly BankSystem _bank = default!;
    [Dependency] private readonly SharedMindSystem _minds = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;
    [Dependency] private readonly NpcFactionSystem _factions = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IAdminLogManager _admin = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EmergencyShuttleSystem _emergencyShuttle = default!;

    // Medical cooldown: last time this victim paid out for being rescued.
    private readonly Dictionary<EntityUid, TimeSpan> _medicalVictimLastPaid = new();
    // Optional defib attribution window: track last defib user for a target to fallback attribution when Origin is null.
    private readonly Dictionary<EntityUid, (EntityUid rescuer, TimeSpan time)> _recentDefibs = new();

    private bool _roundEndPaid;

    private int _medicalAmount;
    private TimeSpan _medicalCooldown;
    private int _secKillHostileAmount;
    private int _secKillAntagAmount;
    private int _secCustodyPerAntagAmount;

    public override void Initialize()
    {
        base.Initialize();

        // Load CVars
        UpdateCvars();
        Subs.CVar(_cfg, NFCCVars.RewardsMedicalReviveAmount, _ => UpdateCvars(), true);
        Subs.CVar(_cfg, NFCCVars.RewardsMedicalReviveCooldownMinutes, _ => UpdateCvars(), true);
        Subs.CVar(_cfg, NFCCVars.RewardsSecurityKillHostileAmount, _ => UpdateCvars(), true);
        Subs.CVar(_cfg, NFCCVars.RewardsSecurityKillAntagAmount, _ => UpdateCvars(), true);
        Subs.CVar(_cfg, NFCCVars.RewardsSecurityCustodyPerAntagAmount, _ => UpdateCvars(), true);

    // Medical: track defibs for attribution fallback.
    SubscribeLocalEvent<MobStateComponent, TargetDefibrillatedEvent>(OnTargetDefibrillated);
        // Medical: pay when crit -> alive.
    SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);

        // Security: kill payouts.
        SubscribeLocalEvent<KillReportedEvent>(OnKillReported);

        // Round lifecycle.
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundCleanup);
    }

    private void UpdateCvars()
    {
        _medicalAmount = _cfg.GetCVar(NFCCVars.RewardsMedicalReviveAmount);
        _medicalCooldown = TimeSpan.FromMinutes(_cfg.GetCVar(NFCCVars.RewardsMedicalReviveCooldownMinutes));
        _secKillHostileAmount = _cfg.GetCVar(NFCCVars.RewardsSecurityKillHostileAmount);
        _secKillAntagAmount = _cfg.GetCVar(NFCCVars.RewardsSecurityKillAntagAmount);
        _secCustodyPerAntagAmount = _cfg.GetCVar(NFCCVars.RewardsSecurityCustodyPerAntagAmount);
    }

    private void OnRoundCleanup(RoundRestartCleanupEvent ev)
    {
        _medicalVictimLastPaid.Clear();
        _recentDefibs.Clear();
        _roundEndPaid = false;
    }

    // Record defib user to attribute later if origin is missing on crit->alive.
    private void OnTargetDefibrillated(EntityUid target, MobStateComponent comp, TargetDefibrillatedEvent ev)
    {
        _recentDefibs[target] = (ev.User, _timing.CurTime);
    }

    private void OnMobStateChanged(MobStateChangedEvent ev)
    {
        // Only pay on Critical -> Alive transitions
        if (ev.OldMobState != MobState.Critical || ev.NewMobState != MobState.Alive)
            return;

        var victim = ev.Target;

        // Enforce per-victim cooldown
        if (_medicalVictimLastPaid.TryGetValue(victim, out var last) && _timing.CurTime < last + _medicalCooldown)
            return;

        // Determine rescuer (prefer Origin, else recent defib within 2 minutes)
        var rescuer = ev.Origin;
        if (rescuer == null && _recentDefibs.TryGetValue(victim, out var info))
        {
            if (_timing.CurTime - info.time < TimeSpan.FromMinutes(2))
                rescuer = info.rescuer;
        }

        if (rescuer == null)
            return;

        if (!TryGetBankTarget(rescuer.Value, out var payTarget))
            return;

        if (_medicalAmount <= 0)
            return;

        if (_bank.TryBankDeposit(payTarget.Value, _medicalAmount))
        {
            _medicalVictimLastPaid[victim] = _timing.CurTime;

            var msg = $"You saved {ToPrettyString(victim)}. You were paid {Content.Shared._NF.Bank.BankSystemExtensions.ToSpesoString(_medicalAmount)}.";
            _popup.PopupEntity(msg, payTarget.Value, Filter.Entities(payTarget.Value), false, PopupType.Small);
            _admin.Add(LogType.Action, LogImpact.Low,
                $"DutyReward-Medical: Paid {_medicalAmount} to {ToPrettyString(payTarget.Value)} for rescuing {ToPrettyString(victim)} (crit->alive).");
        }
    }

    private void OnKillReported(ref KillReportedEvent ev)
    {
        // Ignore suicides and environmental kills
        if (ev.Suicide || ev.Primary is KillEnvironmentSource)
            return;

        // Determine killer's mind and check if Security
        if (!TryGetKillerPlayer(ev.Primary, out var mindId, out var mind))
            return;

        if (mindId is not { } killerMindId || !IsSecurity(killerMindId, mind))
            return;

        // Classify target: antag mind takes precedence
        var target = ev.Entity;
        var amount = 0;

        if (_roles.MindIsAntagonist(_minds.GetMind(target)))
        {
            amount = _secKillAntagAmount;
        }
        else if (_factions.IsFactionHostile("NanoTrasen", (target, CompOrNull<NpcFactionMemberComponent>(target))))
        {
            amount = _secKillHostileAmount;
        }

        if (amount <= 0)
            return;

        // Deposit to the killer's current body if it has a bank account
        if (!TryGetBankTarget(mind, out var payTarget))
            return;

        if (_bank.TryBankDeposit(payTarget.Value, amount))
        {
            var reason = amount == _secKillAntagAmount ? "killing an antagonist" : "killing a hostile";
            var msg = $"Security payout: You were paid {Content.Shared._NF.Bank.BankSystemExtensions.ToSpesoString(amount)} for {reason}.";
            _popup.PopupEntity(msg, payTarget.Value, Filter.Entities(payTarget.Value), false, PopupType.Small);
            _admin.Add(LogType.Action, LogImpact.Low,
                $"DutyReward-SecurityKill: Paid {amount} to {ToPrettyString(payTarget.Value)} for {reason} ({ToPrettyString(target)}).");
        }
    }

    private void OnRoundEndText(RoundEndTextAppendEvent ev)
    {
        if (_roundEndPaid)
            return;

        // Count antagonists in custody
        var antagInCustody = 0;
        var mindQuery = EntityQueryEnumerator<MindComponent>();
        while (mindQuery.MoveNext(out var mindId, out var mind))
        {
            if (!_roles.MindIsAntagonist(mindId))
                continue;

            if (IsInCustody((mindId, mind)))
                antagInCustody++;
        }

        if (antagInCustody <= 0 || _secCustodyPerAntagAmount <= 0)
        {
            _roundEndPaid = true;
            return;
        }

        // Collect security minds to pay
        var recipients = new List<(EntityUid mindId, MindComponent mind)>();
        mindQuery = EntityQueryEnumerator<MindComponent>();
        while (mindQuery.MoveNext(out var secMindId, out var secMind))
        {
            if (!IsSecurity(secMindId, secMind))
                continue;

            recipients.Add((secMindId, secMind));
        }

        if (recipients.Count == 0)
        {
            _roundEndPaid = true;
            return;
        }

        var payout = antagInCustody * _secCustodyPerAntagAmount;
        foreach (var (secMindId, secMind) in recipients)
        {
            if (!TryGetBankTarget(secMind, out var payTarget))
                continue;

            if (_bank.TryBankDeposit(payTarget.Value, payout))
            {
                var msg = $"Round-end security payout: {antagInCustody} antagonist(s) in custody. You were paid {Content.Shared._NF.Bank.BankSystemExtensions.ToSpesoString(payout)}.";
                _popup.PopupEntity(msg, payTarget.Value, Filter.Entities(payTarget.Value), false, PopupType.Small);
                _admin.Add(LogType.Action, LogImpact.Low,
                    $"DutyReward-SecurityCustody: Paid {payout} to {ToPrettyString(payTarget.Value)} for {antagInCustody} antags in custody at round end.");
            }
        }

        _roundEndPaid = true;
    }

    private bool TryGetKillerPlayer(KillSource source, [NotNullWhen(true)] out EntityUid? mindId, [NotNullWhen(true)] out MindComponent? mind)
    {
        mindId = null;
        mind = null;

        switch (source)
        {
            case KillPlayerSource player:
                var m = _minds.GetMind(player.PlayerId);
                if (m != null && TryComp(m.Value, out MindComponent? comp))
                {
                    mindId = m.Value;
                    mind = comp;
                    return true;
                }
                break;
            default:
                return false;
        }

        return false;
    }

    private bool IsSecurity(EntityUid mindId, MindComponent? mind = null)
    {
        if (!Resolve(mindId, ref mind))
            return false;

        if (!_jobs.MindTryGetJobId(mindId, out var jobId) || jobId == null)
            return false;

        if (!_jobs.TryGetPrimaryDepartment(jobId.Value, out var dept))
            return false;

        return dept.ID == "Security";
    }

    private bool TryGetBankTarget(EntityUid entity, [NotNullWhen(true)] out EntityUid? target)
    {
        // If the entity itself has a bank account, pay it directly.
        if (HasComp<BankAccountComponent>(entity))
        {
            target = entity;
            return true;
        }

        // Otherwise try their mind's currently owned entity.
        if (_minds.TryGetMind(entity, out var mindId, out var mind) && mind.OwnedEntity is { } owned && HasComp<BankAccountComponent>(owned))
        {
            target = owned;
            return true;
        }

        target = null;
        return false;
    }

    private bool TryGetBankTarget(MindComponent mind, [NotNullWhen(true)] out EntityUid? target)
    {
        // Prefer current body with bank account
        if (mind.OwnedEntity is { } owned && HasComp<BankAccountComponent>(owned))
        {
            target = owned;
            return true;
        }

        // Fallback: original body if still around
        var original = GetEntity(mind.OriginalOwnedEntity);
        if (original is { } orig && HasComp<BankAccountComponent>(orig))
        {
            target = orig;
            return true;
        }

        target = null;
        return false;
    }

    // Custody logic copied from ObjectivesSystem.IsInCustody
    private bool IsInCustody(Entity<MindComponent?> mind)
    {
        if (!Resolve(mind.Owner, ref mind.Comp))
            return false;

        bool originalEntityInCustody = false;
        EntityUid? originalEntity = GetEntity(mind.Comp.OriginalOwnedEntity);
        if (originalEntity.HasValue && originalEntity != mind.Comp.OwnedEntity)
        {
            originalEntityInCustody = TryComp<CuffableComponent>(originalEntity.Value, out var origCuffed) && origCuffed.CuffedHandCount > 0
                   && _emergencyShuttle.IsTargetEscaping(originalEntity.Value);
        }

     return originalEntityInCustody || (mind.Comp.OwnedEntity is { } owned && TryComp<CuffableComponent>(owned, out var cuffed) && cuffed.CuffedHandCount > 0
         && _emergencyShuttle.IsTargetEscaping(owned));
    }
}
