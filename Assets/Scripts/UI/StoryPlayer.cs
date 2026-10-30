using System;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SaqrAndAqab.Core;

namespace SaqrAndAqab.UI
{
    // Plays a list of story beats: image (Ken Burns zoom), cross-fade, RTL text box,
    // optional speaker. Tap anywhere advances; "common.skip" skips all.
    public class StoryPlayer : MonoBehaviour
    {
        public struct Beat
        {
            public string imageId;
            public string textKey;
            public string speakerKey;
            public float duration;
        }

        static readonly Color BackColor = new Color(0.02f, 0.03f, 0.06f, 1f);
        static readonly Color TextColor = new Color(1f, 0.96f, 0.88f, 1f);
        static readonly Color SpeakerColor = new Color(0.6f, 0.8f, 1f, 1f);

        Beat[] _beats;
        Action _onComplete;

        Image _imageA;
        Image _imageB;
        bool _showingA = true;

        RTLTextMeshPro _text;
        RTLTextMeshPro _speaker;

        Button _tapButton;
        Button _skipButton;

        int _index;
        float _timer;
        float _duration = 4f;

        enum State { Showing, Fading }
        State _state = State.Showing;
        float _fadeTimer;
        const float FadeDuration = 0.5f;

        bool _tapRequested;

        public static StoryPlayer Play(Transform canvas, Beat[] beats, Action onComplete)
        {
            if (canvas == null || beats == null || beats.Length == 0) return null;

            var go = new GameObject("Story Player", typeof(RectTransform), typeof(StoryPlayer));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(canvas, false);
            UIFactory.Fill(rect);

            var player = go.GetComponent<StoryPlayer>();
            player._beats = beats;
            player._onComplete = onComplete;
            player.Build();
            player.ShowBeat(0);
            return player;
        }

        void Build()
        {
            var rect = (RectTransform)transform;

            var back = gameObject.AddComponent<Image>();
            back.color = BackColor;
            back.raycastTarget = true;

            // Two image slots for cross-fade.
            _imageA = CreateImage(rect, "Image A");
            _imageB = CreateImage(rect, "Image B");
            _imageB.color = new Color(1f, 1f, 1f, 0f);

            // Text box at the bottom.
            var textBox = CreateRect(rect, "Text Box", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 40f), new Vector2(1700f, 260f));
            var boxImage = textBox.gameObject.AddComponent<Image>();
            boxImage.color = new Color(0.02f, 0.03f, 0.06f, 0.78f);
            boxImage.raycastTarget = false;

            _speaker = UIFactory.CreateTextRaw(textBox, "", 30, SpeakerColor, TextAlignmentOptions.Center, true);
            UIFactory.Place(_speaker.rectTransform, new Vector2(0f, 80f), new Vector2(1600f, 50f));
            _speaker.gameObject.SetActive(false);

            _text = UIFactory.CreateTextRaw(textBox, "", 34, TextColor, TextAlignmentOptions.Center, true);
            UIFactory.Place(_text.rectTransform, new Vector2(0f, -20f), new Vector2(1600f, 180f));

            // Skip button (top-right).
            _skipButton = UIFactory.CreateButton(rect, "common.skip", OnSkipPressed, new Vector2(200f, 70f), 28);
            var skipRect = _skipButton.GetComponent<RectTransform>();
            skipRect.anchorMin = new Vector2(1f, 1f);
            skipRect.anchorMax = new Vector2(1f, 1f);
            skipRect.pivot = new Vector2(1f, 1f);
            skipRect.anchoredPosition = new Vector2(-28f, -28f);

            // Full-screen tap target (behind the skip button's raycast priority is handled by button order).
            var tap = CreateRect(rect, "Tap", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            tap.anchorMin = Vector2.zero;
            tap.anchorMax = Vector2.one;
            tap.offsetMin = Vector2.zero;
            tap.offsetMax = Vector2.zero;
            var tapImage = tap.gameObject.AddComponent<Image>();
            tapImage.color = new Color(0f, 0f, 0f, 0f);
            tapImage.raycastTarget = true;
            _tapButton = tap.gameObject.AddComponent<Button>();
            _tapButton.targetGraphic = tapImage;
            _tapButton.transition = Selectable.Transition.None;
            _tapButton.onClick.AddListener(OnTapPressed);
        }

