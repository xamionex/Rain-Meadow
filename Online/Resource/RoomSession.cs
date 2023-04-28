﻿using System;
using System.Runtime.CompilerServices;

namespace RainMeadow
{
    public partial class RoomSession : OnlineResource
    {
        public AbstractRoom absroom;
        public bool abstractOnDeactivate;
        public static ConditionalWeakTable<AbstractRoom, RoomSession> map = new();

        public WorldSession worldSession => super as WorldSession;
        protected override World World => worldSession.world;

        public RoomSession(WorldSession ws, AbstractRoom absroom)
        {
            super = ws;
            this.absroom = absroom;
            map.Add(absroom, this);
        }

        protected override void AvailableImpl()
        {
            base.AvailableImpl();

            if(isOwner)
            {
                foreach (var ent in absroom.entities)
                {
                    if (ent is AbstractPhysicalObject apo && OnlineEntity.map.TryGetValue(apo, out var oe)
                         && !oe.realized && !oe.owner.isMe && oe.isTransferable && !oe.isPending)
                    {
                        oe.Request(); // I am realizing this entity, let me have it
                    }
                }
            }
        }

        protected override void ActivateImpl()
        {
            foreach (var ent in absroom.entities)
            {
                if (ent is AbstractPhysicalObject apo)
                {
                    ApoEnteringRoom(apo, apo.pos);
                }
            }
        }

        protected override void DeactivateImpl()
        {
            if (abstractOnDeactivate)
            {
                absroom.Abstractize();
            }
        }
        public override string Id()
        {
            return super.Id() + absroom.name;
        }

        public override ushort ShortId()
        {
            return (ushort)absroom.index;
        }

        public override OnlineResource SubresourceFromShortId(ushort shortId)
        {
            return this.subresources[shortId];
        }

        public override void ReadState(ResourceState newState, ulong ts)
        {
            base.ReadState(newState, ts);
            if(newState is RoomState newRoomState)
            {
                // no op
            }
            else
            {
                throw new InvalidCastException("not a RoomState");
            }
        }

        protected override ResourceState MakeState(ulong ts)
        {
            return new RoomState(this, ts);
        }

        public class RoomState : ResourceState
        {
            public RoomState() : base() { }
            public RoomState(RoomSession resource, ulong ts) : base(resource, ts) { }

            public override StateType stateType => StateType.RoomState;
        }
    }
}