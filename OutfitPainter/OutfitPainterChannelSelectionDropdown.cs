using Illusion.Extensions;
using KKAPI;
using KKAPI.Maker;
using KKAPI.Utilities;
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
    // Shamelessly ripping off the KKAPI dropdown code which, sadly, does not allow me to control where it goes...

    public class OutfitPainterChannelSelectionDropdown : IDisposable
    {
        private static Transform _dropdownCopy;
        private readonly List<GameObject> _controlObjects = new List<GameObject>(1);


        public OutfitPainterChannelSelectionDropdown(string settingName, string[] options, Transform parent, int initialValue)            
        {
            SettingName = settingName;
            Options = options;
            Parent = parent;

            _incomingValue = new BehaviorSubject<int>(initialValue);
            _outgoingValue = new Subject<int>();
            Visible = new BehaviorSubject<bool>(true);
            Visible.Subscribe(
                b =>
                {
                    foreach (var controlObject in ControlObjects)
                        controlObject.SetActive(b);
                });

        }

        public string[] Options { get; }

        public string SettingName { get; }

        public Color TextColor { get; set; } = new Color(0.922f, 0.886f, 0.843f);

        public Transform Parent { get; }

        public bool IsDisposed { get; private set; }
        public BehaviorSubject<bool> Visible { get; }

        public Dropdown Dropdown { get; set; }


        private void MakeCopy()
        {
            _dropdownCopy = Object.Instantiate(GameObject.Find("ddBirthday"), new GameObject("OutfitPainterDropdownPrototype").transform, false).transform;
            _dropdownCopy.gameObject.SetActive(false);
            _dropdownCopy.name = "ddList";

            // Setup layout of the group
            var mainle = _dropdownCopy.GetComponent<LayoutElement>();
            mainle.minHeight = 40;
            mainle.preferredHeight = 40;
            mainle.flexibleHeight = 0;
            _dropdownCopy.gameObject.AddComponent<HorizontalLayoutGroup>();

            // Destroy unnecessary objects
            Text text = null;
            Dropdown dropdown = null;
            foreach (var child in _dropdownCopy.transform.Children())
            {
                if (text == null)
                {
                    text = child.GetComponent<Text>();
                    if (text != null) continue;
                }
                else if (dropdown == null)
                {
                    dropdown = child.GetComponent<Dropdown>();
                    if (dropdown != null) continue;
                }
                Object.DestroyImmediate(child.gameObject);
            }
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (dropdown == null) throw new ArgumentNullException(nameof(dropdown));

            // Needed for HorizontalLayoutGroup
            text.gameObject.AddComponent<LayoutElement>();
            var dle = dropdown.gameObject.AddComponent<LayoutElement>();
            dle.minWidth = 230;
            dle.flexibleWidth = 0;

            dropdown.name = "ddListInner";

            text.alignment = TextAnchor.MiddleLeft;
            SetTextAutosize(text);

            dropdown.onValueChanged.ActuallyRemoveAllListeners();
            dropdown.ClearOptions();
            dropdown.GetComponent<Image>().raycastTarget = true;
            dropdown.template.GetComponentInChildren<UI_ToggleEx>().image.raycastTarget = true;
            SetTextAutosize(dropdown.template.GetComponentInChildren<Text>(true));

            RemoveLocalisation(_dropdownCopy.gameObject);
        }

        public void Initialize()
        {
            if (_dropdownCopy == null)
                MakeCopy();
        }

        public GameObject OnCreateControl()
        {
            var tr = Object.Instantiate(_dropdownCopy, Parent, false);

            var settingName = tr.Find("textKindTitle").GetComponent<Text>();
            settingName.text = SettingName;
            settingName.color = TextColor;

            var dropdown = tr.GetComponentInChildren<Dropdown>();
            dropdown.options.AddRange(Options.Select(x => new Dropdown.OptionData(x)));

            Dropdown = dropdown;

            dropdown.onValueChanged.AddListener(SetValue);
            BufferedValueChanged.Subscribe(i => dropdown.value = i);

            // Fix box not updating if BufferedValueChanged equals the default dropdown val
            if (Value == dropdown.value)
            {
                dropdown.RefreshShownValue();
                SetValue(dropdown.value);
            }

            return tr.gameObject;
        }

        public void UpdateOptions(string[] options)
        {
            Dropdown.ClearOptions();
            Dropdown.AddOptions(options.ToList());
            Dropdown.RefreshShownValue();
        }

        private static readonly Type[] _localisationComponentTypes = typeof(Manager.Scene).Assembly.GetTypes()
            .Where(t => string.Equals(t.Namespace, "Localize.Translate", StringComparison.Ordinal))
            .Where(t => typeof(Component).IsAssignableFrom(t))
            .ToArray();

        private void RemoveLocalisation(GameObject control)
        {
            foreach (var localisationComponentType in _localisationComponentTypes)
            {
                foreach (var localisationComponent in control.GetComponentsInChildren(localisationComponentType, true))
                    UnityEngine.Object.DestroyImmediate(localisationComponent, false);
            }
        }

        private void SetTextAutosize(Text txtCmp)
        {
            txtCmp.resizeTextMaxSize = txtCmp.fontSize;
            txtCmp.resizeTextForBestFit = true;
            txtCmp.verticalOverflow = VerticalWrapMode.Truncate;
            txtCmp.horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        private readonly BehaviorSubject<int> _incomingValue;
        private readonly Subject<int> _outgoingValue;

        public int Value
        {
            get => _incomingValue.Value;
            set => SetValue(value);
        }
        public IObservable<int> ValueChanged => _outgoingValue;

        protected IObservable<int> BufferedValueChanged => _incomingValue;

        public void SetValue(int newValue)
        {
            SetValue(newValue, true);
        }

        private bool _firingEnabled;

        public void SetValue(int newValue, bool fireEvents)
        {
            if (Equals(newValue, _incomingValue.Value))
                return;

            _incomingValue.OnNext(newValue);

            if (_firingEnabled && fireEvents)
                _outgoingValue.OnNext(newValue);
        }

        public void Dispose()
        {
            _incomingValue.Dispose();
            _outgoingValue.Dispose();
            IsDisposed = true;
        }

        public bool Exists => _controlObjects.Any(x => x != null);

        public IEnumerable<GameObject> ControlObjects => _controlObjects.Where(x => x != null);

        public GameObject ControlObject => _controlObjects.FirstOrDefault(x => x != null);

        public void CreateControl()
        {
            var wasCreated = Exists;

            _firingEnabled = false;

            var control = OnCreateControl();

            control.name += " OutfitPainter ";

            // Play nice with the accessory window (lower max width)
            if (MakerAPI.InsideMaker)
            {
                var layoutElement = control.GetComponent<LayoutElement>();
                if (layoutElement != null) layoutElement.minWidth = 300;
            }

            control.SetActive(Visible.Value);
            _controlObjects.Add(control);

            _firingEnabled = true;

            // Trigger value changed events after the control is created to make sure everything updates its state
            // Make sure this only happens with the 1st copy of the control, so it's not fired for every accessory slot
            if (!wasCreated)
            {
                _incomingValue.OnNext(_incomingValue.Value);
                if (!MakerAPI.InsideAndLoaded)
                    _outgoingValue.OnNext(_incomingValue.Value);
            }
        }        

    }
}
