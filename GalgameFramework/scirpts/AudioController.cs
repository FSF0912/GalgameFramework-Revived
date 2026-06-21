using GalgameFramework.Extensions;
using Godot;
using System;
using GodotTask;
using System.Threading;

[GlobalClass]
public partial class AudioController : Node
{
    public static AudioController Instance { get; private set; }
    private float _BGMPanning;
    public float BGMPanning
    {
        get => _BGMPanning;
        set
        {
            var val = Mathf.Clamp(value, -1f, 1f);
            _BGMPanning = val;
            if (AudioServer.GetBusEffect(_bgm_BusIndex, 0) is AudioEffectPanner panner)
            {
                panner.Pan = val;
            }

        }
    }

    private float _VoicePanning;
    public float VoicePanning
    {
        get => _VoicePanning;
        set
        {
            var val = Mathf.Clamp(value, -1f, 1f);
            _VoicePanning = val;
            if (AudioServer.GetBusEffect(_voice_BusIndex, 0) is AudioEffectPanner panner)
            {
                panner.Pan = val;
            }
        }
    }

    private float _SFXPanning;
    public float SFXPanning
    {
        get => _SFXPanning;
        set
        {
            var val = Mathf.Clamp(value, -1f, 1f);
            _SFXPanning = val;
            if (AudioServer.GetBusEffect(_sfx_BusIndex, 0) is AudioEffectPanner panner)
            {
                panner.Pan = val;
            }
        }
    }

    AudioStreamPlayer _bgmPlayer;
    int _bgm_BusIndex;
    AudioStreamPlayer _voicePlayer;
    int _voice_BusIndex;
    AudioStreamPlayer _sfxPlayer;
    int _sfx_BusIndex;
    Tween _fadeTween;
    CancellationTokenSource _cts;

    public override void _EnterTree()
    {
        base._EnterTree();
        Instance = this;
    }


    public override void _Ready()
    {
        _bgmPlayer = new AudioStreamPlayer() {Name = "BGMPlayer"};
        AddChild(_bgmPlayer);
        _bgm_BusIndex = AudioServer.GetBusIndex("BGM");
        _bgmPlayer.Bus = "BGM";

        _voicePlayer = new AudioStreamPlayer() {Name = "VoicePlayer"};
        AddChild(_voicePlayer);
        _voice_BusIndex = AudioServer.GetBusIndex("Voice");
        _voicePlayer.Bus = "Voice";

        _sfxPlayer = new AudioStreamPlayer() {Name = "SFXPlayer"};
        AddChild(_sfxPlayer);
        _sfx_BusIndex = AudioServer.GetBusIndex("SFX");
        _sfxPlayer.Bus = "SFX";
    }

    public async GDTask PlayBGM(AudioStream stream, float fadeDuration = -1)
    {
        if (fadeDuration <= 0) fadeDuration = 0.6f;
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        try
        {
            if (_fadeTween != null && _fadeTween.IsRunning())
            {
                _fadeTween.Stop();
                _fadeTween.Kill();
            }
            
            if (_bgmPlayer.Playing)
            {
                _fadeTween = CreateTween();
                _fadeTween.TweenProperty(_bgmPlayer, "volume_db", -80, fadeDuration).SetPureSine();

                await _fadeTween.ToSignal(this, Tween.SignalName.Finished).AsGDTask().AttachExternalCancellation(token);
            _bgmPlayer.Stop();
            }

            token.ThrowIfCancellationRequested();
            if (stream == null) return;

            _bgmPlayer.Stream = stream;
            _bgmPlayer.VolumeDb = -80f;
            _bgmPlayer.Play();

            _fadeTween = CreateTween();
            _fadeTween.TweenProperty(_bgmPlayer, "volume_db", 0, fadeDuration).SetPureSine();
            await _fadeTween.ToSignal(this, Tween.SignalName.Finished).AsGDTask().AttachExternalCancellation(token);
        }
        catch (OperationCanceledException)
        {
            if (_fadeTween != null && _fadeTween.IsRunning())
            {
                _fadeTween.Stop();
                _fadeTween.Kill();
            }
        }
    }

    public void PlayVoice(AudioStream stream)
    {
        if (stream == null) return;
        _voicePlayer.Stop();
        _voicePlayer.Stream = stream;
        _voicePlayer.Play();
    }

    public void PlaySFX(AudioStream stream)
    {
        if (stream == null) return;
        _sfxPlayer.Stream = stream;
        _sfxPlayer.Play();
    }
}
