using System.Threading;
using Godot;
using GodotInk;
using GodotTask;
using GD_Dictionary = Godot.Collections.Dictionary;
using Ink;
using System.Collections.Generic;
using Godot.Collections;
using System.Linq;
using System;

namespace GalgameFramework
{
    [GlobalClass]
    public partial class DialogueManager : Node
    {
        public static DialogueManager Instance { get; private set; }
        [Export] public Array<CharacterData> characterDatas;
        [Export] public GlobalResData resData;
        [Export] public InkStory story;
        [Export] public TypeWriter typeWriter;
        [Export] public TransitionTexturer backGround;
        InkStory _currentStory;

        GDTask _dialogueTask;
        CancellationTokenSource _dialogueCTS;

        #region godot callbacks
        public override void _Ready()
        {
            base._Ready();
            Instance = this;
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            base._UnhandledInput(@event);
            if (Input.IsActionJustPressed("gal_input"))
            {
                
            }
        }
        #endregion

        public void BindExtrenalFunctionsToStory()
        {
            if (story == null) return;

            story.BindExternalFunction("sfx", new Callable(this, nameof(Sfx)), lookaheadSafe:false);
            story.BindExternalFunction("set_bg", new Callable(this, nameof))
        }
        //callable methods
        private void Sfx(string effectName)
        {
            AudioController.Instance.PlaySFX(resData.GetSfx(effectName));
        }

        private void SetBG(string bg_name)
        {
            backGround.AnimationAsync(new List<SingleAnimationTrack>()[

                //new SingleAnimationTrack(AnimationTrackType.Value, ".modulate:a", 0)
            ]);
        }
        //

        public async GDTask DialogueAsync()
        {
            if (!_dialogueCTS.IsCancellationRequested)
            {
                _dialogueCTS.Cancel();
                _dialogueCTS.Dispose();
            }
            _dialogueCTS = new CancellationTokenSource();
            var _token = _dialogueCTS.Token;
            var dia = story.ContinueMaximally().Trim();
            //now processing tags then emit command.
            var tagsProcessed = new Godot.Collections.Dictionary<string, string>();
            var tags = story.CurrentTags;
            foreach (var t in tags)
            {
                var trimmed = t.Trim();
                var list = trimmed.Split(':', System.StringSplitOptions.TrimEntries);
                if (list.Length > 1)
                {
                    tagsProcessed.Add(list[0], list[1]);
                }
                else if (list.Length == 1)
                {
                    tagsProcessed.Add(list[0], null);
                }
                else continue;
            }
            //

            //now process tags to actual actions.
            List<GDTask> tasks = [];
            string speakerName = null;
            Color outlineColor = Colors.White;
            bool append = false;
            foreach (var kvp in tagsProcessed)
            {
                switch (kvp.Key)
                {
                    case "speaker":
                    var data = characterDatas.FirstOrDefault(t => t.CharacterID == kvp.Value);
                    if (data == null) continue;
                    speakerName = data.DisplayName;
                    outlineColor = data.DialogueOutlineColor;
                    break;

                    case "append":
                    if (!bool.TryParse(kvp.Value, out append)) { append = false; }
                    break;
                }
            }
            tasks.Add(typeWriter.TypeWorkAsync(speakerName, dia, outlineColor, append));

            try
            {
                await tasks;
            }
            catch (OperationCanceledException)
            {
                
            }
        }

    }
}
