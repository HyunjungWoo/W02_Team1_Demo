using UnityEngine;
using System.Collections;

enum RotationDir
{
    Clockwise,
    CounterClockwise,
}

public class RotationPlatform : MonoBehaviour
{
    [SerializeField] float rotationCycle = 1.0f;      // 몇 초마다 회전 시작할지
    [SerializeField] float rotationDuration = 0.5f;   // 45도 회전하는데 걸리는 시간
    [SerializeField] RotationDir rotationDir = RotationDir.Clockwise;

    bool isRotating = false;
    float stepAngle = 90f;

    //  현재 플랫폼에 꽂힌 쿠나이
    private ThrowableKunai stuckKunai;

    void Start()
    {
        StartCoroutine(RotationRoutine());
    }

    IEnumerator RotationRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(rotationCycle);
            yield return StartCoroutine(RotateByStep(stepAngle));
        }
    }

    IEnumerator RotateByStep(float stepAngle_)
    {
        if (isRotating) yield break;
        isRotating = true;

        float dir = (rotationDir == RotationDir.Clockwise) ? -1f : 1f;
        float startAngle = transform.eulerAngles.z;
        float endAngle = startAngle + stepAngle_ * dir;

        float elapsed = 0f;
        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rotationDuration);
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            //  회전 중에도 stuckKunai의 Normal 갱신
            UpdateKunaiNormal();

            yield return null;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, endAngle);
        UpdateKunaiNormal();

        isRotating = false;
    }

    //  쿠나이를 등록
    public void SetKunaiTransform(ThrowableKunai kunai, Vector3 localPos, Vector2 localNormal)
    {
        stuckKunai = kunai;
        kunai.transform.SetParent(this.transform, true);
        kunai.transform.localPosition = localPos;

        // 첫 Normal 저장
        Vector2 worldNormal = transform.TransformDirection(localNormal).normalized;
        kunai.SetHitNormal(worldNormal);
    }

    


    //  회전할 때마다 hitNormal 갱신
    private void UpdateKunaiNormal()
    {
        if (stuckKunai != null)
        {
            // 플랫폼 localNormal 기준 → worldNormal 변환
            Vector2 worldNormal = transform.TransformDirection(Vector2.up).normalized;
            stuckKunai.SetHitNormal(worldNormal);
        }
    }
}
