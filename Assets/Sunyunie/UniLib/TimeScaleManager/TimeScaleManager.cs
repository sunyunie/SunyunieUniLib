using System.Collections;
using UnityEngine;

namespace Sunyunie.UniLib
{
    /// <summary>
    /// 타임 스케일 매니저.
    /// 일시정지, 히트스탑, 불릿타임 등을 관리합니다.
    /// 각 기능은 DebugCommand를 통해 호출할 수 있습니다.
    /// 각 기능은 별도의 코루틴으로 처리되며, 최종 타임 스케일로 합산되어 적용됩니다.
    /// </summary>
    public class TimeScaleManager : MonoBehaviour
    {
        #region 싱글톤

        private static TimeScaleManager instance = null;

        public static TimeScaleManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject("TimeScaleManager");
                    instance = obj.AddComponent<TimeScaleManager>();
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            defaultFixedDeltaTime = Time.fixedDeltaTime;
            UpdateTimeScale(); // 초기 timeScale 설정
        }

        #endregion

        #region 변수

        private Coroutine hitStopCoroutine = null;
        private Coroutine bulletTimeCoroutine = null;

        private float defaultFixedDeltaTime = 0.02f;

        private float pauseScale = 1f;
        private float hitStopScale = 1f;
        private float bulletTimeScale = 1f;

        private bool isPaused = false;
        private bool isHitStop = false;
        private bool isBulletTime = false;

        public bool IsPaused => isPaused;
        public bool IsHitStop => isHitStop;
        public bool IsBulletTime => isBulletTime;

        #endregion

        #region 지역 함수

        private void UpdateTimeScale()
        {
            float finalScale = pauseScale * hitStopScale * bulletTimeScale;
            Time.timeScale = finalScale;
            Time.fixedDeltaTime = defaultFixedDeltaTime * finalScale;
        }

        private IEnumerator HitStopCoroutineProcess(float _duration)
        {
            isHitStop = true;
            hitStopScale = 0f;
            UpdateTimeScale();

            yield return new WaitForSecondsRealtime(_duration);

            hitStopScale = 1f;
            isHitStop = false;
            UpdateTimeScale();

            hitStopCoroutine = null;
        }

        private IEnumerator BulletTimeInCoroutineProcess(float _duration, float _targetScale, bool _useEase)
        {
            isBulletTime = true;

            float elapsed = 0f;
            float start = 1f;

            while (elapsed < _duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _duration);
                float easedT = _useEase ? Easing.EaseInOutQuad(t) : t;
                bulletTimeScale = Mathf.Lerp(start, _targetScale, easedT);
                UpdateTimeScale();
                yield return null;
            }

            bulletTimeScale = _targetScale;
            UpdateTimeScale();
        }

        private IEnumerator BulletTimeOutCoroutineProcess(float _duration, float _startScale, bool _useEase)
        {
            float elapsed = 0f;
            float target = 1f;

            while (elapsed < _duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _duration);
                float easedT = _useEase ? Easing.EaseInOutQuad(t) : t;
                bulletTimeScale = Mathf.Lerp(_startScale, target, easedT);
                UpdateTimeScale();
                yield return null;
            }

            bulletTimeScale = 1f;
            isBulletTime = false;
            UpdateTimeScale();

            bulletTimeCoroutine = null;
        }

        #endregion

        #region 전역 함수

        [DebugCommand("pause", "일시정지")]
        public void Pause()
        {
            if (isPaused) return;
            isPaused = true;
            pauseScale = 0f;
            UpdateTimeScale();
        }

        [DebugCommand("unpause", "일시정지 해제")]
        public void Unpause()
        {
            isPaused = false;
            pauseScale = 1f;
            UpdateTimeScale();
        }

        [DebugCommand("hitstop", "히트스탑")]
        public void HitStop(float duration = 0.1f)
        {
            if (hitStopCoroutine != null) StopCoroutine(hitStopCoroutine);
            hitStopCoroutine = StartCoroutine(HitStopCoroutineProcess(duration));
        }

        [DebugCommand("bulletin", "불릿타임 시작")]
        public void StartBulletTime(float duration = 0.5f, float targetScale = 0.5f, bool useEase = true)
        {
            if (bulletTimeCoroutine != null) StopCoroutine(bulletTimeCoroutine);
            bulletTimeCoroutine = StartCoroutine(BulletTimeInCoroutineProcess(duration, targetScale, useEase));
        }

        [DebugCommand("bulletout", "불릿타임 종료")]
        public void EndBulletTime(float duration = 0.5f, float fromScale = 0.5f, bool useEase = true)
        {
            if (!isBulletTime) return;
            if (bulletTimeCoroutine != null) StopCoroutine(bulletTimeCoroutine);
            bulletTimeCoroutine = StartCoroutine(BulletTimeOutCoroutineProcess(duration, fromScale, useEase));
        }

        #endregion
    }
}