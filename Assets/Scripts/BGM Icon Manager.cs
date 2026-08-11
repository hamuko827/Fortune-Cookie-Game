using UnityEngine;

//manages turning the background music on and off
//when the player clicks the BGM icon
public class BGMButton : MonoBehaviour
{
    //the background music AudioSource
    [Header("BGM")]
    public AudioSource bgmAudioSource;

    //the cross-out GameObject that appears
    //when the BGM is turned off
    [Header("BGM Icon")]
    public GameObject bgmCrossOut;

    //initializes the cross-out icon as hidden
    void Start()
    {
        if (bgmCrossOut != null)
        {
            bgmCrossOut.SetActive(false);
        }
    }

    //checks if the player clicks the BGM icon
    void OnMouseDown()
    {
        ToggleBGM();
    }

    //turns the BGM on or off
    void ToggleBGM()
    {
        if (bgmAudioSource == null)
            return;

        //if the BGM is currently playing,
        //turn it off and show the cross-out icon
        if (bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Pause();

            if (bgmCrossOut != null)
            {
                bgmCrossOut.SetActive(true);
            }
        }
        //if the BGM is currently paused,
        //turn it back on and hide the cross-out icon
        else
        {
            bgmAudioSource.UnPause();

            if (bgmCrossOut != null)
            {
                bgmCrossOut.SetActive(false);
            }
        }
    }
}