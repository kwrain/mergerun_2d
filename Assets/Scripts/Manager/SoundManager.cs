using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Data.Common;

public enum SoundBGMTypes
{
  SOUND_BGM = 0,

  MAX
}

public enum SoundFxTypes
{
  NONE = -1,
  BTN = 0,

  SHOW_UI,
  HIDE_UI,

  SHOW_POPUP,
  HIDE_POPUP,

  BOUNCE,
  DAMAGE,
  DEFEAT,
  GOAL,
  MERGE,

  MAX
}

public class SoundManager : Singleton<SoundManager>
{
  private SoundDataCollection soundDataCollection;

  [Header("Audio Sources")]
  [SerializeField] private AudioSource bgmAudioSource;
  [SerializeField] private AudioSource sfxAudioSource;
  [SerializeField] private Dictionary<SoundFxTypes, List<AudioSource>> sfxAudioSources;
  private Queue<GameObject> destructionQueue = new();

  [Header("Audio Mixer")]
  [SerializeField] private AudioMixer mainMixer;

  public bool IsMuteMaster
  {
    get { return DevicePrefs.GetBool(EDevicePrefs.SOUND_BGM_MUTE, false); }
    set
    {
      DevicePrefs.SetBool(EDevicePrefs.SOUND_BGM_MUTE, value);
    }
  }

  public float VolumeMaster
  {
    get { return DevicePrefs.GetFloat(EDevicePrefs.SOUND_MASTER_VOLUME, 1f); }
    set
    {
      if (value < 0)
        value = 0;
      else if (value > 1)
        value = 1;

      DevicePrefs.SetFloat(EDevicePrefs.SOUND_MASTER_VOLUME, value);
      AudioListener.volume = value;
    }
  }

  public bool IsMuteBGM
  {
    get { return DevicePrefs.GetBool(EDevicePrefs.SOUND_BGM_MUTE, false); }
    set
    {
      var mixerVolume = Mathf.Log10(Mathf.Clamp(0, 0.0001f, 1f)) * 20f;
      // mainMixer.SetFloat(EDevicePrefs.SOUND_EFFECT_VOLUME.ToString(), mixerVolume);
      DevicePrefs.SetBool(EDevicePrefs.SOUND_BGM_MUTE, value);
    }
  }
  public float VolumeBGM
  {
    get { return DevicePrefs.GetFloat(EDevicePrefs.SOUND_BGM_VOLUME, 0.7f); }
    set
    {
      if (value < 0)
        value = 0;
      else if (value > 1)
        value = 1;

      var mixerVolume = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
      // mainMixer.SetFloat(EDevicePrefs.SOUND_BGM_VOLUME.ToString(), mixerVolume);
      DevicePrefs.SetFloat(EDevicePrefs.SOUND_BGM_VOLUME, value);
    }
  }

  public bool IsMuteSFX
  {
    get { return DevicePrefs.GetBool(EDevicePrefs.SOUND_EFFECT_MUTE, false); }
    set
    {
      var mixerVolume = Mathf.Log10(Mathf.Clamp(0, 0.0001f, 1f)) * 20f;
      // mainMixer.SetFloat(EDevicePrefs.SOUND_EFFECT_VOLUME.ToString(), mixerVolume);
      DevicePrefs.SetBool(EDevicePrefs.SOUND_EFFECT_MUTE, value);
    }
  }
  public float VolumeSFX
  {
    get { return DevicePrefs.GetFloat(EDevicePrefs.SOUND_EFFECT_VOLUME, 0.7f); }
    set
    {
      if (value < 0)
        value = 0;
      else if (value > 1)
        value = 1;

      var mixerVolume = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
      // mainMixer.SetFloat(EDevicePrefs.SOUND_EFFECT_VOLUME.ToString(), mixerVolume);
      DevicePrefs.SetFloat(EDevicePrefs.SOUND_EFFECT_VOLUME, value);
    }
  }


  /// <summary>
  /// BGM용 AudioSource가 없으면 생성합니다.
  /// </summary>
  private void EnsureBGMAudioSource()
  {
    if (bgmAudioSource != null)
      return;

    GameObject go = new("BGM");
    go.SetParent(Instance);
    bgmAudioSource = go.AddComponent<AudioSource>();
  }

  protected override void Start()
  {
    base.Start();

    // mainMixer = gameObject.AddComponent<AudioMixer>();

    EnsureBGMAudioSource();

    GameObject go = new("FX_OneShot");
    go.SetParent(Instance);
    sfxAudioSource = go.AddComponent<AudioSource>();

    // Initialize and load saved volumes
    VolumeMaster = DevicePrefs.GetFloat(EDevicePrefs.SOUND_MASTER_VOLUME, 0.7f);
    VolumeBGM = DevicePrefs.GetFloat(EDevicePrefs.SOUND_BGM_VOLUME, 0.7f);
    VolumeSFX = DevicePrefs.GetFloat(EDevicePrefs.SOUND_EFFECT_VOLUME, 0.7f);
  }

