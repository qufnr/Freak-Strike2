using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using FreakStrike2.Utils.Exceptions;

namespace FreakStrike2.Utils.Classes;

public class ParticleSystem
{
    public CParticleSystem Particle { get; private set; }

    public ParticleSystem(string effectName)
    {
        var particle = Utilities.CreateEntityByName<CParticleSystem>("info_particle_system");
        if (particle == null)
            throw new GameNotSupportedException();
        
        particle.EffectName = effectName;
        Particle = particle;
    }

    public void SetParent(CBaseEntity entity)
    {
        Particle.Teleport(entity.AbsOrigin);
        Particle.AcceptInput("FollowEntity", Particle, entity, "!activator");
    }

    public void SetParent(CCSPlayerController player)
    {
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn is null)
            throw new NullReferenceException("Member Name \"m_hPlayerPawn\" Value is null.");
        
        Particle.Teleport(playerPawn.AbsOrigin);
        Particle.AcceptInput("FollowEntity", Particle, playerPawn, "!activator");
    }

    public void SetPoint(Vector end, Vector? start)
    {
        Particle.DataCP = 1;
        Particle.DataCPValue.X = end.X;
        Particle.DataCPValue.Y = end.Y;
        Particle.DataCPValue.Z = end.Z;
        Utilities.SetStateChanged(Particle, "CParticleSystem", "m_vecDataCPValue");
        Particle.Teleport(start);
        Server.RunOnTick(Server.TickCount + 1, () => Particle.AcceptInput("Start"));
    }

    public void Remove()
    {
        Particle.Remove();
    }
}