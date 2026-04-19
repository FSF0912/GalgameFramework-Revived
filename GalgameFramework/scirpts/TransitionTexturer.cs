using Godot;
using System.Threading;
using GodotTask;
using GalgameFramework.Extensions;
using System;

namespace GalgameFramework
{
    public enum TransitionType
    {
        CrossFade,
        FadeOut_Then_In,
        FadeOut,
        Direct
    };

    public partial class TransitionTexturer : Control
    {
        private static Color AppearColor = Colors.White;
        private static Color FadeColor = new Color(1, 1, 1, 0);

        [Export] private TextureRect _firstTex;
        [Export] private TextureRect _secondTex;
        private CancellationTokenSource _cts;
        private Tween _tween;

        private float _actiontime = 0.6f;
        public bool IsTransitioning { get; private set; } = false;

        public override void _Ready()
        {
            base._Ready();
            if (_firstTex == null) _firstTex = GetNode<TextureRect>("First");
            if (_secondTex == null) _secondTex = GetNode<TextureRect>("Second");

            ConfigureTextureColor(true, false);
        }

        public async GDTask TransitionAsync(TransitionType type, 
        string newTexPath = null, 
        float transDuration = 0.6f, bool queueFreeWhenComplete = false)
        {
            if (IsTransitioning) return;//will add parallel transition later
            IsTransitioning = true;

            transDuration = transDuration <= 0 ? _actiontime : transDuration;

            (GDTask task, Action onComplete) t = (default, null);
            switch (type)
            {
            case TransitionType.CrossFade:
                if (!newTexPath.TryLoadRes<Texture2D>(out var newTex)) goto case default;
                t = GenerateCrossfadeTask(newTex, transDuration);
                break;

            case TransitionType.FadeOut_Then_In:
                if (!newTexPath.TryLoadRes<Texture2D>(out var newTex2)) goto case default;
                t = GenerateFadeOutThenInTask(newTex2, transDuration);
                break;

            case TransitionType.FadeOut:
                t = GenerateFadeOutTask(transDuration, queueFreeWhenComplete);
                break;

            case TransitionType.Direct:
                if (!newTexPath.TryLoadRes<Texture2D>(out var newTex3)) goto case default;
                _firstTex.Texture = newTex3;
                _secondTex.Texture = null;
                ConfigureTextureColor(true, false);
                return;
            
            default:
                IsTransitioning = false;
                return;
            }

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            _tween?.Kill();
            _tween = CreateTween();

            try
            {
                await t.task.AttachExternalCancellation(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                t.onComplete?.Invoke();
            }
            finally
            {
                IsTransitioning = false;
            }
        }

        #region Helpers
        (GDTask task, Action onComplete) GenerateCrossfadeTask(Texture2D newTexture, float duration)
        {
            _secondTex.Texture = newTexture;
            ConfigureTextureColor(true, false);
            _tween.SetParallel(true);
            _tween.TweenProperty(_firstTex, nameof(Modulate.A), 0, duration).SetPureSine();
            _tween.TweenProperty(_secondTex, nameof(Modulate.A), 1, duration).SetPureSine();
            Action onComplete = () =>
            {
            _firstTex.Texture = newTexture;
            _secondTex.Texture = null;
            ConfigureTextureColor(true, false);
            };
            _tween.Finished += onComplete;

            GDTask task = _tween.ToSignal(_tween, Tween.SignalName.Finished).AsGDTask();
            return (task, onComplete);
        }

        (GDTask task, Action onComplete) GenerateFadeOutThenInTask(Texture2D newTexture, float duration)
        {
            float timeHalf = duration - duration * 0.1f;
            float waitTime = duration * 0.1f;
            _secondTex.Texture = newTexture;
            ConfigureTextureColor(true, false);
            _secondTex.Modulate = FadeColor;
            //reminder:_tween.SetParallel(false);
            _tween.TweenProperty(_firstTex, nameof(Modulate.A), 0, timeHalf).SetPureSine();
            _tween.TweenProperty(_secondTex, nameof(Modulate.A), 1, waitTime).SetPureSine().SetDelay(waitTime);

            Action onComplete = () =>
            {
            _firstTex.Texture = newTexture;
            _secondTex.Texture = null;
            ConfigureTextureColor(true, false);
            };
            _tween.Finished += onComplete;

            var task = ToSignal(_tween, Tween.SignalName.Finished).AsGDTask();
            return (task, onComplete);
        }

        (GDTask task, Action onComplete) GenerateFadeOutTask(float duration, bool queueFreeWhenComplete)
        {
            ConfigureTextureColor(true, false);
            _tween.TweenProperty(_firstTex, nameof(Modulate.A), 0, duration).SetPureSine();

            Action onComplete = () =>
            {
            ConfigureTextureColor(false, false);
            if (queueFreeWhenComplete) QueueFree();
            };

            _tween.Finished += onComplete;
            var task = ToSignal(_tween, Tween.SignalName.Finished).AsGDTask();
            return (task, onComplete);
        }
        #endregion

        void ConfigureTextureColor(bool firstVisible, bool secondVisible)
        {
            _firstTex.Modulate = firstVisible ? AppearColor : FadeColor;
            _secondTex.Modulate = secondVisible ? AppearColor : FadeColor;
        }
    }
}