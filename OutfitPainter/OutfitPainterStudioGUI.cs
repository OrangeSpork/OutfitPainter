using BepInEx.Logging;
using KKAPI.Studio;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;
using AIChara;
using Studio;
using System.Collections;

namespace OutfitPainter
{
    public class OutfitPainterStudioGUI : MonoBehaviour
    {
        private static ManualLogSource Log => OutfitPainterPlugin.Instance.Log;

        private static Rect windowRect = new Rect(120, 220, 505, 700);
        private static readonly GUILayoutOption expandLayoutOption = GUILayout.ExpandWidth(true);

        private static GUIStyle labelStyle;
        private static GUIStyle selectedButtonStyle;

        private static bool guiLoaded = false;

        private Vector2 scrollPosition = Vector2.zero;

        public static OutfitPainterStudioGUI Instance;

        public static void Show()
        {
            OutfitPainterPlugin.Instance.StudioGUIToolbarToggle.Value = true;
        }

        public static void Hide()
        {
            OutfitPainterPlugin.Instance.StudioGUIToolbarToggle.Value = false;
        }


        private void Awake()
        {
            Instance = this;
            enabled = false;
        }

        private void Start()
        {
            channelTextures = new Texture2D[16];
            channelInverseTextures = new Texture2D[16];

            for(int i = 0; i < 16; i++)
            {
                channelTextures[i] = new Texture2D(1, 1);
                channelInverseTextures[i] = new Texture2D(1, 1);
            }
        }

        private void Update()
        {

        }

        private void OnEnable()
        {

        }

        private void OnDestroy()
        { 
        }

        private ChaControl character;
        private OutfitPainterCharacterController controller;
        private OutfitPainterData data;
        

