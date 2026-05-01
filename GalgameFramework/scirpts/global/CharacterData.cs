using Godot;
using System.Collections.Generic;
using System.Linq;
using GalgameFramework.Extensions;

public partial class SingleCharacterWrapper : Resource
{
    [Export] public string CharacterName = "Character";
    [Export] public Godot.Collections.Array<SingleTextureGroupWrapper> BodyTextures = [];
    [Export] public Godot.Collections.Array<SingleTextureGroupWrapper> FaceTextures = [];
    [Export] public Godot.Collections.Array<SingleTextureGroupWrapper> DecorationTextures = [];
}

public partial class SingleTextureGroupWrapper : Resource
{
    [Export] public string GroupName;
    [Export] public Godot.Collections.Array<SingleTextureWrapper> Textures = [];
}

public partial class SingleTextureWrapper : Resource
{
    [Export] public string TextureName;
    [Export] public string TexturePath;
}

public class SingleCharacter
{
    public string CharacterName;
    //<partname, texture part>
    public List<SingleTextureGroup> BodyTextures = [];
    public List<SingleTextureGroup> FaceTextures = [];
    public List<SingleTextureGroup> DecorationTextures = [];
}

public class SingleTextureGroup
{
    public string GroupName;
    public List<SingleTexture> Textures = [];
}

public class SingleTexture
{
    public string TextureName;
    public string TexturePath;
}

[GlobalClass]
public partial class CharacterData : Resource
{
    public Godot.Collections.Array<SingleCharacterWrapper> Characters = new Godot.Collections.Array<SingleCharacterWrapper>();

    public List<SingleCharacter> GetCharacters()
    {
        var result = Characters.Select(character => new SingleCharacter
        {
            CharacterName = character.CharacterName,
            BodyTextures = character.BodyTextures.Select(g => new SingleTextureGroup
            {
                GroupName = g.GroupName,
                Textures = g.Textures.Select(t => new SingleTexture
                {
                    TextureName = t.TextureName,
                    TexturePath = t.TexturePath
                })
            })


        });

        return result.ToList();

    }
}