        void ShowBeat(int index)
        {
            if (index < 0 || index >= _beats.Length)
            {
                Finish();
                return;
            }

            _index = index;
            _timer = 0f;
            _state = State.Showing;

            var beat = _beats[index];
            _duration = beat.duration > 0f ? beat.duration : 4f;

            var sprite = AssetLibrary.GetSprite(beat.imageId);

            Image target = _showingA ? _imageA : _imageB;
            // Reset both to a known state: current fully visible, other hidden.
            if (_showingA)
            {
                _imageA.sprite = sprite;
                _imageA.color = Color.white;
                _imageA.rectTransform.localScale = Vector3.one;
                _imageB.color = new Color(1f, 1f, 1f, 0f);
            }
            else
            {
                _imageB.sprite = sprite;
                _imageB.color = Color.white;
                _imageB.rectTransform.localScale = Vector3.one;
                _imageA.color = new Color(1f, 1f, 1f, 0f);
            }

            if (_text != null) _text.text = Loc.T(beat.textKey);

            if (!string.IsNullOrEmpty(beat.speakerKey))
            {
                if (_speaker != null)
                {
                    _speaker.text = Loc.T(beat.speakerKey);
                    _speaker.gameObject.SetActive(true);
                }
            }
            else if (_speaker != null)
            {
                _speaker.gameObject.SetActive(false);
            }
        }

        void OnTapPressed()
        {
            _tapRequested = true;
        }

        void OnSkipPressed()
        {
            Finish();
        }

        void Update()
        {
            if (_beats == null) return;

            float dt = Time.deltaTime;

            if (_state == State.Showing)
            {
                _timer += dt;

                // Ken Burns: slow zoom.
                Image current = _showingA ? _imageA : _imageB;
                if (current != null)
                {
                    float progress = _duration > 0f ? Mathf.Clamp01(_timer / _duration) : 1f;
                    float scale = 1f + 0.08f * progress;
                    current.rectTransform.localScale = new Vector3(scale, scale, 1f);
                }

                bool advance = _tapRequested || (_duration > 0f && _timer >= _duration);
                _tapRequested = false;

                if (advance)
                {
                    if (_index >= _beats.Length - 1)
                    {
                        Finish();
                    }
                    else
                    {
                        StartFade();
                    }
                }
            }
            else if (_state == State.Fading)
            {
                _fadeTimer += dt;
                float t = _duration > 0f ? Mathf.Clamp01(_fadeTimer / FadeDuration) : 1f;

                Image current = _showingA ? _imageA : _imageB;
                Image next = _showingA ? _imageB : _imageA;

                if (current != null) current.color = new Color(1f, 1f, 1f, 1f - t);
                if (next != null) next.color = new Color(1f, 1f, 1f, t);

                if (t >= 1f)
                {
                    _showingA = !_showingA;
                    ShowBeat(_index + 1);
                }
            }
        }

        void StartFade()
        {
            _state = State.Fading;
            _fadeTimer = 0f;

            // Prepare the next image slot with the next beat's sprite.
            var nextBeat = _beats[_index + 1];
            var sprite = AssetLibrary.GetSprite(nextBeat.imageId);
            Image next = _showingA ? _imageB : _imageA;
            if (next != null)
            {
                next.sprite = sprite;
                next.color = new Color(1f, 1f, 1f, 0f);
                next.rectTransform.localScale = Vector3.one;
            }
        }

        void Finish()
        {
            if (_onComplete != null) _onComplete();
            Destroy(gameObject);
        }

        static Image CreateImage(RectTransform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.preserveAspect = false;
            image.raycastTarget = false;
            return image;
        }

        static RectTransform CreateRect(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }
    }
}
