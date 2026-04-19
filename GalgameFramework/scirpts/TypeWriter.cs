using System;
using System.Threading;
using Godot;
using GodotTask;

namespace GalgameFramework
{
    public partial class TypeWriter : RichTextLabel
    {
        [Export] public float PerCharSpeed = 0.05f;
        public bool IsTyping { get; private set; }
        CancellationTokenSource _cts;
        Tween _tween;

        public override void _Ready()
        {
            VisibleRatio = 0;
        }

        public async GDTask TypeWorkAsync(string text)
        {
            if (IsTyping) return;
            IsTyping = true;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            VisibleRatio = 0;
            Text = text;
            var totalChars = Text.Length;

            try
            {
                _tween?.Kill();
                _tween = CreateTween();

                _tween.TweenProperty(this, nameof(VisibleRatio), 1, PerCharSpeed * totalChars).
                SetEase(Tween.EaseType.InOut).
                SetTrans(Tween.TransitionType.Linear);

                await _tween.ToSignal(this, Tween.SignalName.Finished).AsGDTask().
                AttachExternalCancellation(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                _tween?.Kill();
                VisibleRatio = 1;
            }
            finally
            {
                IsTyping = false;
            }
        }

        public void Interrupt()
        {
            if (!IsTyping) return;
            _cts.Cancel();
        }
    }
}