        private void OnGUI()
        {
            if (!guiLoaded)
            {
                labelStyle = new GUIStyle(UnityEngine.GUI.skin.label);
                selectedButtonStyle = new GUIStyle(UnityEngine.GUI.skin.button);

                selectedButtonStyle.fontStyle = FontStyle.Bold;
                selectedButtonStyle.normal.textColor = Color.red;

                labelStyle.alignment = TextAnchor.MiddleRight;
                labelStyle.normal.textColor = Color.white;

                windowRect.x = Mathf.Min(Screen.width - windowRect.width, Mathf.Max(0, windowRect.x));
                windowRect.y = Mathf.Min(Screen.height - windowRect.height, Mathf.Max(0, windowRect.y));

                guiLoaded = true;
            }

            IEnumerable<Studio.OCIChar> selectedCharacters = StudioAPI.GetSelectedCharacters();
            bool activeOutfit = false;
            if (selectedCharacters.Count() > 0)
            {                
                character = selectedCharacters.First().GetChaControl();                    
                controller = character.gameObject.GetComponent<OutfitPainterCharacterController>();
                controller.CheckInitializeCustomClothes();
                    
                if (controller != null)
                {
                    data = controller.Data;
                    activeOutfit = data.ActiveChannels.Count() > 0;
                }
            }

            KKAPI.Utilities.IMGUIUtils.DrawSolidBox(windowRect);

            string titleMessage = "Outfit Painter: ";
            if (character == null)
                titleMessage += "No Character Selected";
            else if (activeOutfit)
                titleMessage += $"{character.chaFile.parameter.fullname} - {data.ActiveChannels.Count()} Outfit Channels Defined";
            else
                titleMessage += $"{character.chaFile.parameter.fullname} - No Outfit Channels Defined";


            var rect = GUILayout.Window(8726, windowRect, DoDraw, $"Outfit Painter: {titleMessage}");
            windowRect.x = rect.x;
            windowRect.y = rect.y;

            if (windowRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
                Input.ResetInputAxes();
        }

        private Texture2D[] channelTextures;
        private Texture2D[] channelInverseTextures;

        private void DoDraw(int id)
        {            
            GUILayout.BeginVertical();
            {

                // Header
                GUILayout.BeginHorizontal(expandLayoutOption);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Close Me", GUILayout.ExpandWidth(false))) Hide();
                GUILayout.EndHorizontal();

                if (data != null && data.ActiveChannels != null)
                {
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                    // Display Channels
                    foreach (OutfitPainterChannel channel in data.ActiveChannels)
                    {
                        bool updateChannel = false;
                        GUILayout.BeginVertical();

                        // Color
                        GUILayout.BeginHorizontal(expandLayoutOption);

                        GUIStyle channelStyle = new GUIStyle(GUI.skin.button);
                        
                        channelTextures[channel.ChannelId - 1].SetPixel(0, 0, channel.ChannelDisplayColor);
                        channelTextures[channel.ChannelId - 1].Apply();
                        
                        channelInverseTextures[channel.ChannelId - 1].SetPixel(0, 0, channel.InverseChannelColor);
                        channelInverseTextures[channel.ChannelId - 1].Apply();

                        channelStyle.normal.background = channelTextures[channel.ChannelId - 1];                        
                        channelStyle.normal.textColor = channel.InverseChannelColor;
                        channelStyle.hover.textColor = channel.ChannelDisplayColor;
                        channelStyle.hover.background = channelInverseTextures[channel.ChannelId - 1];

                        if (GUILayout.Button(channel.ChannelDescription, channelStyle, GUILayout.ExpandWidth(true), GUILayout.MinHeight(50f), GUILayout.MaxWidth(250f)))
                        {
                            ColorPalette.Instance.Setup($"Channel {channel.ChannelDescription}", channel.ChannelColor, (c) => {
                                channel.ChannelColor = c;
                                channel.Update(character);
                            }, true);
                        };

                        GUILayout.BeginVertical();

                        // Shiny
                        GUILayout.BeginHorizontal(expandLayoutOption);
                        GUILayout.Label("Shine: ");
                        channel.ChannelGloss = GUILayout.HorizontalSlider(channel.ChannelGloss, -2, 2, GUILayout.MinWidth(150f));
                        int glossInt = (int)(channel.ChannelGloss * 100);
                        float glossFloat = float.Parse(GUILayout.TextField($"{glossInt}", GUILayout.MaxWidth(50f))) / 100f;
                        if (channel.ChannelGloss != glossFloat)
                        {
                            channel.ChannelGloss = glossFloat;
                            updateChannel = true;
                        }

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        // Metallic
                        GUILayout.BeginHorizontal(expandLayoutOption);
                        GUILayout.Label("Texture: ");                        
                        channel.ChannelMetallic = GUILayout.HorizontalSlider(channel.ChannelMetallic, -2, 2, GUILayout.MinWidth(150f));
                        int metalInt = (int)(channel.ChannelMetallic * 100);
                        float metalFloat = float.Parse(GUILayout.TextField($"{metalInt}", GUILayout.MaxWidth(50f))) / 100f;
                        if (channel.ChannelMetallic != metalFloat)
                        {
                            channel.ChannelMetallic = metalFloat;
                            updateChannel = true;
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.EndVertical();

                        GUILayout.EndHorizontal();


                        // Breakdown

                        GUILayout.Label($"Used By: {string.Join(",", channel.Assignments.Select(a => a.Description))}");

                        GUILayout.EndVertical();

                        if (updateChannel && !runningCoroutines.Contains(channel.ChannelId))
                        {
                            runningCoroutines.Add(channel.ChannelId);
                            StartCoroutine(DoUpdateChannel(character, channel));                            
                        }
                    }

                    GUILayout.EndScrollView();
                }
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private List<int> runningCoroutines = new List<int>();
        private IEnumerator DoUpdateChannel(ChaControl character, OutfitPainterChannel channel)
        {
            yield return new WaitForEndOfFrame();
            channel.Update(character);
            runningCoroutines.Remove(channel.ChannelId);
        }
    }
}
