using System.Collections;
using UnityEngine;

namespace Sunyunie.UniLib
{
    /// <summary>
    /// TimeScale 관리 매니저.
    /// Pause, HitStop, Bullet Time 등을 구현 예정.
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
            }

            //fixedDeltaTime의 기본값을 저장
            defaultFixedDeltaTime = Time.fixedDeltaTime;
        }

        #endregion

        #region 변수

        private Coroutine hitStopCoroutine = null; // HitStop을 위한 코루틴
        private Coroutine bulletTimeCoroutine = null; // Bullet Time을 위한 코루틴

        private float defaultFixedDeltaTime = 0.02f; // 기본 FixedDeltaTime 값

        private bool isPaused = false; // 현재 일시정지 상태인지 여부
        private bool isHitStop = false; // 현재 HitStop 상태인지 여부
        private bool isBulletTime = false; // 현재 Bullet Time 상태인지 여부

        public bool IsPaused => isPaused;
        public bool IsHitStop => isHitStop;
        public bool IsBulletTime => isBulletTime;

        public bool CanSetTimeScale => !isHitStop && !isBulletTime && !isPaused; // TimeScale을 설정할 수 있는지 여부

        #endregion

        #region 지역 함수

        private void SetTimeScale(float timeScale)
        {
            Time.timeScale = timeScale;
            Time.fixedDeltaTime = defaultFixedDeltaTime * timeScale;
        }

        private IEnumerator HitStopCoroutineProcess(float _duration = 0.1f)
        {
            isHitStop = true;
            SetTimeScale(0f); // HitStop 동안 시간 정지

            yield return new WaitForSecondsRealtime(_duration); // 실제 시간으로 대기

            SetTimeScale(1f); // HitStop 종료 후 시간 재개
            isHitStop = false;

            hitStopCoroutine = null; // 코루틴 종료
        }

        private IEnumerator BulletTimeInCoroutineProcess(float _duration = 0.5f, float _timeScale = 0.5f, bool _useEaseInOut = true)
        {
            float elapsed = 0f;
            float start = 1f;
            float target = _timeScale;

            while (elapsed < _duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _duration);
                float easedT = _useEaseInOut ? Easing.EaseInOutQuad(t) : t;
                SetTimeScale(Mathf.Lerp(start, target, easedT));
                yield return null;
            }

            SetTimeScale(target);
        }

        private IEnumerator BulletTimeOutCoroutineProcess(float _duration = 0.5f, float _startTimeScale = 0.5f, bool _useEaseInOut = true)
        {
            float elapsed = 0f;
            float target = 1f;

            while (elapsed < _duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _duration);
                float easedT = _useEaseInOut ? Easing.EaseInOutQuad(t) : t;
                SetTimeScale(Mathf.Lerp(_startTimeScale, target, easedT));
                yield return null;
            }

            SetTimeScale(target);
            isBulletTime = false;
            bulletTimeCoroutine = null;
        }

        #endregion

        #region 전역 함수

        public void Pause()
        {
            if (!CanSetTimeScale)
            {
                return; // 일시정지할 수 없는 상태라면 무시
            }

            SetTimeScale(0f); // 일시정지 상태로 전환

            isPaused = true;
        }

        public void Unpause()
        {
            if (hitStopCoroutine != null)       StopCoroutine(hitStopCoroutine);    // HitStop 중지
            if (bulletTimeCoroutine != null)    StopCoroutine(bulletTimeCoroutine); // Bullet Time 중지

            SetTimeScale(1f); // 일시정지 해제

            isPaused = false;
        }

        public void HitStop(float _duration = 0.1f)
        {
            if (!CanSetTimeScale)
            {
                return; // HitStop을 설정할 수 없는 상태라면 무시
            }

            if (hitStopCoroutine != null) StopCoroutine(hitStopCoroutine); // 기존 HitStop 코루틴 중지

            hitStopCoroutine = StartCoroutine(HitStopCoroutineProcess(_duration));
        }

        public void StartBulletTime(float duration = 0.5f, float timeScale = 0.5f, bool useEase = true)
        {
            if (!CanSetTimeScale)
            {
                return; // Bullet Time을 설정할 수 없는 상태라면 무시
            }

            if (bulletTimeCoroutine != null) StopCoroutine(bulletTimeCoroutine);

            isBulletTime = true;
            bulletTimeCoroutine = StartCoroutine(BulletTimeInCoroutineProcess(duration, timeScale, useEase));
        }

        public void EndBulletTime(float duration = 0.5f, float fromTimeScale = 0.5f, bool useEase = true)
        {
            if (isPaused || !isBulletTime)
            {
                return; // Bullet Time이 활성화되지 않았거나 일시정지 상태라면 무시
            }

            if (bulletTimeCoroutine != null) StopCoroutine(bulletTimeCoroutine);

            bulletTimeCoroutine = StartCoroutine(BulletTimeOutCoroutineProcess(duration, fromTimeScale, useEase));
        }

        #endregion
    }
}