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
            if (_bodyTransitioner == null) _bodyTransitioner = GetNode<TransitionTexturer>("BodyTransitioner");
            if (_faceTransitioner == null) _faceTransitioner = GetNode<TransitionTexturer>("FaceTransitioner");
            if (_decorationTransitioner == null) _decorationTransitioner = GetNode<TransitionTexturer>("DecorationTransitioner");
        }


    }
}
