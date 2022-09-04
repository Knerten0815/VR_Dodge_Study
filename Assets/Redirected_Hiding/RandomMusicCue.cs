using System.Collections;
using UnityEngine;

public class RandomMusicCue : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] cueClips;
    [SerializeField] float volume = 0.5f;
    [SerializeField] float minPause = 60f, maxPause = 150;

    int clipIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(playMusicCues());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator playMusicCues()
    {
        while (true)
        {
            float wait = Random.Range(minPause, maxPause);

            yield return new WaitForSeconds(wait);

            audioSource.PlayOneShot(cueClips[clipIndex], volume);

            yield return new WaitForSeconds(cueClips[clipIndex].length);

            if (clipIndex < cueClips.Length - 1)
                clipIndex++;
            else
                clipIndex = 0;
        }
    }
}
