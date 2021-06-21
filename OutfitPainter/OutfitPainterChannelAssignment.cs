using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace OutfitPainter
{
    [Serializable]
    [MessagePackObject]
    public class OutfitPainterChannelAssignment
    {
        [Key(0)]
        public OutfitPainterSlot slot { get; set; }

        [Key(1)]
        public int slotNumber { get; set; }

        [Key(2)]
        public int colorNumber { get; set; }

        [Key(3)]
        public bool patternColor { get; set; }

        public OutfitPainterChannelAssignment()
        {

        }

        public OutfitPainterChannelAssignment(OutfitPainterSlot slot, int slotNumber, int colorNumber, bool patternColor)
        {
            Update(slot, slotNumber, colorNumber, patternColor);
        }

        public void Update(OutfitPainterSlot slot, int slotNumber, int colorNumber, bool patternColor)
        {
            this.slot = slot;
            this.slotNumber = slotNumber;
            this.colorNumber = colorNumber;
            this.patternColor = patternColor;
        }

        [IgnoreMember]
        public string Description 
        {
            get {
                char[] slotName = slot.ToString().ToLower().ToCharArray();
                slotName[0] = char.ToUpper(slotName[0]);
                if (slot == OutfitPainterSlot.ACCESSORY)
                    return $"Acc {slotNumber} {(patternColor ? "Pattern" : "Color")} {(colorNumber == 4 ? colorNumber - 1 : colorNumber)}";
                else
                    return $"{new string(slotName)} {(patternColor ? "Pattern" : "Color")} {colorNumber}";
            }
        }
    }
}
