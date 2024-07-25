using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Browser
{
    [HarmonyPatch(typeof(MatchmakingHandler), "StopRetrievingRooms")]
    internal class StopRetrievingRoomsPatch
    {
        private static bool Prefix()
        {
            if (GameBrowserUI.GuiActive) return false;
            return true;
        }
    }
}
