using Godot;
using System;

namespace GalgameFramework
{
    public partial class DialogueManager : Node
    {
        public static DialogueManager Instance { get; private set; }
        [Export] public CharacterData characterData;
        [Export] public TypeWriter typeWriter;

        public override void _Ready()
        {
            base._Ready();
            Instance = this;
        }
    }
}
