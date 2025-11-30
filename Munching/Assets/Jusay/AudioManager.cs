using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("-------------- Audio Source------------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("-------------- Audio Clip-------------")]
    public AudioClip background;
    public AudioClip lose;
    public AudioClip stars;
    public AudioClip buttons;
    public AudioClip walk;
    public AudioClip eat;

}
