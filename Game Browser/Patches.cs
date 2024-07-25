using HarmonyLib;
using System;
using System.Collections.Generic;
using static VoidManager.Utilities.HarmonyHelpers;
using static HarmonyLib.AccessTools;
using System.Reflection.Emit;
using Photon.Pun;
using Photon.Realtime;

namespace Game_Browser
{
    [HarmonyPatch(typeof(MatchmakingHandler), "StopRetrievingRooms")]
    internal class StopRetrievingRoomsPatch
    {
        private static bool Prefix() => !GameBrowserUI.GuiActive;
    }

    [HarmonyPatch(typeof(MatchmakingHandler), "GetRooms")]
    class FixInRoomCheck
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return PatchBySequence(instructions,
            new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldfld, Field(typeof(MatchmakingRoom), "RoomId")),
                new CodeInstruction(OpCodes.Call, Method(typeof(PhotonNetwork), "get_CurrentRoom")),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(Room), "get_Name")),
                new CodeInstruction(OpCodes.Call, Method(typeof(String), "op_Inequality"))
            },
            new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Call, Method(typeof(FixInRoomCheck), "Patch")),
            }, PatchMode.REPLACE, CheckMode.ALWAYS, false);
            /*
             * Before: value.RoomId != PhotonNetwork.CurrentRoom.Name
             * After: value.RoomId != PhotonNetwork.CurrentRoom?.Name
             */
        }
        public static bool Patch(MatchmakingRoom value) => value.RoomId != PhotonNetwork.CurrentRoom?.Name;
    }
}
