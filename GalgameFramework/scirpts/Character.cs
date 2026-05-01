using Godot;
using GodotTask;
using System;

namespace GalgameFramework
{
    public partial class Character : Control
    {
        [Export] private TransitionTexturer _bodyTransitioner;
        [Export] private TransitionTexturer _faceTransitioner;
        [Export] private TransitionTexturer _decorationTransitioner;

        public override void _Ready()
        {
            base._Ready();
            _bodyTransitioner ??= GetNode<TransitionTexturer>("BodyTransitioner");
            _faceTransitioner ??= GetNode<TransitionTexturer>("FaceTransitioner");
            _decorationTransitioner ??= GetNode<TransitionTexturer>("DecorationTransitioner");
        }

        //tex_name format : partname:texturename
        /*public async GDTask SetBodyAsync(string bodyTexName, TransitionType type = TransitionType.CrossFade, float duration = 0.6f)
        {
            
        }*/
    }
}
