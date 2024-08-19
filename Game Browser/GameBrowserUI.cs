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
using UnityEngine.UI;
using UI.Settings;

namespace Game_Browser
{
    internal class GameBrowserUI : MatchmakingHandler
    {
        #region Matchmaking Menu
        GameObject Background;
        GameObject GBUICanvas;
        Image Image;
        private void Start()
        {
            WindowPos = new Rect(Screen.width / 4f, Screen.height / 4f, Screen.width / 2f, Screen.height / 2f);
            GBUICanvas = new GameObject("GameBrowserUICanvas", new Type[] { typeof(Canvas) });
            Canvas canvasComponent = GBUICanvas.GetComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComponent.sortingOrder = 999;
            canvasComponent.transform.SetAsLastSibling();
            DontDestroyOnLoad(GBUICanvas);


            //Background image to block mouse clicks passing IMGUI
            Background = new GameObject("GameBrowserUIBG", new Type[] { typeof(GraphicRaycaster) });
            Image = Background.AddComponent<UnityEngine.UI.Image>();
            Image.color = Color.clear;
            Background.transform.SetParent(GBUICanvas.transform);
            Background.SetActive(false);
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
                GUI.skin = ChangeSkin();
                WindowPos = GUI.Window(14290, WindowPos, WindowFunction, "Game Browser");
                if (Image != null)
                {
                    Image.rectTransform.position = new Vector3(WindowPos.center.x, (WindowPos.center.y * -1) + Screen.height, 0);
                    Image.rectTransform.sizeDelta = WindowPos.size;
                }
                if (Background != null) Background.SetActive(true);
            }
            else
            {
                if (Background != null) Background.SetActive(false);
            }
        }
        private static FieldInfo InstanceInfo = AccessTools.Field(typeof(MatchmakingHandler), "_instance");
        private static FieldInfo stateInfo = AccessTools.Field(typeof(MatchmakingHandler), "state");
        private static FieldInfo roomsInfo = AccessTools.Field(typeof(MatchmakingHandler), "rooms");
        /*
        public enum RoomFetchState
	    {
		    Disconnected,
		    ConnectingToMasterServer,
		    ConnectingToLobby,
		    UpdatingRooms
	    }
        */
        private void Update()
        {
            if (guiActive && !retrievingRooms) //OnEnable
            {
                PhotonNetwork.AddCallbackTarget(this);
                MatchmakingHandler.Instance.StartRetrievingRooms();
                retrievingRooms = true;
            }
            else if (!guiActive && retrievingRooms) //OnDisable()
            {
                PhotonNetwork.RemoveCallbackTarget(this);
                MatchmakingHandler.Instance.StopRetrievingRooms();
                retrievingRooms = false;
            }
            if (MatchmakingHandler.Instance == null) InstanceInfo.SetValue(this, this);
        }
        private void WindowFunction(int WindowID)
        {
            if (GUILayout.Button("Close")) guiActive = false;
            if (MatchmakingHandler.Instance == null || PhotonService.Instance?.CurrentRegion() == null) return;

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
            GUI.DragWindow();
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
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUILayout.BeginHorizontal();
            for (int i = 0; i < strings.Count; i++)
            {
                GUI.Label(new Rect(buttonRect.x + (i * AverageWidth), buttonRect.y, AverageWidth, buttonRect.height), $"{strings[i]}");
            }
            GUILayout.EndHorizontal();
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
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

        internal static GUISkin _cachedSkin;
        internal static GUIStyle _SelectedButtonStyle;
        internal static Texture2D _buttonBackground;
        internal static Texture2D _hbuttonBackground;
        private static readonly Color32 _classicMenuBackground = new Color32(32, 32, 32, 255);
        private static readonly Color32 _classicButtonBackground = new Color32(40, 40, 40, 255);
        //private static readonly Color32 _hoverButtonFromMenu = new Color32(18, 79, 179, 255);
        internal GUISkin ChangeSkin()
        {
            if (_cachedSkin is null || _cachedSkin.window.active.background is null)
            {
                _cachedSkin = Instantiate(GUI.skin);
                Texture2D windowBackground = BuildTexFrom1Color(_classicMenuBackground);
                _cachedSkin.window.active.background = windowBackground;
                _cachedSkin.window.onActive.background = windowBackground;
                _cachedSkin.window.focused.background = windowBackground;
                _cachedSkin.window.onFocused.background = windowBackground;
                _cachedSkin.window.hover.background = windowBackground;
                _cachedSkin.window.onHover.background = windowBackground;
                _cachedSkin.window.normal.background = windowBackground;
                _cachedSkin.window.onNormal.background = windowBackground;

                _cachedSkin.window.hover.textColor = Color.white;
                _cachedSkin.window.onHover.textColor = Color.white;

                Color32 hoverbutton = new Color32(60, 60, 60, 255);

                _buttonBackground = BuildTexFrom1Color(_classicButtonBackground);
                _hbuttonBackground = BuildTexFrom1Color(hoverbutton);
                _cachedSkin.button.active.background = _buttonBackground;
                _cachedSkin.button.focused.background = _buttonBackground;
                _cachedSkin.button.hover.background = _hbuttonBackground;
                _cachedSkin.button.normal.background = _buttonBackground;
                //_cachedSkin.button.onActive.background = _buttonBackground;
                //_cachedSkin.button.onFocused.background = _buttonBackground;
                //_cachedSkin.button.onHover.background = _hbuttonBackground;
                //_cachedSkin.button.onNormal.background = _buttonBackground;

                //Remember to check out https://forum.unity.com/threads/focusing-gui-controls.20511/ and potentially replace this with better code.
                _SelectedButtonStyle = new GUIStyle(_cachedSkin.button);
                _SelectedButtonStyle.active.background = _hbuttonBackground;
                _SelectedButtonStyle.focused.background = _hbuttonBackground;
                _SelectedButtonStyle.normal.background = _hbuttonBackground;

                GUITools.ButtonMinSizeStyle = new GUIStyle(_cachedSkin.button);
                GUITools.ButtonMinSizeStyle.stretchWidth = false;

                Texture2D sliderBackground = BuildTexFrom1Color(new Color32(47, 79, 79, 255));
                _cachedSkin.horizontalSlider.active.background = sliderBackground;
                _cachedSkin.horizontalSlider.onActive.background = sliderBackground;
                _cachedSkin.horizontalSlider.focused.background = sliderBackground;
                _cachedSkin.horizontalSlider.onFocused.background = sliderBackground;
                _cachedSkin.horizontalSlider.hover.background = sliderBackground;
                _cachedSkin.horizontalSlider.onHover.background = sliderBackground;
                _cachedSkin.horizontalSlider.normal.background = sliderBackground;
                _cachedSkin.horizontalSlider.onNormal.background = sliderBackground;

                Texture2D sliderHandleBackground = BuildTexFrom1Color(new Color32(47, 79, 79, 255));
                _cachedSkin.horizontalSliderThumb.active.background = sliderHandleBackground;
                _cachedSkin.horizontalSliderThumb.onActive.background = sliderHandleBackground;
                _cachedSkin.horizontalSliderThumb.focused.background = sliderHandleBackground;
                _cachedSkin.horizontalSliderThumb.onFocused.background = sliderHandleBackground;
                _cachedSkin.horizontalSliderThumb.hover.background = sliderHandleBackground;
                _cachedSkin.horizontalSliderThumb.onHover.background = sliderHandleBackground;
                _cachedSkin.horizontalSliderThumb.normal.background = sliderHandleBackground;
                _cachedSkin.horizontalSliderThumb.onNormal.background = sliderHandleBackground;

                Texture2D textfield = BuildTexFromColorArray(new Color[] { _classicButtonBackground, _classicButtonBackground, _classicMenuBackground,
                _classicMenuBackground, _classicMenuBackground, _classicMenuBackground , _classicMenuBackground}, 1, 7);
                _cachedSkin.textField.active.background = textfield;
                _cachedSkin.textField.onActive.background = textfield;
                _cachedSkin.textField.focused.background = textfield;
                _cachedSkin.textField.onFocused.background = textfield;
                _cachedSkin.textField.hover.background = textfield;
                _cachedSkin.textField.onHover.background = textfield;
                _cachedSkin.textField.normal.background = textfield;
                _cachedSkin.textField.onNormal.background = textfield;

                _cachedSkin.textField.active.textColor = hoverbutton;
                _cachedSkin.textField.onActive.textColor = hoverbutton;
                _cachedSkin.textField.hover.textColor = hoverbutton;
                _cachedSkin.textField.onHover.textColor = hoverbutton;

                UnityEngine.Object.DontDestroyOnLoad(windowBackground);
                UnityEngine.Object.DontDestroyOnLoad(_buttonBackground);
                UnityEngine.Object.DontDestroyOnLoad(_hbuttonBackground);
                UnityEngine.Object.DontDestroyOnLoad(textfield);
                UnityEngine.Object.DontDestroyOnLoad(_cachedSkin);
                // TODO: Add custom skin for Toggle and other items
            }

            Texture2D BuildTexFrom1Color(Color color)
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, color);
                tex.Apply();
                return tex;
            }

            Texture2D BuildTexFromColorArray(Color[] color, int width, int height)
            {
                Texture2D tex = new Texture2D(width, height);
                tex.SetPixels(color);
                tex.Apply();
                return tex;
            }
            return _cachedSkin;
        }
        #endregion
    }
}
