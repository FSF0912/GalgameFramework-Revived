using System;
using System.Threading;
using GalgameFramework.Extensions;
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

        public async GDTask TypeWorkAsync(string text, bool append = false)
        {
            if (IsTyping) return;
            IsTyping = true;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            if (append)
            {
                AppendText(text);
                VisibleRatio = 1 - (text.Length / Text.Length) + 0.05f;
            }
            else
            {
                VisibleRatio = 0;
                Text = text;
            }

            try
            {
                _tween?.Kill();
                _tween = CreateTween();

                _tween.TweenProperty(this, nameof(VisibleRatio), 1, PerCharSpeed * text.Length).SetPureLinear();

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