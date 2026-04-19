using Godot;
using GDDictionary = Godot.Collections.Dictionary;

[GlobalClass]
public partial class CharacterData : Resource
{
    [Export] public string CharacterName = "Character";
    [Export] public GDDictionary BodyTextures = new GDDictionary();
    [Export] public GDDictionary FaceTextures = new GDDictionary();
    [Export] public GDDictionary DecorationTextures = new GDDictionary();
}