using System.Threading;
using Godot;

namespace GalgameFramework.Extensions
{
    public static class ResExtensions
    {
        public static bool IsPathValid(this string path, bool checkRes = true)
        {
            if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("res://")) return false;
            if (checkRes && !ResourceLoader.Exists(path)) return false;
            return true;
        }

        public static bool TryLoadRes<T>(this string path, out T res) where T : Resource
        {
            if (!path.IsPathValid())
            {
                res = null;
                return false;
            }

            res = GD.Load<T>(path);
            return res != null;
        }
    }
}