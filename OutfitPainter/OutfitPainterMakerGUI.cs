using AIChara;
using CharaCustom;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace OutfitPainter
{
    public class OutfitPainterMakerGUI
    {
        public void RegisterMakerAPIControls()
        {
            MakerAPI.RegisterCustomSubCategories += RegisterCustomControls;
            MakerAPI.ReloadCustomInterface += ReloadCustomInterface;

            AccessoriesApi.SelectedMakerAccSlotChanged += AccessorySelectionChanged;
            AccessoriesApi.AccessoryTransferred += AccessoryCopied;
            AccessoriesApi.AccessoryKindChanged += AccessoryTypeChanged;
        }

        public static void ClothesTypeChanged(ChaControl __instance, int kind)
        {
            if (dropdowns == null || dropdowns.Count == 0)
                return;

            ChaControl character = MakerAPI.GetCharacterControl();
            OutfitPainterCharacterController controller = character?.gameObject.GetComponent<OutfitPainterCharacterController>();
            controller?.Data.ClearSlot(OutfitPainterData.SlotForClothesKind(kind));
            UpdateOutfitPainterMakerGUI();
            for (int i = 0; i < 6; i++)
            {
                OutfitPainterChannelSelectionDropdown dropdown = dropdowns[i];
                int channelId = controller.Data.FindChannelIdForAssignment(OutfitPainterData.SlotForClothesKind(kind), -1, (i / 2) + 1, i % 2 == 1);
                dropdown.Value = channelId;
            }
        }

        
        public static void ClothesSelectionChanged(CvsC_Clothes __instance)
        {
            if (dropdowns == null || dropdowns.Count == 0)
                return;

            ChaControl character = MakerAPI.GetCharacterControl();
            OutfitPainterCharacterController controller = character?.gameObject.GetComponent<OutfitPainterCharacterController>();
            for (int i = 0; i < 6; i++)
            {
                OutfitPainterChannelSelectionDropdown dropdown = dropdowns[i];
                int channelId = controller.Data.FindChannelIdForAssignment(OutfitPainterData.SlotForClothesKind(__instance.SNo), -1, (i / 2) + 1, i % 2 == 1);
                dropdown.Value = channelId;
            }

        }

        private static int SelectedAccessorySlot = 0;
        private void AccessorySelectionChanged(object sender, AccessorySlotEventArgs eventArgs)
        {
            SelectedAccessorySlot = eventArgs.SlotIndex;
            ChaControl character = MakerAPI.GetCharacterControl();
            OutfitPainterCharacterController controller = character?.gameObject.GetComponent<OutfitPainterCharacterController>();
            for (int i = 6; i < dropdowns.Count; i++)
            {
                OutfitPainterChannelSelectionDropdown dropdown = dropdowns[i];
                int channelId = controller.Data.FindChannelIdForAssignment(OutfitPainterSlot.ACCESSORY, eventArgs.SlotIndex, i - 5, false);
                dropdown.Value = channelId;
            }
        }

        private void AccessoryTypeChanged(object sender, AccessorySlotEventArgs eventArgs)
        {
            ChaControl character = MakerAPI.GetCharacterControl();
            OutfitPainterCharacterController controller = character?.gameObject.GetComponent<OutfitPainterCharacterController>();
            controller?.Data.ClearAccessorySlot(eventArgs.SlotIndex);
            UpdateOutfitPainterMakerGUI();
            for (int i = 6; i < dropdowns.Count; i++)
            {
                OutfitPainterChannelSelectionDropdown dropdown = dropdowns[i];
                int channelId = controller.Data.FindChannelIdForAssignment(OutfitPainterSlot.ACCESSORY, eventArgs.SlotIndex, i - 5, false);
                dropdown.Value = channelId;
            }

        }

        public static void UpdateAccessoryOnReload()
        {
            int selectedSlot = AccessoriesApi.SelectedMakerAccSlot;
            if (selectedSlot == -1)
                return;

            ChaControl character = MakerAPI.GetCharacterControl();
            OutfitPainterCharacterController controller = character?.gameObject.GetComponent<OutfitPainterCharacterController>();
            for (int i = 6; i < dropdowns.Count; i++)
            {
                OutfitPainterChannelSelectionDropdown dropdown = dropdowns[i];
                int channelId = controller.Data.FindChannelIdForAssignment(OutfitPainterSlot.ACCESSORY, selectedSlot, i - 5, false);
                dropdown.Value = channelId;
            }
        }


        private void AccessoryCopied(object sender, AccessoryTransferEventArgs eventArgs)
        {
            ChaControl character = MakerAPI.GetCharacterControl();
            OutfitPainterCharacterController controller = character?.gameObject.GetComponent<OutfitPainterCharacterController>();
            controller?.Data.CopyAccessorySlot(eventArgs.SourceSlotIndex, eventArgs.DestinationSlotIndex);
            UpdateOutfitPainterMakerGUI();
        }

        private static MakerCategory makerCategory;
        public void RegisterCustomControls(object sender, RegisterSubCategoriesEvent e)
        {
            makerCategory = new MakerCategory(MakerConstants.Clothes.CategoryName, "Outfit Painter");
            e.AddSubCategory(makerCategory);

            ChaControl character = MakerAPI.GetCharacterControl();
            OutfitPainterCharacterController controller = character?.gameObject.GetComponent<OutfitPainterCharacterController>();

            InitializeOutfitPainterMakerGUI(controller?.Data);
        }

        public void ReloadCustomInterface(object sender, EventArgs e)
        {
            ChaControl character = MakerAPI.GetCharacterControl();
            OutfitPainterCharacterController controller = character.gameObject.GetComponent<OutfitPainterCharacterController>();

            UpdateOutfitPainterMakerGUI(controller?.Data);
        }

        public static bool UpdatingGUI { get; set; }
        private static List<OutfitPainterMakerChannelGUI> guiControls = new List<OutfitPainterMakerChannelGUI>();
        private static MakerText noneTextControl;
        private static List<OutfitPainterChannelSelectionDropdown> dropdowns = new List<OutfitPainterChannelSelectionDropdown>();
        private static CvsC_Clothes clothesController;
        public static void InitializeOutfitPainterMakerGUI(OutfitPainterData data)
        {
            UpdatingGUI = true;
            if (data == null)
            {
                data = new OutfitPainterData();
                //    data.SetDefault();
            }

            SelectedAccessorySlot = 0;

            OutfitPainterPlugin.Instance.Log.LogInfo($"Initializing Outfit Painter GUI");
            guiControls.Clear();
            dropdowns.Clear();

            noneTextControl = new MakerText("Assign Colors to Channels Then Use This Panel To Bulk Update", makerCategory, OutfitPainterPlugin.Instance);
            MakerAPI.AddControl(noneTextControl);

            List<string> activeChannels = new List<string>();
            activeChannels.Add("NONE");
            activeChannels.AddRange(data.FindActiveChannelDescriptions());

            if (data == null || data.ActiveChannels.Count == 0)
                noneTextControl.Visible.OnNext(true);
            else
                noneTextControl.Visible.OnNext(false);

            foreach (OutfitPainterChannel channel in data.Channels)
            {
                OutfitPainterMakerChannelGUI channelGUI = new OutfitPainterMakerChannelGUI();
                channelGUI.Initialize(channel, makerCategory, OutfitPainterPlugin.Instance, data);
                channelGUI.Update(channel, data);
                guiControls.Add(channelGUI);

            }

            OutfitPainterPlugin.Instance.StartCoroutine(OnMakerLoadingCo(data));
            UpdatingGUI = false;
        }

        public static void UpdateOutfitPainterMakerGUI()
        {
            ChaControl character = MakerAPI.GetCharacterControl();
            OutfitPainterCharacterController controller = character.gameObject.GetComponent<OutfitPainterCharacterController>();
            UpdateOutfitPainterMakerGUI(controller?.Data);
        }

        public static void UpdateOutfitPainterMakerGUI(OutfitPainterData data)
        {
            UpdatingGUI = true;
            if (data == null || data.ActiveChannels.Count == 0)
                noneTextControl.Visible.OnNext(true);
            else
                noneTextControl.Visible.OnNext(false);

            foreach (OutfitPainterMakerChannelGUI gui in guiControls)
            {
                gui.Update(data.FindChannelById(gui.Channel.ChannelId), data);
            }

            List<string> channels = new List<string>();
            channels.Add("None");
            channels.AddRange(data.FindChannelDescriptionsUnsorted());
            string[] channelNames = channels.ToArray();

            foreach (OutfitPainterChannelSelectionDropdown dropdown in dropdowns)
            {
                dropdown.UpdateOptions(channelNames);
            }
            UpdatingGUI = false;
        }

        private static IEnumerator OnMakerLoadingCo(OutfitPainterData data)
        {
            // Let maker objects run their Start methods
            yield return new WaitUntil(() => GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinClothes/DefaultWin/C_Clothes/Setting/Setting02/Scroll View/Viewport/Content") != null);
            yield return new WaitForEndOfFrame();

            UpdatingGUI = true;
            GameObject clothes = GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinClothes/DefaultWin/C_Clothes");
            clothesController = clothes.GetComponent<CvsC_Clothes>();

            GameObject go = GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinClothes/DefaultWin/C_Clothes/Setting/Setting02/Scroll View/Viewport/Content");

            List<string> channels = new List<string>();
            channels.Add("None");
            channels.AddRange(data.FindChannelDescriptionsUnsorted());
            string[] channelNames = channels.ToArray();

            OutfitPainterChannelSelectionDropdown dropdown = new OutfitPainterChannelSelectionDropdown("Color 1 Channel: ", channelNames, go.transform, 0);
            dropdown.Initialize();
            dropdown.CreateControl();
            dropdown.ControlObject.transform.SetSiblingIndex(4);
            dropdown.ValueChanged.Subscribe(Observer.Create<int>(i => UpdateChannelSelection(clothesController.SNo, -1, 1, false, i)));

            GameObject buttonHorizLayout = MakeHorizLayoutGO(go.transform);
            buttonHorizLayout.transform.SetSiblingIndex(5);

            OutfitPainterChannelButton syncC1FromChannel = new OutfitPainterChannelButton("Set From Channel", buttonHorizLayout.transform);
            syncC1FromChannel.Initialize();
            syncC1FromChannel.CreateControl();
            syncC1FromChannel.ControlObject.GetComponent<RectTransform>().offsetMin = new Vector2(25, -40);
            syncC1FromChannel.OnClick.AddListener(() => { SyncFromChannel(clothesController.SNo, -1, 1, false, syncC1FromChannel.ControlObject); });

            OutfitPainterChannelButton syncC1ToChannel = new OutfitPainterChannelButton("Sync To Channel", buttonHorizLayout.transform);
            syncC1ToChannel.Initialize();
            syncC1ToChannel.CreateControl();
            syncC1ToChannel.ControlObject.transform.GetChild(0).GetComponent<RectTransform>().offsetMax = new Vector2(225f, 1f);
            syncC1ToChannel.ControlObject.transform.GetChild(0).GetComponent<RectTransform>().offsetMin = new Vector2(50f, 1f);
            syncC1ToChannel.OnClick.AddListener(() => { SyncToChannel(clothesController.SNo, -1, 1, false); });

            dropdowns.Add(dropdown);

            dropdown = new OutfitPainterChannelSelectionDropdown("Pattern 1 Channel: ", channelNames, go.transform, 0);
            dropdown.Initialize();
            dropdown.CreateControl();
            dropdown.ControlObject.transform.SetSiblingIndex(9);
            dropdown.ValueChanged.Subscribe(Observer.Create<int>(i => UpdateChannelSelection(clothesController.SNo, -1, 1, true, i)));

            buttonHorizLayout = MakeHorizLayoutGO(go.transform);
            buttonHorizLayout.transform.SetSiblingIndex(10);

            OutfitPainterChannelButton syncP1FromChannel = new OutfitPainterChannelButton("Set From Channel", buttonHorizLayout.transform);
            syncP1FromChannel.Initialize();
            syncP1FromChannel.CreateControl();
            syncP1FromChannel.ControlObject.GetComponent<RectTransform>().offsetMin = new Vector2(25, -40);
            syncP1FromChannel.OnClick.AddListener(() => { SyncFromChannel(clothesController.SNo, -1, 1, true, syncP1FromChannel.ControlObject); });

            OutfitPainterChannelButton syncP1ToChannel = new OutfitPainterChannelButton("Sync To Channel", buttonHorizLayout.transform);
            syncP1ToChannel.Initialize();
            syncP1ToChannel.CreateControl();
            syncP1ToChannel.ControlObject.transform.GetChild(0).GetComponent<RectTransform>().offsetMax = new Vector2(225f, 1f);
            syncP1ToChannel.ControlObject.transform.GetChild(0).GetComponent<RectTransform>().offsetMin = new Vector2(50f, 1f);
            syncP1ToChannel.OnClick.AddListener(() => { SyncToChannel(clothesController.SNo, -1, 1, true); });

            dropdowns.Add(dropdown);

            go = GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinClothes/DefaultWin/C_Clothes/Setting/Setting03/Scroll View/Viewport/Content");

            dropdown = new OutfitPainterChannelSelectionDropdown("Color 2 Channel: ", channelNames, go.transform, 0);
            dropdown.Initialize();
            dropdown.CreateControl();
            dropdown.ControlObject.transform.SetSiblingIndex(4);
            dropdown.ValueChanged.Subscribe(Observer.Create<int>(i => UpdateChannelSelection(clothesController.SNo, -1, 2, false, i)));

            buttonHorizLayout = MakeHorizLayoutGO(go.transform);
            buttonHorizLayout.transform.SetSiblingIndex(5);

            OutfitPainterChannelButton syncC2FromChannel = new OutfitPainterChannelButton("Set From Channel", buttonHorizLayout.transform);
            syncC2FromChannel.Initialize();
            syncC2FromChannel.CreateControl();
            syncC2FromChannel.ControlObject.GetComponent<RectTransform>().offsetMin = new Vector2(25, -40);
            syncC2FromChannel.OnClick.AddListener(() => { SyncFromChannel(clothesController.SNo, -1, 2, false, syncC2FromChannel.ControlObject); });

            OutfitPainterChannelButton syncC2ToChannel = new OutfitPainterChannelButton("Sync To Channel", buttonHorizLayout.transform);
            syncC2ToChannel.Initialize();
            syncC2ToChannel.CreateControl();
            syncC2ToChannel.ControlObject.transform.GetChild(0).GetComponent<RectTransform>().offsetMax = new Vector2(225f, 1f);
            syncC2ToChannel.ControlObject.transform.GetChild(0).GetComponent<RectTransform>().offsetMin = new Vector2(50f, 1f);
            syncC2ToChannel.OnClick.AddListener(() => { SyncToChannel(clothesController.SNo, -1, 2, false); });

            dropdowns.Add(dropdown);

            dropdown = new OutfitPainterChannelSelectionDropdown("Pattern 2 Channel: ", channelNames, go.transform, 0);
            dropdown.Initialize();
            dropdown.CreateControl();
            dropdown.ControlObject.transform.SetSiblingIndex(8);
            dropdown.ValueChanged.Subscribe(Observer.Create<int>(i => UpdateChannelSelection(clothesController.SNo, -1, 2, true, i)));

            buttonHorizLayout = MakeHorizLayoutGO(go.transform);
            buttonHorizLayout.transform.SetSiblingIndex(10);

            OutfitPainterChannelButton syncP2FromChannel = new OutfitPainterChannelButton("Set From Channel", buttonHorizLayout.transform);
            syncP2FromChannel.Initialize();
            syncP2FromChannel.CreateControl();
            syncP2FromChannel.ControlObject.GetComponent<RectTransform>().offsetMin = new Vector2(25, -40);
            syncP2FromChannel.OnClick.AddListener(() => { SyncFromChannel(clothesController.SNo, -1, 2, true, syncP2FromChannel.ControlObject); });

            OutfitPainterChannelButton syncP2ToChannel = new OutfitPainterChannelButton("Sync To Channel", buttonHorizLayout.transform);
            syncP2ToChannel.Initialize();
            syncP2ToChannel.CreateControl();
            syncP2ToChannel.ControlObject.transform.GetChild(0).GetComponent<RectTransform>().offsetMax = new Vector2(225f, 1f);
            syncP2ToChannel.ControlObject.transform.GetChild(0).GetComponent<RectTransform>().offsetMin = new Vector2(50f, 1f);
            syncP2ToChannel.OnClick.AddListener(() => { SyncToChannel(clothesController.SNo, -1, 2, true); });


            dropdowns.Add(dropdown);


            go = GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinClothes/DefaultWin/C_Clothes/Setting/Setting04/Scroll View/Viewport/Content");

            dropdown = new OutfitPainterChannelSelectionDropdown("Color 3 Channel: ", channelNames, go.transform, 0);
            dropdown.Initialize();
            dropdown.CreateControl();
            dropdown.ControlObject.transform.SetSiblingIndex(4);
            dropdown.ValueChanged.Subscribe(Observer.Create<int>(i => UpdateChannelSelection(clothesController.SNo, -1, 3, false, i)));

            buttonHorizLayout = MakeHorizLayoutGO(go.transform);
            buttonHorizLayout.transform.SetSiblingIndex(5);

            OutfitPainterChannelButton syncC3FromChannel = new OutfitPainterChannelButton("Set From Channel", buttonHorizLayout.transform);
            syncC3FromChannel.Initialize();
            syncC3FromChannel.CreateControl();
            syncC3FromChannel.ControlObject.GetComponent<RectTransform>().offsetMin = new Vector2(25, -40);
            syncC3FromChannel.OnClick.AddListener(() => { SyncFromChannel(clothesController.SNo, -1, 3, false, syncC3FromChannel.ControlObject); });

            OutfitPainterChannelButton syncC3ToChannel = new OutfitPainterChannelButton("Sync To Channel", buttonHorizLayout.transform);
            syncC3ToChannel.Initialize();
            syncC3ToChannel.CreateControl();
            syncC3ToChannel.ControlObject.transform.GetChild(0).GetComponent<RectTransform>().offsetMax = new Vector2(225f, 1f);
            syncC3ToChannel.ControlObject.transform.GetChild(0).GetComponent<RectTransform>().offsetMin = new Vector2(50f, 1f);
            syncC3ToChannel.OnClick.AddListener(() => { SyncToChannel(clothesController.SNo, -1, 3, false); });

            dropdowns.Add(dropdown);

            dropdown = new OutfitPainterChannelSelectionDropdown("Pattern 3 Channel: ", channelNames, go.transform, 0);
            dropdown.Initialize();
            dropdown.CreateControl();
            dropdown.ControlObject.transform.SetSiblingIndex(8);
            dropdown.ValueChanged.Subscribe(Observer.Create<int>(i => UpdateChannelSelection(clothesController.SNo, -1, 3, true, i)));

            buttonHorizLayout = MakeHorizLayoutGO(go.transform);
            buttonHorizLayout.transform.SetSiblingIndex(10);

            OutfitPainterChannelButton syncP3FromChannel = new OutfitPainterChannelButton("Set From Channel", buttonHorizLayout.transform);
            syncP3FromChannel.Initialize();
            syncP3FromChannel.CreateControl();
            syncP3FromChannel.ControlObject.GetComponent<RectTransform>().offsetMin = new Vector2(25, -40);
            syncP3FromChannel.OnClick.AddListener(() => { SyncFromChannel(clothesController.SNo, -1, 3, true, syncP3FromChannel.ControlObject); });

            OutfitPainterChannelButton syncP3ToChannel = new OutfitPainterChannelButton("Sync To Channel", buttonHorizLayout.transform);
            syncP3ToChannel.Initialize();
            syncP3ToChannel.CreateControl();
            syncP3ToChannel.ControlObject.transform.GetChild(0).GetComponent<RectTransform>().offsetMax = new Vector2(225f, 1f);
            syncP3ToChannel.ControlObject.transform.GetChild(0).GetComponent<RectTransform>().offsetMin = new Vector2(50f, 1f);
            syncP3ToChannel.OnClick.AddListener(() => { SyncToChannel(clothesController.SNo, -1, 3, true); });


            dropdowns.Add(dropdown);

            go = GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinAccessory/A_Slot/Setting/Setting02/Scroll View/Viewport/Content");
            GameObject colorGroup = go.transform.GetChild(0).gameObject;

            dropdown = new OutfitPainterChannelSelectionDropdown("Color 1 Channel: ", channelNames, colorGroup.transform, 0);
            dropdown.Initialize();
            dropdown.CreateControl();
            dropdown.ControlObject.transform.SetSiblingIndex(4);
            dropdown.ValueChanged.Subscribe(Observer.Create<int>(i => UpdateChannelSelection(-1, SelectedAccessorySlot, 1, false, i)));

            buttonHorizLayout = MakeHorizLayoutGO(colorGroup.transform);
            buttonHorizLayout.transform.SetSiblingIndex(5);

            OutfitPainterChannelButton syncA1FromChannel = new OutfitPainterChannelButton("Set From Channel", buttonHorizLayout.transform);
            syncA1FromChannel.Initialize();
            syncA1FromChannel.CreateControl();
            syncA1FromChannel.ControlObject.GetComponent<RectTransform>().offsetMin = new Vector2(25, -40);
            syncA1FromChannel.OnClick.AddListener(() => { SyncFromChannel(-1, SelectedAccessorySlot, 1, false, syncA1FromChannel.ControlObject); });

            OutfitPainterChannelButton syncA1ToChannel = new OutfitPainterChannelButton("Sync To Channel", buttonHorizLayout.transform);
            syncA1ToChannel.Initialize();
            syncA1ToChannel.CreateControl();
            syncA1ToChannel.ControlObject.transform.GetChild(0).GetComponent<RectTransform>().offsetMax = new Vector2(225f, 1f);
            syncA1ToChannel.ControlObject.transform.GetChild(0).GetComponent<RectTransform>().offsetMin = new Vector2(50f, 1f);
            syncA1ToChannel.OnClick.AddListener(() => { SyncToChannel(-1, SelectedAccessorySlot, 1, false); });

            dropdowns.Add(dropdown);

            colorGroup = go.transform.GetChild(1).gameObject;

            dropdown = new OutfitPainterChannelSelectionDropdown("Color 2 Channel: ", channelNames, colorGroup.transform, 0);
            dropdown.Initialize();
            dropdown.CreateControl();
            dropdown.ControlObject.transform.SetSiblingIndex(4);
            dropdown.ValueChanged.Subscribe(Observer.Create<int>(i => UpdateChannelSelection(-1, SelectedAccessorySlot, 2, false, i)));

            buttonHorizLayout = MakeHorizLayoutGO(colorGroup.transform);
            buttonHorizLayout.transform.SetSiblingIndex(5);

            OutfitPainterChannelButton syncA2FromChannel = new OutfitPainterChannelButton("Set From Channel", buttonHorizLayout.transform);
            syncA2FromChannel.Initialize();
            syncA2FromChannel.CreateControl();
            syncA2FromChannel.ControlObject.GetComponent<RectTransform>().offsetMin = new Vector2(25, -40);
            syncA2FromChannel.OnClick.AddListener(() => { SyncFromChannel(-1, SelectedAccessorySlot, 2, false, syncA2FromChannel.ControlObject); });

            OutfitPainterChannelButton syncA2ToChannel = new OutfitPainterChannelButton("Sync To Channel", buttonHorizLayout.transform);
            syncA2ToChannel.Initialize();
            syncA2ToChannel.CreateControl();
            syncA2ToChannel.ControlObject.transform.GetChild(0).GetComponent<RectTransform>().offsetMax = new Vector2(225f, 1f);
            syncA2ToChannel.ControlObject.transform.GetChild(0).GetComponent<RectTransform>().offsetMin = new Vector2(50f, 1f);
            syncA2ToChannel.OnClick.AddListener(() => { SyncToChannel(-1, SelectedAccessorySlot, 2, false); });

            dropdowns.Add(dropdown);

            colorGroup = go.transform.GetChild(2).gameObject;

            dropdown = new OutfitPainterChannelSelectionDropdown("Color 3 Channel: ", channelNames, colorGroup.transform, 0);
            dropdown.Initialize();
            dropdown.CreateControl();
            dropdown.ControlObject.transform.SetSiblingIndex(4);
            dropdown.ValueChanged.Subscribe(Observer.Create<int>(i => UpdateChannelSelection(-1, SelectedAccessorySlot, 3, false, i)));

            buttonHorizLayout = MakeHorizLayoutGO(colorGroup.transform);
            buttonHorizLayout.transform.SetSiblingIndex(5);

            OutfitPainterChannelButton syncA3FromChannel = new OutfitPainterChannelButton("Set From Channel", buttonHorizLayout.transform);
            syncA3FromChannel.Initialize();
            syncA3FromChannel.CreateControl();
            syncA3FromChannel.ControlObject.GetComponent<RectTransform>().offsetMin = new Vector2(25, -40);
            syncA3FromChannel.OnClick.AddListener(() => { SyncFromChannel(-1, SelectedAccessorySlot, 3, false, syncA3FromChannel.ControlObject); });

            OutfitPainterChannelButton syncA3ToChannel = new OutfitPainterChannelButton("Sync To Channel", buttonHorizLayout.transform);
            syncA3ToChannel.Initialize();
            syncA3ToChannel.CreateControl();
            syncA3ToChannel.ControlObject.transform.GetChild(0).GetComponent<RectTransform>().offsetMax = new Vector2(225f, 1f);
            syncA3ToChannel.ControlObject.transform.GetChild(0).GetComponent<RectTransform>().offsetMin = new Vector2(50f, 1f);
            syncA3ToChannel.OnClick.AddListener(() => { SyncToChannel(-1, SelectedAccessorySlot, 3, false); });

            dropdowns.Add(dropdown);

            colorGroup = go.transform.GetChild(3).gameObject;

            dropdown = new OutfitPainterChannelSelectionDropdown("Color 3 Channel: ", channelNames, colorGroup.transform, 0);
            dropdown.Initialize();
            dropdown.CreateControl();
            dropdown.ControlObject.transform.SetSiblingIndex(4);
            dropdown.ValueChanged.Subscribe(Observer.Create<int>(i => UpdateChannelSelection(-1, SelectedAccessorySlot, 4, false, i)));

            buttonHorizLayout = MakeHorizLayoutGO(colorGroup.transform);
            buttonHorizLayout.transform.SetSiblingIndex(5);

            OutfitPainterChannelButton syncA4FromChannel = new OutfitPainterChannelButton("Set From Channel", buttonHorizLayout.transform);
            syncA4FromChannel.Initialize();
            syncA4FromChannel.CreateControl();
            syncA4FromChannel.ControlObject.GetComponent<RectTransform>().offsetMin = new Vector2(25, -40);
            syncA4FromChannel.OnClick.AddListener(() => { SyncFromChannel(-1, SelectedAccessorySlot, 4, false, syncA4FromChannel.ControlObject); });

            OutfitPainterChannelButton syncA4ToChannel = new OutfitPainterChannelButton("Sync To Channel", buttonHorizLayout.transform);
            syncA4ToChannel.Initialize();
            syncA4ToChannel.CreateControl();
            syncA4ToChannel.ControlObject.transform.GetChild(0).GetComponent<RectTransform>().offsetMax = new Vector2(225f, 1f);
            syncA4ToChannel.ControlObject.transform.GetChild(0).GetComponent<RectTransform>().offsetMin = new Vector2(50f, 1f);
            syncA4ToChannel.OnClick.AddListener(() => { SyncToChannel(-1, SelectedAccessorySlot, 4, false); });

            dropdowns.Add(dropdown);
            UpdatingGUI = false;

        }

        private static GameObject MakeHorizLayoutGO(Transform parent)
        {
            GameObject horizLayoutGO = UnityEngine.Object.Instantiate(GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinClothes/DefaultWin/C_Clothes/SelectMenu"), parent, false);
            GameObject.DestroyImmediate(horizLayoutGO.GetComponent<ToggleGroup>());
            for (int i = horizLayoutGO.transform.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(horizLayoutGO.transform.GetChild(i).gameObject);
            }
            horizLayoutGO.name = "OutfitPainter Sync Button Layout";
            return horizLayoutGO;
        }

        private static void SyncFromChannel(int clothesKind, int slotNumber, int colorNumber, bool pattern, GameObject sourceButton)
        {
            ChaControl character = MakerAPI.GetCharacterControl();
            OutfitPainterCharacterController controller = character.gameObject.GetComponent<OutfitPainterCharacterController>();
            GameObject content = sourceButton.transform.parent.parent.gameObject;
            if (!pattern)
            {
                GameObject glossSlider = null;
                GameObject metallicSlider = null;
                foreach (Transform t in content.transform)
                {
                    if (t.name == "SliderSet" && glossSlider == null)
                        glossSlider = t.gameObject;
                    else if (t.name == "SliderSet")
                        metallicSlider = t.gameObject;
                }
                controller.Data.SyncSlotColor(character, OutfitPainterData.SlotForClothesKind(clothesKind), slotNumber, colorNumber, pattern, content.transform.GetComponentInChildren<CustomColorSet>(true), glossSlider.GetComponent<CustomSliderSet>(), metallicSlider.GetComponent<CustomSliderSet>());
            }
            else
            {
                GameObject patternSetting = sourceButton.transform.parent.parent.Find("PatternSetting").gameObject;
                GameObject glossSlider = null;
                GameObject metallicSlider = null;
                foreach (Transform t in content.transform)
                {
                    if (t.name == "SliderSet" && glossSlider == null)
                        glossSlider = t.gameObject;
                    else if (t.name == "SliderSet")
                        metallicSlider = t.gameObject;
                }
                controller.Data.SyncSlotColor(character, OutfitPainterData.SlotForClothesKind(clothesKind), slotNumber, colorNumber, pattern, patternSetting.GetComponentInChildren<CustomColorSet>(true), glossSlider.GetComponent<CustomSliderSet>(), metallicSlider.GetComponent<CustomSliderSet>());
            }
        }

        private static void SyncToChannel(int clothesKind, int slotNumber, int colorNumber, bool pattern)
        {
            ChaControl character = MakerAPI.GetCharacterControl();
            OutfitPainterCharacterController controller = character.gameObject.GetComponent<OutfitPainterCharacterController>();
            controller.Data.SetFromSlot(character, OutfitPainterData.SlotForClothesKind(clothesKind), slotNumber, colorNumber, pattern);
            UpdateOutfitPainterMakerGUI();
        }

        private static void UpdateChannelSelection(int clothesKind, int slotNumber, int colorNumber, bool pattern, int selectedChannel)
        {
            ChaControl character = MakerAPI.GetCharacterControl();
            OutfitPainterCharacterController controller = character.gameObject.GetComponent<OutfitPainterCharacterController>();

            if (selectedChannel == 0)
            {
                controller.Data.RemoveAssignment(OutfitPainterData.SlotForClothesKind(clothesKind), slotNumber, colorNumber, pattern);
            }
            else
            {
                controller.Data.SetAssignment(OutfitPainterData.SlotForClothesKind(clothesKind), slotNumber, colorNumber, pattern, selectedChannel);
            }

            // Update visible maker painter controls
            UpdateOutfitPainterMakerGUI(controller.Data);

        }        
    }
}
