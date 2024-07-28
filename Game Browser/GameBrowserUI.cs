using CG.GameLoopStateMachine.GameStates;
using CG.GameLoopStateMachine;
using System;
using System.Collections.Generic;
using ToolClasses;
using UnityEngine;
using VoidManager.Utilities;
using HarmonyLib;
using System.Reflection;
using System.Text.RegularExpressions;
using Photon.Pun;
using System.Threading.Tasks;

namespace Game_Browser
{
    internal class GameBrowserUI : MatchmakingHandler
    {
        #region Matchmaking Menu
        private void Start()
        {
            WindowPos = new Rect(Screen.width / 4f, Screen.height / 4f, Screen.width / 2f, Screen.height / 2f);
        }
        private void OnGUI()
        {
            if (MenuScreenController.Instance?.OpenCount == 1)
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
                GUI.Window(14290, WindowPos, new GUI.WindowFunction(WindowFunction), "Game Browser");
            }
        }
        private static FieldInfo InstanceInfo = AccessTools.Field(typeof(MatchmakingHandler), "_instance");
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
            if (MatchmakingHandler.Instance == null) InstanceInfo.SetValue(this, this);
        }
        private void WindowFunction(int WindowID)
        {
            if (GUILayout.Button("Close")) guiActive = false;
            if (MatchmakingHandler.Instance == null) return;

            List<MatchmakingRoom> roomList = MatchmakingHandler.Instance.GetRooms(Config.showFullRooms.Value, Config.showEmptyRooms.Value);
            if (!roomList.Contains(selectedRoom)) selectedRoom = null;
            GUILayout.BeginHorizontal();
            string pattern = @"\[.*?\]:";
            string region = Regex.Replace(PhotonService.Instance?.CurrentRegion()?.ToString(), pattern, string.Empty);
            GUILayout.Label($"Current Region: {region}");
            GUILayout.Label($"Found {roomList.Count} rooms");
            GUILayout.EndHorizontal();

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
            if (roomList.Count != 0)
            {
                GUILayout.BeginScrollView(scrollPosition);
                foreach (MatchmakingRoom roomInfo in roomList)
                {
                    if (roomInfo == null) return;
                    Rect buttonRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(35));
                    if (GUI.Button(buttonRect, GUIContent.none))
                    {
                        selectedRoom = roomInfo;
                    }
                    FormattedRect(buttonRect, new List<string>() { roomInfo.RoomName, $"{roomInfo.CurrentPlayers} / {roomInfo.MaxPlayers}", $"{roomInfo.AverageRank}", roomInfo.QuestDifficulty, roomInfo.SystemName, roomInfo.ShipName, $"{(roomInfo.InHub ? "In Hub" : "Quest")}" });
                }
                GUILayout.EndScrollView();

                if (selectedRoom != null)
                {
                    GUILayout.Label("<b>Room Info</b>");
                    Rect buttonRect2 = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(35));
                    FormattedRect(buttonRect2, new List<string>() { selectedRoom.RoomName, $"{selectedRoom.CurrentPlayers} / {selectedRoom.MaxPlayers}", $"{selectedRoom.AverageRank}", selectedRoom.QuestDifficulty, selectedRoom.SystemName, selectedRoom.ShipName, $"{(selectedRoom.InHub ? "In Hub" : "Quest")}" });
                    if (GUILayout.Button("Join Game"))
                    {
                        JoinRequested();
                    }
                }
            }
            else
            {
                GUILayout.BeginScrollView(scrollPosition);
                GUILayout.Label($"{(retrievingRooms ? "Searching for rooms . . ." : "Unable to retrieve rooms" )}");
                GUILayout.EndScrollView();
            }
            if (GUILayout.Button("Create Hub"))
            {
                JoinHub();
            }
        }
        private Vector2 scrollPosition;
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
        #region Matchmaking Menu Methods
        private void JoinRequested()
        {
            if (selectedRoom == null)
            {
                //this.failPopup.Show(AbstractDataTable<UIHelperData>.Instance.RoomDoesNotExist.String);
                return;
            }
            MatchmakingHandler.RoomJoinStatus roomJoinStatus = MatchmakingHandler.Instance.JoinGame(selectedRoom.RoomId);
            switch (roomJoinStatus)
            {
                case MatchmakingHandler.RoomJoinStatus.RoomFull:
                    //this.failPopup.Show(AbstractDataTable<UIHelperData>.Instance.RoomFull.String);
                    return;
                case MatchmakingHandler.RoomJoinStatus.RoomDoesNotExist:
                    //this.failPopup.Show(AbstractDataTable<UIHelperData>.Instance.RoomDoesNotExist.String);
                    return;
                case MatchmakingHandler.RoomJoinStatus.RoomIsPrivate:
                    //this.failPopup.Show(AbstractDataTable<UIHelperData>.Instance.RoomPrivate.String);
                    return;
                default:
                    return;
            }
        }
        private async void JoinHub()
        {
            if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("R_IH"))
            {
                Singleton<GameStateMachine>.Instance.ChangeState<GSQuitFromMenu>();
                Singleton<MenuControllerUGUI>.I.CloseTopMenu();

                // Wait asynchronously until the PhotonNetwork.CurrentRoom is null
                await WaitForRoomToBeNullAsync();
            }
            Singleton<SteamService>.I.CreateLobby().Then(() => PunSingleton<PhotonService>.I.CreateRoom(false)).Then(new Action(this.LaunchHub));
        }
        private async Task WaitForRoomToBeNullAsync()
        {
            while (!(Singleton<GameStateMachine>.Instance.CurrentState is GSMainMenu))
            {
                await Task.Delay(100); // Check every 100ms
            }
        }
        private void LaunchHub()
        {
            GameSessionManager.Instance.SetNextGameSession(GameSessionLoadPromise.LoadLobbyData());
            Singleton<GameStateMachine>.Instance.ChangeState<GSLoadingGame>();
        }
        #endregion
        #region Matchmaking Menu Formatting

        private void FormattedRect(Rect buttonRect, List<string> strings)
        {
            float AverageWidth = buttonRect.width / strings.Count;
            GUILayout.BeginHorizontal();
            for (int i = 0; i < strings.Count; i++)
            {
                GUI.Label(new Rect(buttonRect.x + (i * AverageWidth), buttonRect.y, AverageWidth, buttonRect.height), $"{strings[i]}");
            }
            GUILayout.EndHorizontal();
        }

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
