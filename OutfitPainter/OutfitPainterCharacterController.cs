using AIChara;
using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Studio;
using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OutfitPainter
{
    public class OutfitPainterCharacterController : CharaCustomFunctionController
    {

        BepInEx.Logging.ManualLogSource Log = OutfitPainterPlugin.Instance.Log;

        public OutfitPainterData Data { get; set; }

        private bool initializedCustomClothes = false;

        protected override void OnEnable()
        {
            base.OnEnable();

            Data = new OutfitPainterData();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            var data = new PluginData();
            data.data["OutfitPainterData"] = MessagePackSerializer.Serialize(Data);
            SetExtendedData(data);
        }

        protected override void OnCoordinateBeingSaved(ChaFileCoordinate coordinate)
        {
            var data = new PluginData();
            data.data["OutfitPainterData"] = MessagePackSerializer.Serialize(Data);
            SetCoordinateExtendedData(coordinate, data);
        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (!maintainState)
            {
                PluginData data = GetExtendedData();
                LoadData(data);
            }

            InitializeCustomClothes();

            if (MakerAPI.InsideAndLoaded)
                OutfitPainterMakerGUI.UpdateAccessoryOnReload();
        }

        protected override void OnCoordinateBeingLoaded(ChaFileCoordinate coordinate, bool maintainState)
        {
            if (!maintainState)
            {
                PluginData data = GetCoordinateExtendedData(coordinate);
                LoadData(data);
            }

            InitializeCustomClothes();

            if (MakerAPI.InsideAndLoaded)
                OutfitPainterMakerGUI.UpdateAccessoryOnReload();

        }

        private void ReleaseCustomClothes()
        {
            if (initializedCustomClothes && StudioAPI.InsideStudio)
            {
                OutfitPainterPlugin.ReleaseCustomClothes(ChaControl);
                initializedCustomClothes = false;
            }
        }

        public void CheckInitializeCustomClothes()
        {
            bool reinitialize = false;
            for (int part = 0; part < 8; part++)
            {
                for (int color = 0; color < 3; color++)
                {
                    if (Data != null && Data.FindChannelForAssignment(OutfitPainterData.SlotForClothesKind(part), -1, color + 1, false) != null || Data.FindChannelForAssignment(OutfitPainterData.SlotForClothesKind(part), -1, color + 1, true) != null)
                    {
                        if (ChaControl.IsClothesStateKind(part) && !IsCtCreateClothesInstantiated(part, color))
                        {
#if DEBUG
                            OutfitPainterPlugin.Instance.Log.LogInfo($"Lost initialization on {ChaControl.fileParam.fullname} slot {OutfitPainterData.SlotForClothesKind(part)} color {color + 1}");
#endif
                            reinitialize = true;
                            break;
                        }
                    }
                }

                if (reinitialize)
                    break;
            }

            if (reinitialize)
                InitializeCustomClothes();
        }

        private bool IsCtCreateClothesInstantiated(int part, int color)
        {
            if (ChaControl.ctCreateClothes == null)
            {
#if DEBUG
                OutfitPainterPlugin.Instance.Log.LogInfo($"No ctCreateClothes on {ChaControl.fileParam.fullname} slot {OutfitPainterData.SlotForClothesKind(part)} color {color + 1}");
#endif
                return false;
            }

            // Only the first texture is required for initialization - see InitBaseCustomTextureClothes

            if (ChaControl.ctCreateClothes[part, 0] == null)
            {
#if DEBUG
                OutfitPainterPlugin.Instance.Log.LogInfo($"No ctCreateClothes part/color on {ChaControl.fileParam.fullname} slot {OutfitPainterData.SlotForClothesKind(part)} color {color + 1}");
#endif
                return false;
            }

            if (ChaControl.ctCreateClothes[part, 0].texMain == null)
            {
#if DEBUG
                OutfitPainterPlugin.Instance.Log.LogInfo($"No texMain on {ChaControl.fileParam.fullname} slot {OutfitPainterData.SlotForClothesKind(part)} color {color + 1}");
#endif
                return false;
            } 

            if (ChaControl.ctCreateClothes[part, 0].matCreate == null)
            {
#if DEBUG
                OutfitPainterPlugin.Instance.Log.LogInfo($"No matCreate on {ChaControl.fileParam.fullname} slot {OutfitPainterData.SlotForClothesKind(part)} color {color + 1}");
#endif
                return false;
            }

            return true;
        }

        private Coroutine initializeClothesCoroutine;
        private void InitializeCustomClothes()
        {
            if (StudioAPI.InsideStudio && Data.ActiveChannels.Count > 0 && initializeClothesCoroutine == null)
            {
                initializeClothesCoroutine = StartCoroutine(DoInitializeCustomClothes());
            }
        }

        private IEnumerator DoInitializeCustomClothes()
        {
            yield return null;
            yield return null;

            OutfitPainterPlugin.InitCustomClothes(ChaControl);
            initializedCustomClothes = true;
            initializeClothesCoroutine = null;
        }


        private void LoadData(PluginData data)
        {
            if (data != null)
            {
                byte[] outfitPainterBytes = (byte[])data.data["OutfitPainterData"];
                if (outfitPainterBytes != null)
                {
                    Data = MessagePackSerializer.Deserialize<OutfitPainterData>(outfitPainterBytes);
                }
                else
                {
                    Data = new OutfitPainterData();
                   // Data.SetDefault();
                }

            }
            else
            {
                Data = new OutfitPainterData();
              //
              //
              //Data.SetDefault();
            }
        }
    }
}
