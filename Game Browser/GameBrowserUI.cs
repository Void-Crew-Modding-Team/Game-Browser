using CG.GameLoopStateMachine;
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
            /*if (GameStateMachine.Instance?.CurrentState is GSMainMenu)
            {*/
                if (GUI.Button(new Rect(10f, 47.5f, 30f, 60f), "M") && Time.time - updatetime != 1f)
                {
                    updatetime = Time.time;
                    guiActive = !guiActive;
                }
            /*}
            else guiActive = false;*/
            if (guiActive)
            {
                WindowPos = new Rect(Screen.width / 4f, Screen.height / 4f, Screen.width / 2f, Screen.height / 2f);
                GUI.Window(14290, WindowPos, new GUI.WindowFunction(WindowFunction), "Game Browser");
            }
        }
        private void Update()
        {
            if (guiActive && !retrievingRooms)
            {
                MatchmakingHandler.Instance.StartRetrievingRooms();
                retrievingRooms = true;
            }
            else if (!guiActive && retrievingRooms)
            {
                MatchmakingHandler.Instance.StopRetrievingRooms();
                retrievingRooms = false;
            }
        }
        private void WindowFunction(int WindowID)
        {
            if (GUILayout.Button("Close")) guiActive = false;
            if (MatchmakingHandler.Instance == null) return;

            List<MatchmakingRoom> roomList = MatchmakingHandler.Instance.GetRooms(Config.showFullRooms.Value, Config.showEmptyRooms.Value);
            GUILayout.Label($"Found {roomList.Count} rooms");

            GUILayout.BeginHorizontal();
            GUITools.DrawCheckbox("Show Full Rooms", ref Config.showFullRooms);
            GUITools.DrawCheckbox("Show Empty Rooms", ref Config.showEmptyRooms);
            GUILayout.Label($"Retrieving Rooms: {retrievingRooms}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Game Name", GUILayout.ExpandWidth(true))) SortByColumn(SortColumn.RoomName);
            if (GUILayout.Button("Players", GUILayout.ExpandWidth(true))) SortByColumn(SortColumn.Players);
            if (GUILayout.Button("Avg Rank", GUILayout.ExpandWidth(true))) SortByColumn(SortColumn.AverageRank);
            if (GUILayout.Button("Difficulty", GUILayout.ExpandWidth(true))) SortByColumn(SortColumn.QuestDifficulty);
            if (GUILayout.Button("System", GUILayout.ExpandWidth(true))) SortByColumn(SortColumn.SystemName);
            if (GUILayout.Button("Ship", GUILayout.ExpandWidth(true))) SortByColumn(SortColumn.ShipName);
            if (GUILayout.Button("Status", GUILayout.ExpandWidth(true))) SortByColumn(SortColumn.InHub);
            GUILayout.EndHorizontal();
            roomList.Sort(new RoomComparer { SortBy = (SortColumn)Config.currentSortColumn.Value, Ascending = ascending });


            GUILayout.Space(10);
            foreach (MatchmakingRoom roomInfo in roomList)
            {
                if (roomInfo == null) return;
                Rect buttonRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(30));
                if (GUI.Button(buttonRect, GUIContent.none))
                {
                    selectedRoom = roomInfo;
                }
                float AverageWidth = buttonRect.width / 7;
                GUILayout.BeginHorizontal();
                GUI.Label(new Rect(buttonRect.x, buttonRect.y, AverageWidth, buttonRect.height), $"{roomInfo.RoomName}");
                GUI.Label(new Rect(buttonRect.x + AverageWidth, buttonRect.y, AverageWidth, buttonRect.height), $"{roomInfo.CurrentPlayers} / {roomInfo.MaxPlayers}");
                GUI.Label(new Rect(buttonRect.x + 2 * AverageWidth, buttonRect.y, AverageWidth, buttonRect.height), $"{roomInfo.AverageRank}");
                GUI.Label(new Rect(buttonRect.x + 3 * AverageWidth, buttonRect.y, AverageWidth, buttonRect.height), $"{roomInfo.QuestDifficulty}");
                GUI.Label(new Rect(buttonRect.x + 4 * AverageWidth, buttonRect.y, AverageWidth, buttonRect.height), $"{roomInfo.SystemName}");
                GUI.Label(new Rect(buttonRect.x + 5 * AverageWidth, buttonRect.y, AverageWidth, buttonRect.height), $"{roomInfo.ShipName}");
                GUI.Label(new Rect(buttonRect.x + 6 * AverageWidth, buttonRect.y, AverageWidth, buttonRect.height), $"{(roomInfo.InHub ? "In Hub" : "Quest")}");
                GUILayout.EndHorizontal();
            }
            GUI.DragWindow();
        }
        private MatchmakingRoom selectedRoom;
        private static bool guiActive = false;
        public static bool GuiActive
        {
            get { return guiActive; }
            private set { guiActive = value; }
        }
        private float updatetime;
        private bool retrievingRooms = false;
        private Rect WindowPos;
        #endregion
        #region Matchmaking Methods
        private enum SortColumn
        {
            RoomName,
            Players,
            AverageRank,
            QuestDifficulty,
            SystemName,
            ShipName,
            InHub
        }
        private class RoomComparer : IComparer<MatchmakingRoom>
        {
            public SortColumn SortBy { get; set; }
            public bool Ascending { get; set; }

            public int Compare(MatchmakingRoom x, MatchmakingRoom y)
            {
                int result = 0;
                switch (SortBy)
                {
                    case SortColumn.RoomName:
                        result = string.Compare(x.RoomName, y.RoomName);
                        break;
                    case SortColumn.Players:
                        result = x.CurrentPlayers.CompareTo(y.CurrentPlayers);
                        break;
                    case SortColumn.AverageRank:
                        result = x.AverageRank.CompareTo(y.AverageRank);
                        break;
                    case SortColumn.QuestDifficulty:
                        result = x.QuestDifficulty.CompareTo(y.QuestDifficulty);
                        break;
                    case SortColumn.SystemName:
                        result = x.SystemName.CompareTo(y.SystemName);
                        break;
                    case SortColumn.ShipName:
                        result = x.ShipName.CompareTo(y.ShipName);
                        break;
                    case SortColumn.InHub:
                        result = x.InHub.CompareTo(y.InHub);
                        break;
                }
                return Ascending ? result : -result;
            }
        }
        private void SortByColumn(SortColumn column)
        {
            if (Config.currentSortColumn.Value == (int)column)
            {
                ascending = !ascending;
            }
            else
            {
                Config.currentSortColumn.Value = (int)column;
                ascending = true;
            }
        }
        private bool ascending = true;
        #endregion
    }
}
