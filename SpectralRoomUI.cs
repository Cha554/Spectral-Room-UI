using BepInEx;
using GorillaNetworking;
using Photon.Pun;
using System;
using UnityEngine;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace SpectralRoomUI
{
    [BepInPlugin("com.cha.spectral.roomui", "SpectralRoomUI", "1.0.0")]
    public class Class : BaseUnityPlugin
    {
        private string joinRoom = ""; 
        private bool isVisible = true;

        private const float PANEL_X = 10f;
        private const float PANEL_Y = 10f;
        private const float PANEL_W = 660f;
        private const float PANEL_H = 52f;
        private const float BTN_W = 100f;
        private const float BTN_H = 32f;
        private const float INPUT_W = 130f;
        private const float ELEM_Y_OFF = 10f;
        private const float PADDING = 8f;

        private GUIStyle _panelStyle;
        private GUIStyle _btnStyle;
        private GUIStyle _inputStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _glowStyle;
        private GUIStyle _inputOutlineStyle;

        private static readonly Color Pink = new Color(1.00f, 0.27f, 0.65f, 1f);
        private static readonly Color Cyan = new Color(0.00f, 0.90f, 0.95f, 1f);
        private static readonly Color PinkSoft = new Color(1.00f, 0.55f, 0.78f, 0.85f);
        private static readonly Color CyanSoft = new Color(0.40f, 0.95f, 1.00f, 0.85f);
        private static readonly Color DarkBG = new Color(0.05f, 0.04f, 0.10f, 0.92f);
        private static readonly Color MidBG = new Color(0.10f, 0.08f, 0.18f, 0.95f);
        private static readonly Color White80 = new Color(1f, 1f, 1f, 0.90f);

        private void BuildStyles()
        {
            var panelTex = MakeTex(2, 2, DarkBG);
            var btnNormal = MakeGradientTex(128, 32, PinkSoft, CyanSoft);
            var btnHover = MakeGradientTex(128, 32, Pink, Cyan);
            var btnActive = MakeGradientTex(128, 32, Cyan, Pink);
            var inputTex = MakeTex(2, 2, MidBG);
            var glowTex = MakeGradientTex(4, 2, Pink, Cyan);

            _panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = panelTex, textColor = White80 },
                border = new RectOffset(6, 6, 6, 6),
                padding = new RectOffset(6, 6, 4, 4),
                alignment = TextAnchor.MiddleLeft,
            };

            _btnStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { background = btnNormal, textColor = Color.white },
                hover = { background = btnHover, textColor = Color.white },
                active = { background = btnActive, textColor = Color.white },
                focused = { background = btnHover, textColor = Color.white },
                font = GUI.skin.button.font,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(4, 4, 4, 4),
            };

            _inputStyle = new GUIStyle(GUI.skin.textArea)
            {
                normal = { background = inputTex, textColor = Cyan },
                focused = { background = inputTex, textColor = Color.white },
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(6, 6, 6, 6),
            };

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = Pink },
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };

            _glowStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = glowTex }
            };

            var outlineTex = MakeGradientTex(4, 2, Pink, Cyan);
            _inputOutlineStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = outlineTex }
            };
        }

        public void OnGUI()
        {
            if (_panelStyle == null) BuildStyles();

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F8)
                isVisible = !isVisible;

            if (!isVisible) return;

            GUI.Box(new Rect(PANEL_X - 2, PANEL_Y - 2, PANEL_W + 4, PANEL_H + 4), GUIContent.none, _glowStyle);
            GUI.Box(new Rect(PANEL_X, PANEL_Y, PANEL_W, PANEL_H), GUIContent.none, _panelStyle);

            float x = PANEL_X + PADDING;
            float y = PANEL_Y + ELEM_Y_OFF;

            GUI.Label(new Rect(x, y, 80f, BTN_H), "Spectral UI", _labelStyle); x += 88f;
            GUI.Box(new Rect(x - 2, y - 2, INPUT_W + 4, BTN_H + 4), GUIContent.none, _inputOutlineStyle); joinRoom = GUI.TextField(new Rect(x, y, INPUT_W, BTN_H), joinRoom, 24, _inputStyle); x += INPUT_W + PADDING;

            if (GUI.Button(new Rect(x, y, BTN_W, BTN_H), "Join Room", _btnStyle)) PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(joinRoom.ToUpper(), 0); x += BTN_W + PADDING;
            if (GUI.Button(new Rect(x, y, BTN_W, BTN_H), "Leave", _btnStyle)) PhotonNetwork.Disconnect(); x += BTN_W + PADDING;
            if (GUI.Button(new Rect(x, y, BTN_W + 20f, BTN_H), "Change Name", _btnStyle)) ChangeName(joinRoom); x += BTN_W + 20f + PADDING;
            if (GUI.Button(new Rect(x, y, 60f, BTN_H), "Clear", _btnStyle)) joinRoom = "";
        }

        private static Texture2D MakeTex(int w, int h, Color col)
        {
            var tex = new Texture2D(w, h);
            var pix = new Color[w * h];
            for (int i = 0; i < pix.Length; i++) pix[i] = col; tex.SetPixels(pix); tex.Apply(); return tex;
        }

        private static Texture2D MakeGradientTex(int w, int h, Color left, Color right)
        {
            var tex = new Texture2D(w, h);
            for (int x = 0; x < w; x++)
            {
                Color col = Color.Lerp(left, right, (float)x / (w - 1)); for (int y = 0; y < h; y++) tex.SetPixel(x, y, col);
            }
            tex.Apply();
            return tex;
        }

        public static void ChangeName(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName)) return;
            GorillaComputer.instance.currentName = playerName;
            GorillaComputer.instance.SetLocalNameTagText(playerName);
            GorillaComputer.instance.savedName = playerName;
            PlayerPrefs.SetString("playerName", playerName);
            PlayerPrefs.Save();
            PhotonNetwork.LocalPlayer.NickName = playerName;
        }
    }
}