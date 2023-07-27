﻿using static RainMeadow.Serializer;

namespace RainMeadow
{
    public class TickReference : ICustomSerializable
    {
        public ushort fromPlayer;
        public ulong tick;

        public TickReference() { }
        public TickReference(OnlinePlayer fromPlayer, ulong tick)
        {
            this.fromPlayer = fromPlayer.inLobbyId;
            this.tick = tick;
        }

        public TickReference(OnlinePlayer player)
        {
            this.fromPlayer = player.inLobbyId;
            this.tick = player.tick;
        }

        public bool Invalid()
        {
            return OnlineManager.lobby.PlayerFromId(fromPlayer) == null;
        }

        public bool ChecksOut()
        {
            return !Invalid() && NetIO.IsNewerOrEqual(OnlineManager.lobby.PlayerFromId(fromPlayer).tick, tick);
        }

        public void CustomSerialize(Serializer serializer)
        {
            serializer.Serialize(ref fromPlayer);
            serializer.Serialize(ref tick);
        }

        public static TickReference NewestOf(TickReference tick, OnlineResource inResource, TickReference otherTick, OnlineResource otherResource)
        {
            if (otherTick.fromPlayer != otherResource.owner.inLobbyId && tick.fromPlayer != inResource.owner.inLobbyId) return null;
            if (otherTick.fromPlayer != otherResource.owner.inLobbyId) return tick;
            if (tick.fromPlayer != inResource.owner.inLobbyId) return otherTick;
            return NetIO.IsNewerOrEqual(tick.tick, otherTick.tick) ? tick : otherTick;
        }

        public static TickReference NewestOfMemberships(ResourceMembership membershipA, ResourceMembership membershipB)
        {
            if (membershipA.memberSinceTick.fromPlayer != membershipA.resource.owner.inLobbyId && membershipB.memberSinceTick.fromPlayer != membershipB.resource.owner.inLobbyId) return null;
            if (membershipA.memberSinceTick.fromPlayer != membershipA.resource.owner.inLobbyId) return membershipB.memberSinceTick;
            if (membershipB.memberSinceTick.fromPlayer != membershipB.resource.owner.inLobbyId) return membershipA.memberSinceTick;
            return NetIO.IsNewerOrEqual(membershipA.memberSinceTick.tick, membershipB.memberSinceTick.tick) ? membershipA.memberSinceTick : membershipB.memberSinceTick;
        }
    }
}