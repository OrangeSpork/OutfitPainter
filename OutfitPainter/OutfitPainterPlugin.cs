using AIChara;
using BepInEx;
using CharaCustom;
using HarmonyLib;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Studio.UI;
using KKAPI.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace OutfitPainter
{
    [BepInPlugin(GUID, PluginName, Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency("com.deathweasel.bepinex.materialeditor")]
    public class OutfitPainterPlugin : BaseUnityPlugin
    {

        public const string GUID = "orange.spork.outfitpainter";
        public const string PluginName = "Outfit Painter";
        public const string Version = "1.0.1";

        public static OutfitPainterPlugin Instance { get; set; }

        internal BepInEx.Logging.ManualLogSource Log => Logger;

        private OutfitPainterMakerGUI MakerGUI { get; set; }

        public ToolbarToggle StudioGUIToolbarToggle { get; set; }

        public OutfitPainterPlugin()
        {
            if (Instance != null)
                throw new InvalidOperationException("Singleton only.");

            Instance = this;

            var harmony = new Harmony(GUID);
            harmony.Patch(typeof(CvsC_Clothes).GetMethod("UpdateCustomUI"), null, new HarmonyMethod(typeof(OutfitPainterMakerGUI).GetMethod("ClothesSelectionChanged", AccessTools.all)));
            harmony.Patch(typeof(ChaControl).GetMethod("ChangeClothes", new Type[] { typeof(int), typeof(int), typeof(bool) }), null, new HarmonyMethod(typeof(OutfitPainterMakerGUI).GetMethod("ClothesTypeChanged", AccessTools.all)));

#if DEBUG
            Log.LogInfo("Outfit Painter Loaded.");
#endif
        }

        private static MethodInfo initBaseCustomTextureClothesMethod = AccessTools.Method(typeof(ChaControl), "InitBaseCustomTextureClothes");
        private static MethodInfo releaseBaseCustomTextureClothesMethod = AccessTools.Method(typeof(ChaControl), "ReleaseBaseCustomTextureClothes");
        public static void InitCustomClothes(ChaControl character)
        {
            for (int part = 0; part < 8; part++)
            {
                bool result = (bool)initBaseCustomTextureClothesMethod.Invoke(character, new object[] { part });
#if DEBUG
                Instance.Log.LogInfo($"Initialized {character.fileParam.fullname} Slot {part}: {result}");
#endif
            }
        }

        public static void ReleaseCustomClothes(ChaControl character)
        {
            for (int part = 0; part < 8; part++)
            {
                releaseBaseCustomTextureClothesMethod.Invoke(character, new object[] { part, true });
            }
        }

        public void Start()
        {

            CharacterApi.RegisterExtraBehaviour<OutfitPainterCharacterController>(GUID);

            MakerGUI = new OutfitPainterMakerGUI();
            MakerGUI.RegisterMakerAPIControls();

            if (KKAPI.Studio.StudioAPI.InsideStudio)
            {
                gameObject.AddComponent<OutfitPainterStudioGUI>();

                Texture2D gIconTex = new Texture2D(32, 32);
                byte[] texData = ResourceUtils.GetEmbeddedResource("outfit_painter_studio.png");
                ImageConversion.LoadImage(gIconTex, texData);
                StudioGUIToolbarToggle = KKAPI.Studio.UI.CustomToolbarButtons.AddLeftToolbarToggle(gIconTex, false, active => {
                    OutfitPainterStudioGUI.Instance.enabled = active;
                });
            }


        }







       





       

        

       

    }
}
