using AIChara;
using CharaCustom;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OutfitPainter
{
    [Serializable]
    [MessagePackObject]
    public class OutfitPainterData
    {
        [Key(0)]
        public List<OutfitPainterChannel> Channels { get; set; }

        [IgnoreMember]
        public List<OutfitPainterChannel> ActiveChannels
        {
            get
            {
                return Channels.Where(c => c.Assignments.Count > 0).ToList();
            }
        }

        public OutfitPainterData()
        {
            Channels = new List<OutfitPainterChannel>();
            Channels.Clear();

            for (int i = 1; i <= 16; i++)
            {
                Channels.Add(new OutfitPainterChannel(i, Color.black, 0, 0));
            }            
        }

        public void SetDefault()
        {

            FindChannelById(1).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.TOP, -1, 1, false));
            FindChannelById(2).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.TOP, -1, 2, false));
            FindChannelById(3).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.TOP, -1, 3, false));

            FindChannelById(1).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.BOT, -1, 1, false));
            FindChannelById(2).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.BOT, -1, 2, false));
            FindChannelById(3).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.BOT, -1, 3, false));

            FindChannelById(1).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.INNER_TOP, -1, 1, false));
            FindChannelById(2).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.INNER_TOP, -1, 2, false));
            FindChannelById(3).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.INNER_TOP, -1, 3, false));

            FindChannelById(1).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.INNER_BOT, -1, 1, false));
            FindChannelById(2).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.INNER_BOT, -1, 2, false));
            FindChannelById(3).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.INNER_BOT, -1, 3, false));

            FindChannelById(1).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.GLOVE, -1, 1, false));
            FindChannelById(2).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.GLOVE, -1, 2, false));
            FindChannelById(3).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.GLOVE, -1, 3, false));

            FindChannelById(1).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.PANTYHOSE, -1, 1, false));
            FindChannelById(2).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.PANTYHOSE, -1, 2, false));
            FindChannelById(3).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.PANTYHOSE, -1, 3, false));

            FindChannelById(1).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.SOCK, -1, 1, false));
            FindChannelById(2).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.SOCK, -1, 2, false));
            FindChannelById(3).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.SOCK, -1, 3, false));

            FindChannelById(1).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.SHOE, -1, 1, false));
            FindChannelById(2).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.SHOE, -1, 2, false));
            FindChannelById(3).Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.SHOE, -1, 3, false));
        }

        public List<string> FindChannelDescriptions()
        {
            return Channels.Select(c => c.ChannelDescription).OrderBy(s => s).ToList();
        }

        public List<string> FindChannelDescriptionsUnsorted()
        {
            return Channels.Select(c => c.ChannelDescription).ToList();
        }

        public List<string> FindActiveChannelDescriptions()
        {
            return Channels.Where(c => c.Assignments.Count > 0).Select(c => c.ChannelDescription).OrderBy(s => s).ToList();
        }

        public OutfitPainterChannel FindChannelById(int id)
        {
            return Channels.First(c => c.ChannelId == id);
        }

        public OutfitPainterChannel FindChannelForAssignment(OutfitPainterSlot slot, int slotNumber, int colorNumber, bool pattern)
        {
            return Channels.Find(c => c.Assignments.Find(a => a.slot == slot && a.slotNumber == slotNumber && a.colorNumber == colorNumber && a.patternColor == pattern) != null);
        }

        public int FindChannelIdForAssignment(OutfitPainterSlot slot, int slotNumber, int colorNumber, bool pattern)
        {
            OutfitPainterChannel channel = Channels.Find(c => c.Assignments.Find(a => a.slot == slot && a.slotNumber == slotNumber && a.colorNumber == colorNumber && a.patternColor == pattern) != null);
            return channel == null ? 0 : channel.ChannelId;
        }

        public List<OutfitPainterChannelAssignment> FindSlotsForChannel(int id)
        {
            return Channels.FindAll(c => c.ChannelId == id).Select(c => c.Assignments).SelectMany(a => a).ToList();
        }

        public void CopyAccessorySlot(int oldNumber, int newNumber)
        {
            Channels.ForEach(c => c.Assignments.FindAll(a => a.slot == OutfitPainterSlot.ACCESSORY && a.slotNumber == oldNumber).ForEach(a => c.Assignments.Add(new OutfitPainterChannelAssignment(OutfitPainterSlot.ACCESSORY, newNumber, a.colorNumber, a.patternColor))));
        }

        public void ClearAccessorySlot(int slotNumber)
        {
            Channels.ForEach(c => c.Assignments.RemoveAll(a => a.slot == OutfitPainterSlot.ACCESSORY && a.slotNumber == slotNumber));
        }

        public void ClearSlot(OutfitPainterSlot slot)
        {
            Channels.ForEach(c => c.Assignments.RemoveAll(a => a.slot == slot));
        }

        public void ChangeAccessorySlot(int oldNumber, int newNumber)
        {
            Channels.ForEach(c => c.Assignments.FindAll(a => a.slot == OutfitPainterSlot.ACCESSORY && a.slotNumber == oldNumber).ForEach(a => a.slotNumber = newNumber));
        }

        public void SyncSlotColor(ChaControl character, OutfitPainterSlot slot, int slotNumber, int colorNumber, bool pattern, CustomColorSet customColorSet, CustomSliderSet gloss, CustomSliderSet metallic)
        {
            OutfitPainterChannel channel = FindChannelForAssignment(slot, slotNumber, colorNumber, pattern);
            if (channel != null)
            {
                OutfitPainterChannelAssignment assignment = channel.FindAssignmentForSlot(slot, slotNumber, colorNumber, pattern);
                channel.UpdateAssignment(character, assignment);
                customColorSet?.SetColor(channel.ChannelColor);
                gloss?.SetSliderValue(channel.ChannelGloss);
                metallic?.SetSliderValue(channel.ChannelMetallic);
            }
        }

        public void SetFromSlot(ChaControl character, OutfitPainterSlot slot, int slotNumber, int colorNumber, bool pattern)
        {
            OutfitPainterChannel channel = FindChannelForAssignment(slot, slotNumber, colorNumber, pattern);
            if (channel != null)
            {
                OutfitPainterChannelAssignment assignment = channel.FindAssignmentForSlot(slot, slotNumber, colorNumber, pattern);
                channel.SetFromAssignment(character, assignment);
            }
        }

        public void SetAssignment(OutfitPainterSlot slot, int slotNumber, int colorNumber, bool pattern, int channelId)
        {
            if (slot != OutfitPainterSlot.ACCESSORY)
                slotNumber = -1;
           
            OutfitPainterChannel channel = FindChannelById(channelId);
            OutfitPainterChannelAssignment assignment = channel.Assignments.Find(a => a.slot == slot && a.slotNumber == slotNumber && a.colorNumber == colorNumber && a.patternColor == pattern);
            if (assignment == null)
                channel.Assignments.Add(new OutfitPainterChannelAssignment(slot, slotNumber, colorNumber, pattern));            

            foreach (OutfitPainterChannel removeChannel in Channels)
            {
                if (removeChannel.ChannelId == channelId)
                    continue;

                OutfitPainterChannelAssignment removeAssignment = removeChannel.Assignments.Find(a => a.slot == slot && a.slotNumber == slotNumber && a.colorNumber == colorNumber && a.patternColor == pattern);
                if (removeAssignment != null)
                {
                    removeChannel.Assignments.Remove(removeAssignment);
                }
            }

        }

        public void RemoveAssignment(OutfitPainterSlot slot, int slotNumber, int colorNumber, bool pattern)
        {
            if (slot != OutfitPainterSlot.ACCESSORY)
                slotNumber = -1;


            foreach (OutfitPainterChannel channel in Channels)
            {
                OutfitPainterChannelAssignment assignment = channel.Assignments.Find(a => a.slot == slot && a.slotNumber == slotNumber && a.colorNumber == colorNumber && a.patternColor == pattern);
                if (assignment != null)
                {
                    channel.Assignments.Remove(assignment);
                }
            }
        }

        public static OutfitPainterSlot SlotForClothesKind(int clothesKind)
        {
            switch (clothesKind)
            {
                case 0:
                    return OutfitPainterSlot.TOP;
                case 1:
                    return OutfitPainterSlot.BOT;
                case 2:
                    return OutfitPainterSlot.INNER_TOP;
                case 3:
                    return OutfitPainterSlot.INNER_BOT;
                case 4:
                    return OutfitPainterSlot.GLOVE;
                case 5:
                    return OutfitPainterSlot.PANTYHOSE;
                case 6:
                    return OutfitPainterSlot.SOCK;
                case 7:
                    return OutfitPainterSlot.SHOE;
            }
            return OutfitPainterSlot.ACCESSORY;

        }
    }
}
