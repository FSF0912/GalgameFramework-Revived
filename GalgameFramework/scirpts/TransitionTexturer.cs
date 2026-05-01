using Godot;
using System.Threading;
using GodotTask;
using System;
using System.Threading.Tasks;
using Ease = Godot.Animation.InterpolationType;
using GD_Array = Godot.Collections.Array;
using GD_Dictionary = Godot.Collections.Dictionary;
using System.Collections.Generic;

namespace GalgameFramework
{
    public partial class TransitionTexturer : Control
    {
        public static readonly Color FadeColor = new Color(1, 1, 1, 0);
        public static readonly Color VisibleColor = new Color(1, 1, 1, 1);

        [Export] private TextureRect _firstTex;
        [Export] private TextureRect _secondTex;
        private CancellationTokenSource _cts;
        //private Tween _tween;

        private const string LibName = "texturer";
        private const string AnimName = "current";
        private AnimationPlayer _animPlayer;
        private AnimationLibrary _animLib;
        private float _actiontime = 0.6f;
        public bool IsAnimating { get; private set; } = false;

        public override void _Ready()
        {
            base._Ready();
            if (_firstTex == null) _firstTex = GetNode<TextureRect>("FirstTex");
            if (_secondTex == null) _secondTex = GetNode<TextureRect>("SecondTex");

            _animPlayer = new AnimationPlayer();
            AddChild(_animPlayer);
            _animLib = new AnimationLibrary();
            _animPlayer.AddAnimationLibrary(LibName, _animLib);
        }

/*
        public async GDTask TransitionAsync(TransitionType type, 
        Texture2D newTex, 
        float transDuration = 0.6f, bool queueFreeWhenComplete = false)
        {
            if (IsTransitioning) return;//will add parallel transition later
            IsTransitioning = true;

            transDuration = transDuration <= 0 ? _actiontime : transDuration;

            (GDTask task, Action onComplete) t = (default, null);
            switch (type)
            {
            case TransitionType.CrossFade:
                t = GenerateCrossfadeTask(newTex, transDuration);
                break;

            case TransitionType.FadeOut_Then_In:
                t = GenerateFadeOutThenInTask(newTex, transDuration);
                break;

            case TransitionType.FadeOut:
                t = GenerateFadeOutTask(transDuration, queueFreeWhenComplete);
                break;

            case TransitionType.Direct:
                _firstTex.Texture = newTex;
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
            _tween.TweenProperty(_firstTex, "modulate:a", 0, duration).SetPureSine();
            _tween.TweenProperty(_secondTex, "modulate:a", 1, duration).SetPureSine();
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
            _tween.TweenProperty(_firstTex, "modulate:a", 0, timeHalf).SetPureSine();
            _tween.TweenProperty(_secondTex, "modulate:a", 1, waitTime).SetPureSine().SetDelay(waitTime);

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
            _tween.TweenProperty(_firstTex, "modulate:a", 0, duration).SetPureSine();

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

        
*/

        public async Task AnimationAsync(List<SingleAnimationTrack> animQueues)
        {
            if (IsAnimating) return;
            IsAnimating = true;

            var anim = new Animation();
            foreach (var tk in animQueues)
            {
                switch (tk.TrackType)
                {
                    case AnimationTrackType.Value:
                        int valueTrackIdx = anim.AddTrack(Animation.TrackType.Value);
                        anim.TrackSetPath(valueTrackIdx, tk.path);
                        foreach (var (value, time) in tk.KeyFrames)
                        {
                            anim.TrackInsertKey(valueTrackIdx, time, value);
                        }
                        anim.TrackSetInterpolationType(valueTrackIdx, tk.ease);

                    break;

                    case AnimationTrackType.Method:
                        int methodTrackIdx = anim.AddTrack(Animation.TrackType.Method);
                        anim.TrackSetPath(methodTrackIdx, tk.path);
                        foreach (var (value, time) in tk.KeyFrames)
                        {
                            anim.TrackInsertKey(methodTrackIdx, time, (GD_Array)value);
                        }

                    break;

                    default:
                        continue;
                }
                
            }

            float maxTime = 0f;
            for (int i = 0; i < anim.GetTrackCount(); i++)
            {
                int keyCount = anim.TrackGetKeyCount(i);
                if (keyCount > 0)
                {
                    float lastKeyTime = (float)anim.TrackGetKeyTime(i, keyCount - 1);
                    maxTime = MathF.Max(lastKeyTime, maxTime);
                }
            }
            anim.Length = maxTime;

            if (_animLib.HasAnimation(AnimName)) _animLib.RemoveAnimation(AnimName);
            _animLib.AddAnimation(AnimName, anim);

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            _animPlayer.Play($"{LibName}/{AnimName}");

            try
            {
                await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished).AsGDTask().AttachExternalCancellation(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                _animPlayer.Seek(maxTime, true);
                _animPlayer.Stop();
            }
            finally
            {
                IsAnimating = false;
            }
        }

