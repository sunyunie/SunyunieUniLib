using UnityEngine;

namespace Sunyunie.UniLib
{
    public class KeepRotate : MonoBehaviour
    {
        [Header("회전 속도")]
        [SerializeField] private float rotationSpeed = 10f;

        void Update()
        {
            // 단순 2D 회전
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }
    }
}
