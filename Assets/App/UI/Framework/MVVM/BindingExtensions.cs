using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Miyo.UI.MVVM
{
    public static class BindingExtensions
    {
        // String → TMP_Text
        public static IDisposable BindTo(this IReadOnlyReactiveProperty<string> property, TMP_Text text)
        {
            return property.Subscribe(value => text.text = value);
        }

        // Int → TMP_Text
        public static IDisposable BindTo(this IReadOnlyReactiveProperty<int> property, TMP_Text text, string format = "{0}")
        {
            return property.Subscribe(value => text.text = string.Format(format, value));
        }

        // Float → TMP_Text
        public static IDisposable BindTo(this IReadOnlyReactiveProperty<float> property, TMP_Text text, string format = "{0:F1}")
        {
            return property.Subscribe(value => text.text = string.Format(format, value));
        }

        // Sprite → Image
        public static IDisposable BindTo(this IReadOnlyReactiveProperty<Sprite> property, Image image)
        {
            return property.Subscribe(value => image.sprite = value);
        }

        // Float → Image.fillAmount
        public static IDisposable BindToFill(this IReadOnlyReactiveProperty<float> property, Image image)
        {
            return property.Subscribe(value => image.fillAmount = value);
        }

        // Bool → GameObject.SetActive
        public static IDisposable BindTo(this IReadOnlyReactiveProperty<bool> property, GameObject go)
        {
            return property.Subscribe(value => go.SetActive(value));
        }

        // Bool → CanvasGroup visibility
        public static IDisposable BindTo(this IReadOnlyReactiveProperty<bool> property, CanvasGroup canvasGroup)
        {
            return property.Subscribe(value =>
            {
                canvasGroup.alpha = value ? 1f : 0f;
                canvasGroup.interactable = value;
                canvasGroup.blocksRaycasts = value;
            });
        }

        // Bool → Selectable.interactable
        public static IDisposable BindToInteractable(this IReadOnlyReactiveProperty<bool> property, Selectable selectable)
        {
            return property.Subscribe(value => selectable.interactable = value);
        }

        // Color → Image
        public static IDisposable BindTo(this IReadOnlyReactiveProperty<Color> property, Image image)
        {
            return property.Subscribe(value => image.color = value);
        }

        // Color → TMP_Text
        public static IDisposable BindTo(this IReadOnlyReactiveProperty<Color> property, TMP_Text text)
        {
            return property.Subscribe(value => text.color = value);
        }

        // Float → Slider (two-way)
        public static IDisposable BindTwoWay(this ReactiveProperty<float> property, Slider slider)
        {
            var composite = new CompositeDisposable();

            // Property → Slider
            property.Subscribe(value =>
            {
                if (Math.Abs(slider.value - value) > 0.001f)
                    slider.SetValueWithoutNotify(value);
            }).AddTo(composite);

            // Slider → Property (Value = ile aboneler tetiklensin, örn. limit metni güncellensin)
            void OnSliderChanged(float value) => property.Value = value;
            slider.onValueChanged.AddListener(OnSliderChanged);
            composite.Add(new Disposable(() => slider.onValueChanged.RemoveListener(OnSliderChanged)));

            return composite;
        }

        // Button click
        public static IDisposable BindClick(this Button button, Action action)
        {
            void OnClick() => action?.Invoke();
            button.onClick.AddListener(OnClick);
            return new Disposable(() => button.onClick.RemoveListener(OnClick));
        }

        // TMP_InputField (two-way)
        public static IDisposable BindTwoWay(this ReactiveProperty<string> property, TMP_InputField inputField)
        {
            var composite = new CompositeDisposable();

            property.Subscribe(value =>
            {
                if (inputField.text != value)
                    inputField.SetTextWithoutNotify(value);
            }).AddTo(composite);

            void OnValueChanged(string value) => property.SetValueWithoutNotify(value);
            inputField.onValueChanged.AddListener(OnValueChanged);
            composite.Add(new Disposable(() => inputField.onValueChanged.RemoveListener(OnValueChanged)));

            return composite;
        }

        // Toggle (two-way)
        public static IDisposable BindTwoWay(this ReactiveProperty<bool> property, Toggle toggle)
        {
            var composite = new CompositeDisposable();

            property.Subscribe(value =>
            {
                if (toggle.isOn != value)
                    toggle.SetIsOnWithoutNotify(value);
            }).AddTo(composite);

            void OnToggleChanged(bool value) => property.SetValueWithoutNotify(value);
            toggle.onValueChanged.AddListener(OnToggleChanged);
            composite.Add(new Disposable(() => toggle.onValueChanged.RemoveListener(OnToggleChanged)));

            return composite;
        }

        // DateTime? → DateTimeInputField (two-way)
        public static IDisposable BindTwoWay(this ReactiveProperty<DateTime?> property, DateTimeInputField field)
        {
            var composite = new CompositeDisposable();

            // Property → Field
            property.Subscribe(value => field.SetValueWithoutNotify(value)).AddTo(composite);

            // Field → Property (Value = ile aboneler tetiklensin, örn. CanSubmit güncellensin)
            void OnFieldChanged(DateTime? value) => property.Value = value;
            field.OnValueChanged += OnFieldChanged;
            composite.Add(new Disposable(() => field.OnValueChanged -= OnFieldChanged));

            return composite;
        }
    }
}
