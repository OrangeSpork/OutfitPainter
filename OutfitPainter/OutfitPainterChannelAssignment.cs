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
                    return $"Acc {slotNumber + 1} {(patternColor ? "Pattern" : "Color")} {(colorNumber == 4 ? colorNumber - 1 : colorNumber)}";
                else
                    return $"{new string(slotName)} {(patternColor ? "Pattern" : "Color")} {colorNumber}";
            }

        }

        public static Comparer<OutfitPainterChannelAssignment> CreateComparer()
        {
            return Comparer<OutfitPainterChannelAssignment>.Create((a1, a2) => {
                if (a1.slot == a2.slot)
                {
                    if (a1.slotNumber == a2.slotNumber)
                    {
                        if (a1.patternColor == a2.patternColor)
                        {
                            // By Color Number
                            return a1.colorNumber.CompareTo(a2.colorNumber);
                        }
                        else
                        {
                            // Normal Before Pattern
                            return a1.patternColor.CompareTo(a2.patternColor);
                        }
                    }
                    else
                    {
                        // By Slot Number
                        return a1.slotNumber.CompareTo(a2.slotNumber);
                    }
                }
                else if (a1.slot != OutfitPainterSlot.ACCESSORY && a2.slot != OutfitPainterSlot.ACCESSORY)
                {
                    // by slot
                    return a1.slot.CompareTo(a2.slot);
                }
                else if (a1.slot == OutfitPainterSlot.ACCESSORY)
                {
                    return 1;
                }
                else if (a2.slot == OutfitPainterSlot.ACCESSORY)
                {
                    return -1;
                }
                else
                {
                    return a1.slot.CompareTo(a2.slot);
                }
            });
        }
    }

    
}
