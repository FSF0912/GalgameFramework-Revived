using Godot;
using System;
using System.Linq;

[GlobalClass]
public partial class CharacterData : Resource
{
    /*
    texture strcuture:
    res://GalgameFramework/textures/
    -[char_name]
        -[char_pose_1]
            pose1_normalcloth.png
            pose1_uniform.png
            ...
            -Expressions    #char_pose_1_expressions
                anger.png
                happy.png
                ...
    
        -[char_pose_2]
            pose2_normalcloth.png
            pose2_uniform.png
            ...
            -Expressions    #char_pose_2_expressions
                anger.png
                happy.png
    */
    [Export] public string CharacterID { get; set; } = "name";
    [Export] public string DisplayName { get; set; }= "Name";
    [Export] public Color DialogueOutlineColor { get; set; } = Colors.White;
    [Export] public Godot.Collections.Array<string> ExpressionNames { get; set; } = new () { "happy", "anger" };
    [Export(PropertyHint.FilePath)] public Godot.Collections.Dictionary<string, string> Poses { get; set; } = new ()
    {
        { "stand", "res://path/stand.png" },
        { "stand_right" ,"res://path/stand_right.png" }
    };

    public Texture2D ExpressionTexture(string poseName, string expressionName)
    {
        var value = Poses.FirstOrDefault(t => t.Value.Contains(poseName.Trim(), StringComparison.CurrentCultureIgnoreCase)).Value;
        int lastIndex = value.LastIndexOf('/');
        var path = value.Substring(0, lastIndex + 1);
        path = $"{path}Expressions/{expressionName}.png";
        return GD.Load<Texture2D>(path);
    }

    public Texture2D GetPoseTexture(string poseName)
    {
        var path = Poses.FirstOrDefault(t => t.Key == poseName).Value;
        if (string.IsNullOrWhiteSpace(path)) { path = Poses.First().Value; }
        if (string.IsNullOrWhiteSpace(path))
        {
            GD.PrintErr($"[GalgameFramework] Pose \"{poseName}\" does not exists in current pose list.");
            return null;
        }
        return GD.Load<Texture2D>(path);
    }

    public (Texture2D, Texture2D) GetPoseAndExpression(string poseName, string expressionName)
    {
        return (GetPoseTexture(poseName), ExpressionTexture(poseName, expressionName));
    }
}
