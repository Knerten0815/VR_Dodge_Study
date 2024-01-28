using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RD_Hiding
{
    public class RandomMusicCue : MonoBehaviour
    {
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip[] cueClips;
        [SerializeField] float volume = 0.5f;
        [SerializeField] float minPause = 60f, maxPause = 150;
        [SerializeField] bool timeCuesByDistance;    // or time cues by pauses 
        [SerializeField] List<float> travelledDistancesToCueMusic;

        private RedirectionManager rdManager;
        private bool cueIsPlaying;
        private int clipIndex = 0;

        void Awake()
        {
            if (travelledDistancesToCueMusic == null || travelledDistancesToCueMusic.Count == 0)
                timeCuesByDistance = false;

            if (timeCuesByDistance)
            {
                travelledDistancesToCueMusic.Sort();
                rdManager = (RedirectionManager)FindObjectOfType(typeof(RedirectionManager));
            }
            else
            {
                StartCoroutine(timeMusicCuesByPauses());
            }
        }

        private void Update()
        {
            if (timeCuesByDistance)
            {
                if (rdManager.currPos.magnitude > travelledDistancesToCueMusic[0])
                {
                    if (cueIsPlaying)   // reached next distance before clip is over. Stop timing by distance and fallback to timing by pauses. 
                    {
                        timeCuesByDistance = false;
                        if (cueClips[clipIndex].length > minPause)
                            minPause = cueClips[clipIndex].length;

                        StartCoroutine(timeMusicCuesByPauses());
                        return;
                    }
                    else
                    {
                        StartCoroutine(timeMusicCuesByDistances());

                        travelledDistancesToCueMusic.RemoveAt(0);           // remove the first distance
                        if (travelledDistancesToCueMusic.Count == 0)        // stop checking distances if cue was played at every distance
                            timeCuesByDistance = false;
                    }
                }
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        IEnumerator timeMusicCuesByPauses()
        {
            while (true)
            {
                float wait = Random.Range(minPause, maxPause);

                yield return new WaitForSeconds(wait);

                audioSource.PlayOneShot(cueClips[clipIndex], volume);
                cueIsPlaying = true;

                yield return new WaitForSeconds(cueClips[clipIndex].length);
                cueIsPlaying = false;

                if (clipIndex < cueClips.Length - 1)
                    clipIndex++;
                else
                    clipIndex = 0;
            }
        }

        IEnumerator timeMusicCuesByDistances()
        {
            audioSource.PlayOneShot(cueClips[clipIndex], volume);
            cueIsPlaying = true;

            yield return new WaitForSeconds(cueClips[clipIndex].length);
            cueIsPlaying = false;

            if (clipIndex < cueClips.Length - 1)
                clipIndex++;
            else
                clipIndex = 0;
        }
    }
}

