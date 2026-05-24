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

        public override void _ExitTree()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            base._ExitTree();
        }

        public async GDTask AnimationAsync(List<SingleAnimationTrack> animQueues)
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
            var token = _cts.Token;

            _animPlayer.Play($"{LibName}/{AnimName}");

            try
            {
                await ToSignal(_animPlayer, AnimationPlayer.SignalName.AnimationFinished).AsGDTask().AttachExternalCancellation(token);
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

        //public async GDTask

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