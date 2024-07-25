﻿using CG.GameLoopStateMachine;
using CG.GameLoopStateMachine.GameStates;
using CG.Input;
using System.Collections;
using System.Collections.Generic;
using UI.Matchmaking;
using UnityEngine;
using VoidManager.Utilities;

namespace Game_Browser
{
    internal class GameBrowserUI : MatchmakingHandler
    {
        #region Matchmaking Menu
        private void OnGUI()
        {
            if (GameStateMachine.Instance?.CurrentState is GSMainMenu)
            {
                if (GUI.Button(new Rect(10f, 47.5f, 30f, 60f), "M") && Time.time - updatetime != 1f)
                {
                    updatetime = Time.time;
                    guiActive = !guiActive;
                }
            }
            else guiActive = false;
            if (guiActive)
            {
                WindowPos = new Rect(Screen.width / 4f, Screen.height / 4f, Screen.width / 2f, Screen.height / 2f);
                GUI.Window(14290, WindowPos, new GUI.WindowFunction(WindowFunction), "Game Browser");
            }
        }
        private void WindowFunction(int WindowID)
        {
            if (GUILayout.Button("Close")) guiActive = false;
            if (MatchmakingHandler.Instance == null) return;
            GUILayout.BeginHorizontal();
            GUITools.DrawCheckbox("Show Full Rooms", ref Config.showFullRooms);
            GUITools.DrawCheckbox("Show Empty Rooms", ref Config.showEmptyRooms);
            GUILayout.EndHorizontal();
            List<MatchmakingRoom> roomList = MatchmakingHandler.Instance.GetRooms(Config.showFullRooms.Value, Config.showEmptyRooms.Value);
            GUILayout.Label($"Found {roomList.Count} rooms");
            foreach (MatchmakingRoom roomInfo in roomList)
            {
                if (GUILayout.Button($"{roomInfo.RoomName} | {roomInfo.CurrentPlayers} / {roomInfo.MaxPlayers}")) selectedRoom = roomInfo;
            }
        }
        private MatchmakingRoom selectedRoom;
        private bool guiActive = false;
        private float updatetime;
        private Rect WindowPos;
        #endregion
        #region Matchmaking Methods

        #endregion
    }
}
