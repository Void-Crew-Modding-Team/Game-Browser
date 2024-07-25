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
            if (GUILayout.Button("Room Name", GUILayout.ExpandWidth(true))) SortByColumn(SortColumn.RoomName);
            if (GUILayout.Button("Current Players", GUILayout.ExpandWidth(true))) SortByColumn(SortColumn.CurrentPlayers);
            if (GUILayout.Button("Max Players", GUILayout.ExpandWidth(true))) SortByColumn(SortColumn.MaxPlayers);
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

                GUILayout.BeginHorizontal();
                GUI.Label(new Rect(buttonRect.x, buttonRect.y, buttonRect.width / 3, buttonRect.height), $"     {roomInfo.RoomName}");
                GUI.Label(new Rect(buttonRect.x + buttonRect.width / 3, buttonRect.y, buttonRect.width / 3, buttonRect.height), $"     {roomInfo.CurrentPlayers} / {roomInfo.MaxPlayers}");
                GUI.Label(new Rect(buttonRect.x + 2 * buttonRect.width / 3, buttonRect.y, buttonRect.width / 3, buttonRect.height), $"     {roomInfo.MaxPlayers.ToString()}");
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
            CurrentPlayers,
            MaxPlayers
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
                    case SortColumn.CurrentPlayers:
                        result = x.CurrentPlayers.CompareTo(y.CurrentPlayers);
                        break;
                    case SortColumn.MaxPlayers:
                        result = x.MaxPlayers.CompareTo(y.MaxPlayers);
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