        public List<SingleAnimationTrack> GenerateCrossfadeTrack(Texture2D tex, float duration = -1, Ease ease = Ease.Cubic)
        {
            if (duration <= 0) duration = _actiontime;
            var tracks = new List<SingleAnimationTrack>();

            var startKeyTex = new GD_Dictionary
            {
                {"method", nameof(SetTexture_ReflectionCallback)},
                {"args", new GD_Array {false, tex} }
            };

            var startKeyColorFirst = new GD_Dictionary
            {
                {"method", nameof(SetTextureVisible_ReflectionCallback)},
                {"args", new GD_Array {true, true} }
            };

            var startKeyColorSecond = new GD_Dictionary
            {
                {"method", nameof(SetTextureVisible_ReflectionCallback)},
                {"args", new GD_Array {false, false} }
            };

            var endKeyTexFirst = new GD_Dictionary
            {
                {"method", nameof(SetTexture_ReflectionCallback)},
                {"args", new GD_Array {true, tex} }
            };

            var endKeyTexSecond = new GD_Dictionary
            {
                {"method", nameof(SetTexture_ReflectionCallback)},
                {"args", new GD_Array {false, default(Texture2D)} }
            };

            var endKeyColorFirst = new GD_Dictionary
            {
                {"method", nameof(SetTextureVisible_ReflectionCallback)},
                {"args", new GD_Array {true, true} }
            };

            var endKeyColorSecond = new GD_Dictionary
            {
                {"method", nameof(SetTextureVisible_ReflectionCallback)},
                {"args", new GD_Array {false, false} }
            };

            var methodTrack = new SingleAnimationTrack(AnimationTrackType.Method, ".",
            [
                (startKeyTex, 0f),
                (startKeyColorFirst, 0f),
                (startKeyColorSecond, 0f),
                (endKeyTexFirst, duration),
                (endKeyTexSecond, duration),
                (endKeyColorFirst, duration),
                (endKeyColorSecond, duration)
            ]);

            var animTrackFirst = new SingleAnimationTrack(AnimationTrackType.Value, "FirstTex:modulate:a",
            [
                (1f, 0f),
                (0f, duration)
            ], ease); 

            var animTrackSecond = new SingleAnimationTrack(AnimationTrackType.Value, "SecondTex:modulate:a",
            [
                (0f, 0f),
                (1f, duration)
            ], ease);

            tracks.Add(methodTrack);
            tracks.Add(animTrackFirst);
            tracks.Add(animTrackSecond);

            return tracks;
        }

        public void SetTexture_ReflectionCallback(bool isFirst, Texture2D tex)
        {
            TextureRect target = isFirst ? _firstTex : _secondTex;
            target.Texture = tex;
        }

        public void SetTextureVisible_ReflectionCallback(bool isFirst, bool is_visible)
        {
            TextureRect target = isFirst ? _firstTex : _secondTex;
            target.Modulate = is_visible ? VisibleColor : FadeColor;
        }
    }


    public enum AnimationTrackType
    {
        Value,
        Method
    };
    public class SingleAnimationTrack
    {
        /// <summary>
        /// 动画轨道类型，决定轨道是 <b>操作属性 (Value)</b> 还是 <b>调用方法 (Method)</b>。
        /// </summary>
        /// <remarks>
        /// <para><b>注意：</b>当 <c>TrackType</c> 为 <c>Method</c> 时，配置规则如下：</para>
        /// <list type="bullet">
        ///     <item>
        ///         <description>
        ///         <b>数据结构：</b>KeyFrames 中的 <c>value</c> 必须是一个 <c>Godot.Collections.Dictionary</c>。
        ///         <code>
        ///         value = new Godot.Collections.Dictionary 
        ///         { 
        ///             { "method", "a_method" }, 
        ///             { "args",   new Godot.Collections.Array { arg1, arg2, ... } }
        ///         };
        ///         </code>
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description><b>节点路径：</b>声明 <c>path</c> 为 <c>"."</c>，表示调用当前节点的方法。</description>
        ///     </item>
        ///     <item>
        ///         <description><b>方法定义：</b><c>void a_method(arg1_type arg1, arg2_type arg2, ...)</c></description>
        ///     </item>
        /// </list>
        /// </remarks>
        public AnimationTrackType TrackType = AnimationTrackType.Value;
        /// <summary>
        /// 被操作的属性路径，如"modulate:a","rect_scale:y","FirstTex/modulate:a"。
        /// </summary>
        public string path;
        /// <summary>
        /// 关键帧数据，包含每帧的值和时间。
        /// value可以是float、Color等类型，time为该帧插入的时间（秒）。
        /// </summary>
        public (Variant value, float time)[] KeyFrames;
        /// <summary>
        /// 插值类型，决定了关键帧之间的过渡方式，如线性、立方等。
        /// </summary>
        public Ease ease = Ease.Cubic;

        public SingleAnimationTrack(AnimationTrackType trackType, string path, (Variant value, float time)[] keyFrames, Ease ease = Ease.Cubic)
        {
            this.TrackType = trackType;
            this.path = path;
            this.KeyFrames = keyFrames;
            this.ease = ease;
        }
    }
}