﻿using IL.RWCustom;
using System;
using System.Collections.Generic;
using Kittehface.Framework20;
using IL.Menu;
using System.Linq;

namespace RainMeadow
{
    public static class RPCs
    {
        [RPCMethod]
        public static void DeltaReset(RPCEvent rpcEvent, OnlineResource onlineResource, OnlineEntity.EntityId entity)
        {
            RainMeadow.Debug($"from {rpcEvent.from} resource {onlineResource} entity {entity}");
            if (entity != null)
            {
                foreach (var feed in OnlineManager.feeds)
                {
                    if (feed.player == rpcEvent.from && feed.entity.id == entity && feed.resource == onlineResource)
                    {
                        feed.ResetDeltas();
                        return;
                    }
                }
            }
            else
            {
                foreach (var subscription in OnlineManager.subscriptions)
                {
                    if (subscription.player == rpcEvent.from && subscription.resource == onlineResource)
                    {
                        subscription.ResetDeltas();
                        return;
                    }
                }
            }
        }

        [RPCMethod]
        public static void AddFood(short add)
        {
            ((RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame)?.Players[0].realizedCreature as Player).AddFood(add);
        }

        [RPCMethod]
        public static void AddQuarterFood()
        {
            ((RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame)?.Players[0].realizedCreature as Player).AddQuarterFood();
        }

        [RPCMethod]
        public static void AddMushroomCounter()
        {
            ((RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame)?.Players[0].realizedCreature as Player).mushroomCounter += 320;
        }

        [RPCMethod]
        public static void ReinforceKarma()
        {
            ((RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame)?.session as StoryGameSession).saveState.deathPersistentSaveData.reinforcedKarma = true;
        }

        [RPCMethod]
        public static void PlayReinforceKarmaAnimation()
        {
            (RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame).cameras[0].hud.karmaMeter.reinforceAnimation = 0;
        }

        [RPCMethod]
        public static void InitGameOver()
        {
            var player = ((RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame)?.Players[0]);
            (RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame)?.cameras[0].hud.InitGameOverMode(null, 0, player.pos.room, new UnityEngine.Vector2(0f, 0f));
        }


        [RPCMethod]
        public static void IncrementPlayersLeftt()
        {
            if (RainMeadow.isArenaMode(out var arena))
            {
                arena.clientWaiting = arena.clientWaiting+1;

            }

        }

        [RPCMethod]
        public static void ResetPlayersLeft()
        {
            if (RainMeadow.isArenaMode(out var arena))
            {
                arena.clientWaiting = 0;

            }

        }

        [RPCMethod]
        public static void MovePlayersToDeathScreen()
        {
            foreach (OnlinePlayer player in OnlineManager.players)
            {
                if (!player.OutgoingEvents.Any(e => e is RPCEvent rpc && rpc.IsIdentical(RPCs.GoToDeathScreen))) {
                    player.InvokeRPC(RPCs.GoToDeathScreen);
                }
            }
        }

        [RPCMethod]
        public static void GoToDeathScreen()
        {
            var game = (RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame);
            if (game == null || game.manager.upcomingProcess != null)
            {
                return;
            }
            if (game.IsStorySession && game.GetStorySession.RedIsOutOfCycles && !game.rainWorld.ExpeditionMode)
            {
                game.GoToRedsGameOver();
                return;
            }
            game.GetStorySession.saveState.SessionEnded(game, false, false);
            game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.DeathScreen);
        }

        [RPCMethod]
        public static void MovePlayersToWinScreen(bool malnourished, string denPos)
        {
            RainMeadow.Debug($"MovePlayersToWinScreen({malnourished}, {denPos}");
            if (denPos == null) {
                var game = (RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame);
                if (game == null || game.manager.upcomingProcess != null)
                {
                    return;
                }
                denPos = game.world.GetAbstractRoom(game.FirstAlivePlayer.pos).name;
            }
            foreach (OnlinePlayer player in OnlineManager.players)
            {
                if (!player.OutgoingEvents.Any(e => e is RPCEvent rpc && rpc.IsIdentical(RPCs.GoToWinScreen, malnourished, denPos)))
                {
                    player.InvokeRPC(RPCs.GoToWinScreen, malnourished, denPos);
                }
            }
        }

        //Assumed to be called for storymode only
        [RPCMethod]
        public static void GoToWinScreen(bool malnourished, string denPos)
        {
            RainMeadow.Debug($"GoToWinScreen({malnourished}, {denPos}");
            var game = (RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame);

            if (game == null)
            {
                RainMeadow.Debug($"game is null");
                return;
            }

            if (game.manager.upcomingProcess != null)
                RainMeadow.Debug($"game has upcomming process");

            if (!malnourished && !game.rainWorld.saveBackedUp)
            {
                game.rainWorld.saveBackedUp = true;
                game.rainWorld.progression.BackUpSave("_Backup");
            }

            if (OnlineManager.lobby.isOwner) {
                if (denPos != null) {
                    game.GetStorySession.saveState.denPosition = denPos;
                }
                game.GetStorySession.saveState.SessionEnded(game, true, malnourished);
            }

            //TODO: need to sync p5 and l2m deam events. Not doing it rn.
            DreamsState dreamsState = game.GetStorySession.saveState.dreamsState;

            if (dreamsState != null)
            {
                dreamsState.EndOfCycleProgress(game.GetStorySession.saveState, game.world.region.name, denPos);
                if (dreamsState.AnyDreamComingUp && !malnourished) {
                    game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Dream);
                    return;
                }
            }
            RainMeadow.Debug("I am moving to the sleepscreen");
            game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SleepScreen);
        }


        [RPCMethod]
        public static void MovePlayersToGhostScreen(string ghostID)
        {
            foreach (OnlinePlayer player in OnlineManager.players)
            {
                player.InvokeRPC(RPCs.GoToGhostScreen, ghostID);
            }
        }

        [RPCMethod]
        public static void GoToGhostScreen(string ghostID)
        {
            //For MSC support, we'll need to add a check for artificer campaign and send it to the VengeanceGhostScreen
            var game = (RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame);
            if (game.manager.upcomingProcess != null)
            {
                return;
            }
            ExtEnumBase.TryParse(typeof(GhostWorldPresence.GhostID), ghostID, false, out var rawEnumBase);
            game.sawAGhost = rawEnumBase as GhostWorldPresence.GhostID;
            game.GetStorySession.AppendTimeOnCycleEnd(true);
            if (game.GetStorySession.saveState.deathPersistentSaveData.karmaCap < 9)
            {
                game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.GhostScreen);
                return;
            }
            game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.KarmaToMaxScreen);
        }

        [RPCMethod]
        public static void Arena_NextLevelCall()
        {
            var game = (RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame);
            if (game.manager.upcomingProcess != null)
            {
                return;
            }
            game.GetArenaGameSession.arenaSitting.NextLevel(game.manager);
            game.arenaOverlay.nextLevelCall = true;
        }

        [RPCMethod]
        public static void AddShortCutVessel(RWCustom.IntVector2 pos, OnlinePhysicalObject crit, RoomSession roomSess, int wait)
        {

            var game = (RWCustom.Custom.rainWorld.processManager.currentMainLoop as RainWorldGame);
            if (game.manager.upcomingProcess != null)
            {
                return;
            }
            var creature = (crit?.apo.realizedObject as Creature);
            var room = roomSess.absroom.world;
            var roomPos = room.GetAbstractRoom(0);
            var shortCutVessel = new ShortcutHandler.ShortCutVessel(pos, creature, roomPos, wait);
            game.GetArenaGameSession.exitManager.playersInDens.Add(shortCutVessel);

        }

    }
}
