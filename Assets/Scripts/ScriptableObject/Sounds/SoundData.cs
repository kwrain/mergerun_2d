using System;
using FAIRSTUDIOS.SODB.Utils;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = "Data/Sound/SoundData")]
public class SoundData : ScriptableObject
{
  [Serializable]
  public class Data
  {
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1.0f;
    [Range(-3f, 3f)]
    public float pitch = 1.0f;
    public float delay = 0f;
    public bool loop = false;
  }

  [SerializeField] private GenericDictionary<string, Data> values;

  public Data GetData(string key)
  {
    if (values == null || values.Count == 0)
      return null;

    if (values.ContainsKey(key))
    {
      return values[key];
    }
    else
    {
      return null;
    }
  }

  public AudioClip GetAudioClip(string key)
  {
    if (values == null || values.Count == 0)
      return null;

    if (values.ContainsKey(key))
    {
      return values[key].clip;
    }
    else
    {
      return null;
    }
  }
}
