using BepInEx;
using KKAPI.Maker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace OutfitPainter
{

    // Shamelessly ripping off the KKAPI button code which, sadly, does not allow me to control where it goes...
    public class OutfitPainterChannelButton : IDisposable
    {
        private static Transform _buttonCopy;

        private readonly List<GameObject> _controlObjects = new List<GameObject>(1);
        public Transform Parent { get; }


        private static readonly Type[] _localisationComponentTypes = typeof(Manager.Scene).Assembly.GetTypes()
            .Where(t => string.Equals(t.Namespace, "Localize.Translate", StringComparison.Ordinal))
            .Where(t => typeof(Component).IsAssignableFrom(t))
            .ToArray();
        
        public OutfitPainterChannelButton(string text, Transform parent)
        {
            Text = text;
            OnClick = new Button.ButtonClickedEvent();
            Parent = parent;

            Visible = new BehaviorSubject<bool>(true);
            Visible.Subscribe(
                b =>
                {
                    foreach (var controlObject in ControlObjects)
                        controlObject.SetActive(b);
                });

            TextColor = new Color(0.090f, 0.118f, 0.141f);
        }

        public Button.ButtonClickedEvent OnClick { get; }

        public string Text { get; }

        private static Transform ButtonCopy
        {
            get
            {
                if (_buttonCopy == null)
                    MakeCopy();
                return _buttonCopy;
            }
        }

        private static void MakeCopy()
        {
            var original = GameObject.Find("DefaultColor").transform;

            _buttonCopy = Object.Instantiate(original, new GameObject("OutfitPainterButtonPrototype").transform, false);
            _buttonCopy.gameObject.SetActive(false);
            _buttonCopy.name = "btnCustom";

            var button = _buttonCopy.GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.targetGraphic.raycastTarget = true;

            button.GetComponent<RectTransform>().sizeDelta = new Vector2(414, 0);

            RemoveLocalisation(_buttonCopy.gameObject);
        }

        public void Initialize()
        {
            if (_buttonCopy == null)
                MakeCopy();
        }

        public void Dispose()
        {
            OnClick.RemoveAllListeners();
            IsDisposed = true;
        }

        public GameObject OnCreateControl()
        {
            var tr = Object.Instantiate(ButtonCopy, Parent, false);

            var button = tr.GetComponentInChildren<Button>();
            button.onClick.AddListener(OnClick.Invoke);

            button.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(175, 0);

            var text = tr.GetComponentInChildren<Text>();
            text.text = Text;
            text.color = TextColor;
            SetTextAutosize(text);

            return tr.gameObject;
        }

        internal static void RemoveLocalisation(GameObject control)
        {
            foreach (var localisationComponentType in _localisationComponentTypes)
            {
                foreach (var localisationComponent in control.GetComponentsInChildren(localisationComponentType, true))
                    UnityEngine.Object.DestroyImmediate(localisationComponent, false);
            }
        }

        internal static void SetTextAutosize(Text txtCmp)
        {
            txtCmp.resizeTextMaxSize = txtCmp.fontSize;
            txtCmp.resizeTextForBestFit = true;
            txtCmp.verticalOverflow = VerticalWrapMode.Truncate;
            txtCmp.horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        public bool IsDisposed { get; private set; }

        public void CreateControl()
        {
            var control = OnCreateControl();

            control.name += " OutfitPainter ";

            // Play nice with the accessory window (lower max width)
            if (MakerAPI.InsideMaker)
            {
                var layoutElement = control.GetComponent<LayoutElement>();
                if (layoutElement != null) layoutElement.minWidth = 175;
            }

            control.SetActive(Visible.Value);
            _controlObjects.Add(control);
        }

        public Color TextColor { get; set; } = MakerConstants.DefaultControlTextColor;

        public BehaviorSubject<bool> Visible { get; }

        public IEnumerable<GameObject> ControlObjects => _controlObjects.Where(x => x != null);

        public GameObject ControlObject => _controlObjects.FirstOrDefault(x => x != null);

        public bool Exists => _controlObjects.Any(x => x != null);
    }

}
