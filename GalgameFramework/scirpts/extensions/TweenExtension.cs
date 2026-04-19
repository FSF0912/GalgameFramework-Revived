using Godot;

namespace GalgameFramework.Extensions
{
    public static class TweenExtension
    {
        public static PropertyTweener SetPureSine(this PropertyTweener t)
        {
            t.SetEase(Tween.EaseType.InOut);
            t.SetTrans(Tween.TransitionType.Sine);
            return t;
        }

        public static PropertyTweener SetPureLinear(this PropertyTweener t)
        {
            t.SetEase(Tween.EaseType.InOut);
            t.SetTrans(Tween.TransitionType.Linear);
            return t;
        }
    }
}