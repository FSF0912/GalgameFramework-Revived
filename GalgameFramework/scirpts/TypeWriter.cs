using System;
using System.Threading;
using GalgameFramework.Extensions;
using Godot;
using GodotTask;
using MathE = MathExtensions;

namespace GalgameFramework
{
    public partial class TypeWriter : Control
    {
        [ExportGroup("Typewriter Settings")]
        [Export] public RichTextLabel ContextLabel;
        [Export] public Label NameLabel;
        [Export] public float PerCharSpeed = 0.05f;

        [ExportGroup("Follow Settings")]
        [Export] public bool EnableFollow = true;
        public Control FollowTarget { get; set; }
        public Vector2 OriginalPosition;
        [ExportSubgroup("Movement")]
        [Export] public Vector2 LimitMovePixels = new Vector2(40, 20);
        [Export] public Vector2 OffsetRatio = new Vector2(0.5f, 0.5f);
        [ExportSubgroup("Acceleration&Curves")]
        [Export] public Curve LerpCurve;
        [Export] public float LerpSpeed = 5f;
        [Export] public Curve AccelerationCurve;
        [Export] public float DeltaA_Threshold = 1500;

        public bool IsTyping { get; private set; }


        CancellationTokenSource _cts;
        Tween _tween;

        Vector2 _lastPosition;
        Vector2 _lastVelocity;

        public override void _Ready()
        {
            ContextLabel.VisibleRatio = 0;
            OriginalPosition = Position;

        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            if (!EnableFollow) return;
            Vector2 followTargetRatio = Vector2.Zero;
            if (FollowTarget != null)
                followTargetRatio = FollowTarget.Position / GetViewportRect().Size - OffsetRatio;

            //acceleration
            Vector2 deltaV = (FollowTarget.Position - _lastPosition) / (float)delta;
            Vector2 deltaA = (deltaV - _lastVelocity) / (float)delta;
            float acceleration = deltaA.Length();
            _lastPosition = FollowTarget.Position;
            _lastVelocity = deltaV;

            acceleration = MathE.Clamp01(acceleration / DeltaA_Threshold);
            //

            float weight_linear = MathE.Clamp01(LerpSpeed * (float)delta);
            float weight_processed = LerpCurve.Sample(weight_linear);
            float weight_accelerationed = MathE.Clamp01(weight_processed * AccelerationCurve.Sample(acceleration));
            var Move = MathE.Lerp(Position, OriginalPosition + LimitMovePixels * followTargetRatio, weight_accelerationed);
            Position = Move;
        }

        public async GDTask TypeWorkAsync(string text, bool append = false)
        {
            if (IsTyping) return;
            IsTyping = true;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            if (append)
            {
                ContextLabel.AppendText(text);
                ContextLabel.VisibleRatio = 1 - (text.Length / ContextLabel.Text.Length) + 0.05f;
            }
            else
            {
                ContextLabel.VisibleRatio = 0;
                ContextLabel.Text = text;
            }

            try
            {
                _tween?.Kill();
                _tween = CreateTween();

                _tween.TweenProperty(ContextLabel, nameof(ContextLabel.VisibleRatio), 1, PerCharSpeed * text.Length).SetPureLinear();

                await _tween.ToSignal(this, Tween.SignalName.Finished).AsGDTask().
                AttachExternalCancellation(token);
            }
            catch (OperationCanceledException)
            {
                _tween?.Kill();
                ContextLabel.VisibleRatio = 1;
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