using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource sfxSource;

    public AudioClip sfx_choose_number;
    public AudioClip sfx_row_clear;
    public AudioClip sfx_pair_clear;
    public AudioClip sfx_pop_2;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayChooseNumber()
    {
        PlaySFX(sfx_choose_number);
    }

    public void PlayRowClear()
    {
        PlaySFX(sfx_row_clear);
    }

    public void PlayPairClear()
    {
        PlaySFX(sfx_pair_clear);
    }
    public void PlayUIClick()
    {
        PlaySFX(sfx_pop_2);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}