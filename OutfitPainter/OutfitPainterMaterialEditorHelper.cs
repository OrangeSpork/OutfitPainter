using AIChara;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace OutfitPainter
{
    public class OutfitPainterMaterialEditorHelper
    {
        private static Type MaterialEditorCharaControllerType = AccessTools.TypeByName("KK_Plugins.MaterialEditor.MaterialEditorCharaController");
        private static PropertyInfo CustomClothesOverrideProperty = AccessTools.Property(MaterialEditorCharaControllerType, "CustomClothesOverride");

        public static void SetMaterialEditorCustomClothesFlag(ChaControl chaControl)
        {
            if (chaControl == null)
                return;

            var controller = chaControl.gameObject.GetComponent(MaterialEditorCharaControllerType);
            CustomClothesOverrideProperty.SetValue(controller, true);
        }
    }
}
