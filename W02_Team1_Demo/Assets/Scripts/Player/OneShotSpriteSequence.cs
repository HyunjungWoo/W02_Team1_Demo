using System.Collections;
using UnityEngine;

public class OneShotSpriteSequence : MonoBehaviour
{
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (!sr) sr = gameObject.AddComponent<SpriteRenderer>();
    }

    public void Play(Sprite[] frames, float interval, bool useUnscaledTime = true)
    {
        StartCoroutine(CoPlay(frames, interval, useUnscaledTime));
    }

    private IEnumerator CoPlay(Sprite[] frames, float interval, bool useUnscaledTime)
    {
        if (frames == null || frames.Length == 0) { Destroy(gameObject); yield break; }

        float t = 0f;
        int i = 0;

        while (i < frames.Length)
        {
            sr.sprite = frames[i++];
            t = 0f;
            while (t < interval)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }
        }

        Destroy(gameObject);
    }
}
