using UnityEngine;

[CreateAssetMenu(fileName = "SoundDataCollection", menuName = "Data/Sound/SoundDataCollection")]
public class SoundDataCollection : ScriptableObject
{
  [SerializeField] private SoundData soundDataFX;
  [SerializeField] private SoundData soundDataBGM;

  public SoundData SoundDataFX => soundDataFX;
  public SoundData SoundDataBGM => soundDataBGM;
}
