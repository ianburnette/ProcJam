using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nothke.ProtoGUI
{
    public class ToolbarGUI : WindowGUI
    {
        public override string WindowLabel { get { return "Toolbar"; } }


        public bool collectOnStart = true;
        public List<GameWindow> windows;

        public int buttonWidth = 120;
        public int toolbarOffset = 30;
        public int toolbarHeight = 30;
        
        void Start()
        {
            draggable = false;
            windowRect.x = buttonWidth;
            windowRect.width = 0;

            if (collectOnStart)
            {
                windows = new List<GameWindow>(FindObjectsOfType<GameWindow>());

                windows.Remove(this);
            }
        }

        protected override void Window()
        {
            windowRect.y = toolbarOffset;
            windowRect.x = 0;
            windowRect.width = Screen.width;
            GUILayout.BeginHorizontal();
            for (int i = 0; i < windows.Count; i++)
            {
                if (GUILayout.Button(windows[i].WindowLabel, GUILayout.Width(buttonWidth))) {
                    for (int j = 0; j < windows.Count; j++) {
                        if (j == i)
                            windows[i].Enabled = !windows[i].Enabled;
                        else
                            windows[j].Enabled = false;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}