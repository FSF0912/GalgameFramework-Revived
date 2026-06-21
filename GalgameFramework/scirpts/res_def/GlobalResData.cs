using System.Linq;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class GlobalResData : Resource
{
    [Export(PropertyHint.TypeString, "Background Values")]
    public Dictionary<string, Texture2D> BackGrounds { get; set; }= [];
    [Export]
    public Dictionary<string, AudioStream> BGMs { get; set; } = [];
    public Dictionary<string, AudioStream> Sfxs { get; set; } = [];

    public Texture2D GetBackground(string key)
    {
        return BackGrounds.FirstOrDefault(t => t.Key == key).Value;
    }

    public AudioStream GetBGM(string key)
    {
        return BGMs.FirstOrDefault(t => t.Key == key).Value;
    }

    public AudioStream GetSfx(string key)
    {
        return Sfxs.FirstOrDefault(t => t.Key == key).Value;
    }
}