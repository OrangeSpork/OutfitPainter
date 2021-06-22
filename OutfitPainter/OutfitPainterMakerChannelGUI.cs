using AIChara;
using BepInEx;
using CharaCustom;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniRx;
using UnityEngine;

namespace OutfitPainter
{
    public class OutfitPainterMakerChannelGUI
    {

        public OutfitPainterChannel Channel { get; set; }
        public MakerTextbox Name { get; set; }
        public MakerColor Color { get; set; }
        public MakerSlider Gloss { get; set; }
        public MakerSlider Metallic { get; set; }
        public MakerSeparator Separator { get; set; }
        public MakerText Description { get; set; }
        public MakerButton SyncNow { get; set; }

        public void Initialize(OutfitPainterChannel channel, MakerCategory makerCategory, BaseUnityPlugin owner, OutfitPainterData data)
        {
            Channel = channel;
            Name = new MakerTextbox(makerCategory, "Channel", channel.ChannelDescription, owner);
            Name.ValueChanged.Subscribe(Observer.Create<string>((s) => {
                if (s != Channel.ChannelDescription)
                {
                    Channel.ChannelDescription = s;
                    OutfitPainterMakerGUI.UpdateOutfitPainterMakerGUI();
                }
            }));
            Color = new MakerColor("Color", true, makerCategory, channel.ChannelColor, owner);
            Color.ValueChanged.Subscribe(Observer.Create<Color>(c => UpdateMakerColors(c, Channel.ChannelMetallic, Channel.ChannelGloss)));
            Gloss = new MakerSlider(makerCategory, "Shine", -2, 2, 0, owner);
            Gloss.ValueChanged.Subscribe(Observer.Create<float>(f => UpdateMakerColors(Channel.ChannelColor, Channel.ChannelMetallic, f)));
            Metallic = new MakerSlider(makerCategory, "Texture", -2, 2, 0, owner);
            Metallic.ValueChanged.Subscribe(Observer.Create<float>(f => UpdateMakerColors(Channel.ChannelColor, f, Channel.ChannelGloss)));
            Separator = new MakerSeparator(makerCategory, owner);
            SyncNow = new MakerButton("Sync All Now", makerCategory, owner);
            SyncNow.OnClick.AddListener(() => {
                Channel.Update(MakerAPI.GetCharacterControl());
            });
            Description = new MakerText("Used By: " + String.Join(", ", data.FindSlotsForChannel(Channel.ChannelId).OrderBy(a => a, OutfitPainterChannelAssignment.CreateComparer()).Select(a => a.Description).ToList()), makerCategory, owner);

            MakerAPI.AddControl(Name);
            MakerAPI.AddControl(Color);
            MakerAPI.AddControl(Gloss);
            MakerAPI.AddControl(Metallic);
            MakerAPI.AddControl(SyncNow);
            MakerAPI.AddControl(Description);
            MakerAPI.AddControl(Separator);            
        }

        public void Update(OutfitPainterChannel channel, OutfitPainterData data)
        {
            Channel = channel;
            Name.Value = channel.ChannelDescription;
            Color.Value = channel.ChannelColor;
            Color.ControlObject?.GetComponent<CustomColorSet>().SetColor(channel.ChannelColor);
            Gloss.Value = channel.ChannelGloss;
            Metallic.Value = channel.ChannelMetallic;
            Description.Text = "Used By: " + String.Join(", ", data.FindSlotsForChannel(channel.ChannelId).OrderBy(a => a, OutfitPainterChannelAssignment.CreateComparer()).Select(a => a.Description).ToList());

            bool visible = channel.Assignments.Count > 0;

            Name.Visible.OnNext(visible);
            Color.Visible.OnNext(visible);
            Gloss.Visible.OnNext(visible);
            Metallic.Visible.OnNext(visible);
            Description.Visible.OnNext(visible);
            SyncNow.Visible.OnNext(visible);
            Separator.Visible.OnNext(visible);

        }

        private void UpdateMakerColors(Color color, float metallic, float gloss)
        {
            ChaControl character = MakerAPI.GetCharacterControl();

            Channel.ChannelColor = color;
            Channel.ChannelMetallic = metallic;
            Channel.ChannelGloss = gloss;
            Channel.Update(character);
        }

    }
}
