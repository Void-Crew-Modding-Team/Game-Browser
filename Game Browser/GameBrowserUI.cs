using CG.GameLoopStateMachine;
using CG.GameLoopStateMachine.GameStates;
using CG.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Game_Browser
{
    internal class GameBrowserUI : MonoBehaviour, IShowCursorSource, IInputActionMapRequest
    {
        /*private void Update()
        {
            bool flag = PauseManager.IsPaused != this.guiActive;
            if (flag)
            {
                InputActionMapRequests.RemoveRequest(this);
                this.guiActive = PauseManager.IsPaused;
                bool flag2 = this.guiActive;
                if (flag2)
                {
                    this.WindowPos = new Rect((float)Screen.width / 4f, (float)Screen.height / 4f, (float)Screen.width / 2f, (float)Screen.height / 2f);
                }
                CursorUtility.ShowCursor(this, this.guiActive);
            }
        }*/
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
                //InputActionMapRequests.AddOrChangeRequest(this, InputStateRequestType.UI);
                WindowPos = new Rect(Screen.width / 4f, Screen.height / 4f, Screen.width / 2f, Screen.height / 2f);
                GUI.Window(14290, WindowPos, new GUI.WindowFunction(WindowFunction), "Game Browser");
            }
        }
        private void WindowFunction(int WindowID)
        {
            GUILayout.Label("Hello!");
            if (GUILayout.Button("Close")) guiActive = false;

        }
        private bool guiActive = false;
        private float updatetime;
        private Rect WindowPos;
    }
}