    protected override void OnDestroy()
    {
      base.OnDestroy();

      StopAllCoroutines();
    }

    #region BGM

    /// <summary>
    /// Plays background music with a fade-in effect.
    /// </summary>
    /// <param name="clip">The AudioClip to play.</param>
    /// <param name="fadeDuration">The duration of the fade-in effect.</param>
    public void PlayBGM(SoundBGMTypes type, float fadeDuration = 1.5f)
  {
    if (soundDataCollection == null)
    {
      if (Application.isPlaying)
      {
        soundDataCollection = SOManager.Instance.SoundDataCollection;
      }
      else
      {
#if UNITY_EDITOR
        soundDataCollection = GlobalDataAccessor.Instance.SoundDataCollection;
#endif
      }
    }

    // BGM AudioSource가 없는 경우 생성
    EnsureBGMAudioSource();

    var clip = soundDataCollection.SoundDataBGM.GetAudioClip(type.ToString());
    if (bgmAudioSource != null && bgmAudioSource.isPlaying)
    {
      StopAllCoroutines();
      StartCoroutine(FadeOutAndPlay(clip, fadeDuration));
    }
    else
    {
      StartCoroutine(FadeIn(clip, fadeDuration));
    }
  }

  private IEnumerator FadeIn(AudioClip clip, float duration)
  {
    float startVolume = 0f;
    float finalVolume = VolumeBGM;

    bgmAudioSource.clip = clip;
    bgmAudioSource.volume = startVolume;
    bgmAudioSource.loop = true;
    bgmAudioSource.Play();

    float timer = 0f;
    while (timer < duration)
    {
      timer += Time.deltaTime;
      float currentVolume = Mathf.Lerp(startVolume, finalVolume, timer / duration);
      bgmAudioSource.volume = currentVolume;
      yield return null;
    }

    bgmAudioSource.volume = finalVolume;
  }

  private IEnumerator FadeOutAndPlay(AudioClip newClip, float duration)
  {
    float startVolume = bgmAudioSource.volume;
    float timer = 0f;

    while (timer < duration)
    {
      timer += Time.deltaTime;
      bgmAudioSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
      yield return null;
    }

    bgmAudioSource.Stop();
    StartCoroutine(FadeIn(newClip, duration));
  }

  /// <summary>
  /// BGM 재생 속도(피치)를 설정합니다.
  /// </summary>
  /// <param name="speed">재생 속도 배율 (1.0f = 기본 속도)</param>
  public void SetBGMSpeed(float speed)
  {
    if (bgmAudioSource == null)
      return;

    // 너무 극단적인 값 방지
    float clamped = Mathf.Clamp(speed, 0.1f, 3f);
    bgmAudioSource.pitch = clamped;
  }
  #endregion BGM

  #region FX

  public void PlayFX(SoundFxTypes type, float delay  = 0)
  {
    if (soundDataCollection == null)
    {
      if (Application.isPlaying)
      {
        soundDataCollection = SOManager.Instance.SoundDataCollection;
      }
      else
      {
#if UNITY_EDITOR
        soundDataCollection = GlobalDataAccessor.Instance.SoundDataCollection;
#endif
      }
    }
    var data = soundDataCollection.SoundDataFX.GetData(type.ToString());
    if (data == null)
      return;

    if (data.loop || delay > 0 || data.delay > 0)
    {
      if (!sfxAudioSources.ContainsKey(type))
      {
        sfxAudioSources.Add(type, new List<AudioSource>());
      }

      GameObject go = new($"FX_{type}_{sfxAudioSources[type].Count}");
      go.SetParent(Instance);

      var audioSource = go.AddComponent<AudioSource>();
      audioSource.clip = data.clip;
      audioSource.loop = data.loop;

      sfxAudioSources[type].Add(audioSource);

      if(data.delay > 0)
      {
        delay = data.delay;
      }

      if(delay > 0)
      {
        audioSource.PlayDelayed(delay);
      }
      else
      {
        audioSource.Play();
      }

      StartCoroutine(Played(type, audioSource.gameObject, data.clip.length + delay));
    }
    else
    {
      sfxAudioSource.PlayOneShot(data.clip, data.volume);
    }

    IEnumerator Played(SoundFxTypes type, GameObject go, float length)
    {
      yield return new WaitForSeconds(length);

      // 오브젝트를 파괴하는 대신 풀로 반환
      if (go != null)
      {
        if(sfxAudioSources[type].Count > 2)
        {
          ScheduleForDestruction(go);
        }
      }
    }
  }

  public void ScheduleForDestruction(GameObject obj)
  {
    destructionQueue.Enqueue(obj);

    // 일정 수준 이상 쌓이면 즉시 처리
    if (destructionQueue.Count > 5)
    {
      ProcessDestructionQueue();
    }
  }

  public void ProcessDestructionQueue()
  {
    while (destructionQueue.Count > 0)
    {
      var obj = destructionQueue.Dequeue();
      if (obj != null)
      {
        Destroy(obj);
      }
    }
  }
  #endregion
}