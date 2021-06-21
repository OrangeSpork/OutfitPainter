using AIChara;
using KKAPI.Studio;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OutfitPainter
{
    [Serializable]
    [MessagePackObject]
    public class OutfitPainterChannel
    {
        [Key(0)]
        public int ChannelId { get; set; }

        [Key(1)]
        public string ChannelDescription { get; set; }

        [Key(2)]
        public Color ChannelColor { get; set; }

        [Key(3)]
        public float ChannelGloss { get; set; }

        [Key(4)]
        public float ChannelMetallic { get; set; }

        [Key(5)]
        public List<OutfitPainterChannelAssignment> Assignments { get; set; } = new List<OutfitPainterChannelAssignment>();

        [IgnoreMember]
        public Color ChannelDisplayColor
        {
            get
            {
                return new Color(ChannelColor.r, ChannelColor.g, ChannelColor.b, 1.0f);
            }
        }


        [IgnoreMember]
        public Color InverseChannelColor
        {
            get
            {
                float r = 1.0f - ChannelColor.r;
                float g = 1.0f - ChannelColor.g;
                float b = 1.0f - ChannelColor.b;

                if ((r + g + b) / 3.0f > .5f)
                    return Color.Lerp(Color.white, new Color(r, g, b, 1.0f), .5f);
                else
                    return Color.Lerp(Color.black, new Color(r, g, b, 1.0f), .5f);
            }
        }

        public OutfitPainterChannel()
        {

        }

        public OutfitPainterChannel(int id, Color color, float gloss, float metallic, string description = null)
        {
            ChannelId = id;
            ChannelColor = color;
            ChannelGloss = gloss;
            ChannelMetallic = metallic;
            ChannelDescription = description == null ? id.ToString() : description;
        }

        public OutfitPainterChannelAssignment FindAssignmentForSlot(OutfitPainterSlot slot, int slotNumber, int colorNumber, bool pattern)
        {
            return Assignments.Find(a => a.slot == slot && a.slotNumber == slotNumber && a.colorNumber == colorNumber && a.patternColor == pattern);
        }

        public void UpdateAssignment(ChaControl character, OutfitPainterChannelAssignment assignment)
        {
            if (assignment.slot != OutfitPainterSlot.ACCESSORY)
            {
                character.nowCoordinate.clothes.parts[(int)assignment.slot].colorInfo[assignment.colorNumber - 1].metallicPower = ChannelMetallic;
                character.chaFile.coordinate.clothes.parts[(int)assignment.slot].colorInfo[assignment.colorNumber - 1].metallicPower = ChannelMetallic; ;

                character.nowCoordinate.clothes.parts[(int)assignment.slot].colorInfo[assignment.colorNumber - 1].glossPower = ChannelGloss;
                character.chaFile.coordinate.clothes.parts[(int)assignment.slot].colorInfo[assignment.colorNumber - 1].glossPower = ChannelGloss;


                if (assignment.patternColor)
                {
                    character.nowCoordinate.clothes.parts[(int)assignment.slot].colorInfo[assignment.colorNumber - 1].patternColor = ChannelColor;
                    character.chaFile.coordinate.clothes.parts[(int)assignment.slot].colorInfo[assignment.colorNumber - 1].patternColor = ChannelColor;
                }
                else
                {
                    character.nowCoordinate.clothes.parts[(int)assignment.slot].colorInfo[assignment.colorNumber - 1].baseColor = ChannelColor;
                    character.chaFile.coordinate.clothes.parts[(int)assignment.slot].colorInfo[assignment.colorNumber - 1].baseColor = ChannelColor;
                }                
                character.ChangeCustomClothes(kind: ((int)assignment.slot), updateColor: true, updateTex01: true, updateTex02: true, updateTex03: true);               
            }
            else
            {
                character.nowCoordinate.accessory.parts[assignment.slotNumber].colorInfo[assignment.colorNumber - 1].metallicPower = ChannelMetallic;
                character.chaFile.coordinate.accessory.parts[assignment.slotNumber].colorInfo[assignment.colorNumber - 1].metallicPower = ChannelMetallic;

                character.nowCoordinate.accessory.parts[assignment.slotNumber].colorInfo[assignment.colorNumber - 1].glossPower = ChannelGloss;
                character.chaFile.coordinate.accessory.parts[assignment.slotNumber].colorInfo[assignment.colorNumber - 1].glossPower = ChannelGloss;

                character.nowCoordinate.accessory.parts[assignment.slotNumber].colorInfo[assignment.colorNumber - 1].color = ChannelColor;
                character.chaFile.coordinate.accessory.parts[assignment.slotNumber].colorInfo[assignment.colorNumber - 1].color = ChannelColor;

                character.ChangeAccessoryColor(assignment.slotNumber);
            }
        }

        public void SetFromAssignment(ChaControl character, OutfitPainterChannelAssignment assignment)
        {
            if (assignment.slot != OutfitPainterSlot.ACCESSORY)
            {
                ChannelMetallic = character.nowCoordinate.clothes.parts[(int)assignment.slot].colorInfo[assignment.colorNumber - 1].metallicPower;
                ChannelGloss= character.nowCoordinate.clothes.parts[(int)assignment.slot].colorInfo[assignment.colorNumber - 1].glossPower;
                if (assignment.patternColor)
                    ChannelColor = character.nowCoordinate.clothes.parts[(int)assignment.slot].colorInfo[assignment.colorNumber - 1].patternColor;
                else
                    ChannelColor = character.nowCoordinate.clothes.parts[(int)assignment.slot].colorInfo[assignment.colorNumber - 1].baseColor;
            }
            else
            {
                ChannelMetallic = character.nowCoordinate.accessory.parts[assignment.slotNumber].colorInfo[assignment.colorNumber - 1].metallicPower;
                ChannelGloss = character.nowCoordinate.accessory.parts[assignment.slotNumber].colorInfo[assignment.colorNumber - 1].glossPower;
                ChannelColor = character.nowCoordinate.accessory.parts[assignment.slotNumber].colorInfo[assignment.colorNumber - 1].color;
            }

            Update(character);
        }

        public void Update(ChaControl character)
        {
            if (OutfitPainterMakerGUI.UpdatingGUI)
                return;

            foreach (OutfitPainterChannelAssignment assignment in Assignments)
            {
                UpdateAssignment(character, assignment);
            }
        }
    }
}
