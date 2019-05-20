using UnityEngine;

namespace utils
{
    public static class Util
    {
        public static void Roundvec2toint(ref Vector2 i,out Vector2 o)
        {
            o = new Vector2(Mathf.FloorToInt(i.x), Mathf.FloorToInt(i.y));
        }
    }
}
