using Godot;
using GodotInk;
using GodotTask;
using Ink;

namespace GalgameFramework
{
    public partial class DialogueManager : Node
    {
        public static DialogueManager Instance { get; private set; }
        [Export] public CharacterData characterData;
        [Export] public TypeWriter typeWriter;
        InkStory _currentStory;

        public override void _Ready()
        {
            base._Ready();
            Instance = this;
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            base._UnhandledInput(@event);
        }

        public async GDTask DiglogueAsync()
        {
            
        }

    }
}
