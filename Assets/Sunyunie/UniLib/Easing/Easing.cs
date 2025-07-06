using UnityEngine;

namespace Sunyunie.UniLib
{
    /// <summary>
    /// 간단한 이징 함수 제공.
    /// 보통은 DoTween같은 라이브러리 권장.
    /// </summary>
    public static class Easing
    {
        public static float Linear(float t) // 선형
        {
            return t;
        }

        public static float EaseInQuad(float t) // 시작할 때 부드럽게 작동
        {
            return t * t;
        }

        public static float EaseOutQuad(float t) // 마지막에 도달할 때 부드럽게 작동
        {
            return 1f - (1f - t) * (1f - t);
        }

        public static float EaseInOutQuad(float t) // 양쪽 모두 부드럽게 작동
        {
            return t < 0.5f
                ? 2f * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }

        public static float EaseOutCubic(float t) // EaseInOutQuad와 유사하지만 더욱 부드럽게 작동
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }
    }
}
