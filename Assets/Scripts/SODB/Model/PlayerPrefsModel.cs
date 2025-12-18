using System.Collections.Generic;
using FAIRSTUDIOS.SODB.Core;
using FAIRSTUDIOS.SODB.Property;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

/// <summary>
/// RTS PlayerPrefs 전용
/// </summary>
[PreferBinarySerialization]
[CreateAssetMenu(fileName = "PlayerPrefsModel", menuName = "Model/PlayerPrefsModel")]
public partial class PlayerPrefsModel : ModelBase
{
  #region SODB Properties
  [SerializeField] private PropertyPlayerPrefsString userPrefsString;
  [SerializeField] private PropertyPlayerPrefsInteger userPrefsInt;
  [SerializeField] private PropertyPlayerPrefsFloat userPrefsFloat;
  [SerializeField] private PropertyPlayerPrefsBoolean userPrefsBool;

  [SerializeField] private PropertyPlayerPrefsString devicePrefsString;
  [SerializeField] private PropertyPlayerPrefsInteger devicePrefsInt;
  [SerializeField] private PropertyPlayerPrefsFloat devicePrefsFloat;
  [SerializeField] private PropertyPlayerPrefsBoolean devicePrefsBool;

  public PropertyPlayerPrefsString UserPrefsString => userPrefsString;
  public PropertyPlayerPrefsInteger UserPrefsInt => userPrefsInt;
  public PropertyPlayerPrefsFloat UserPrefsFloat => userPrefsFloat;
  public PropertyPlayerPrefsBoolean UserPrefsBool => userPrefsBool;

  public PropertyPlayerPrefsString DevicePrefsString => devicePrefsString;
  public PropertyPlayerPrefsInteger DevicePrefsInt => devicePrefsInt;
  public PropertyPlayerPrefsFloat DevicePrefsFloat => devicePrefsFloat;
  public PropertyPlayerPrefsBoolean DevicePrefsBool => devicePrefsBool;
  #endregion

  #region Editor
#if UNITY_EDITOR
  public static PlayerPrefsModel GetPlayerPrefsModelEditor()
  {
    var guids = AssetDatabase.FindAssets("t:PlayerPrefsModel");
    if (guids.Length > 1)
    {
      Debug.LogError($"PlayerPrefsModel이 중복입니다. 확인해주세요");
      return null;
    }
    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
    return AssetDatabase.LoadAssetAtPath<PlayerPrefsModel>(path);
  }
  [MenuItem("Tools/PlayerPrefsModel/SetKeys")]
  public static void SetPlayerPrefsKeyEditor() => GetPlayerPrefsModelEditor().SetPlayerPrefsKey();
  [MenuItem("Tools/PlayerPrefsModel/DeleteAllPlayerPrefs")]
  public static void DeleteAllPlayerPrefsEditor() => PlayerPrefs.DeleteAll();
  [MenuItem("Tools/PlayerPrefsModel/DeleteAllUserPrefs")]
  public static void DeleteAllUserPrefsEditor() => GetPlayerPrefsModelEditor().DeleteAllUserPrefs();
  [MenuItem("Tools/PlayerPrefsModel/DeleteAllDevicePrefs")]
  public static void DeleteAllDevicePrefsEditor() => GetPlayerPrefsModelEditor().DeleteAllDevicePrefs();


  [Button(nameof(SetPlayerPrefsKey))]
  public void SetPlayerPrefsKey()
  {
    userPrefsString.DefaultValue.Clear();
    userPrefsInt.DefaultValue.Clear();
    userPrefsFloat.DefaultValue.Clear();
    userPrefsBool.DefaultValue.Clear();

    devicePrefsString.DefaultValue.Clear();
    devicePrefsInt.DefaultValue.Clear();
    devicePrefsFloat.DefaultValue.Clear();
    devicePrefsBool.DefaultValue.Clear();
    // UserPrefs
    for (int i = (int)PlayerPrefsKey.STR_UserPrefs + 1; i < (int)PlayerPrefsKey.INT_UserPrefs; i++) // STR_UserPrefs
      userPrefsString.DefaultValue[((PlayerPrefsKey)i).ToString()] = default;
    for (int i = (int)PlayerPrefsKey.INT_UserPrefs + 1; i < (int)PlayerPrefsKey.FLOAT_UserPrefs; i++) // INT_UserPrefs
      userPrefsInt.DefaultValue[((PlayerPrefsKey)i).ToString()] = default;
    for (int i = (int)PlayerPrefsKey.FLOAT_UserPrefs + 1; i < (int)PlayerPrefsKey.BOOL_UserPrefs; i++) // FLOAT_UserPrefs
      userPrefsFloat.DefaultValue[((PlayerPrefsKey)i).ToString()] = default;
    for (int i = (int)PlayerPrefsKey.BOOL_UserPrefs + 1; i < (int)PlayerPrefsKey.STR_DevicePrefs; i++) // BOOL_UserPrefs
      userPrefsBool.DefaultValue[((PlayerPrefsKey)i).ToString()] = default;

    // DevicePrefs
    for (int i = (int)PlayerPrefsKey.STR_DevicePrefs + 1; i < (int)PlayerPrefsKey.INT_DevicePrefs; i++) // STR_DevicePrefs
      devicePrefsString.DefaultValue[((PlayerPrefsKey)i).ToString()] = default;
    for (int i = (int)PlayerPrefsKey.INT_DevicePrefs + 1; i < (int)PlayerPrefsKey.FLOAT_DevicePrefs; i++) // INT_DevicePrefs
      devicePrefsInt.DefaultValue[((PlayerPrefsKey)i).ToString()] = default;
    for (int i = (int)PlayerPrefsKey.FLOAT_DevicePrefs + 1; i < (int)PlayerPrefsKey.BOOL_DevicePrefs; i++)  // FLOAT_DevicePrefs
      devicePrefsFloat.DefaultValue[((PlayerPrefsKey)i).ToString()] = default;
    for (int i = (int)PlayerPrefsKey.BOOL_DevicePrefs + 1; i < (int)PlayerPrefsKey.Count; i++)  // BOOL_DevicePrefs
      devicePrefsBool.DefaultValue[((PlayerPrefsKey)i).ToString()] = default;

    EditorUtility.SetDirty(this);
    AssetDatabase.SaveAssetIfDirty(this);
  }
#endif

  [Button(nameof(DeleteAllUserPrefs))]
  public void DeleteAllUserPrefs()
  {
    for (int i = (int)PlayerPrefsKey.STR_UserPrefs + 1; i < (int)PlayerPrefsKey.INT_UserPrefs; i++) // STR_UserPrefs
      PlayerPrefs.DeleteKey(((PlayerPrefsKey)i).ToString());
    for (int i = (int)PlayerPrefsKey.INT_UserPrefs + 1; i < (int)PlayerPrefsKey.FLOAT_UserPrefs; i++) // INT_UserPrefs
      PlayerPrefs.DeleteKey(((PlayerPrefsKey)i).ToString());
    for (int i = (int)PlayerPrefsKey.FLOAT_UserPrefs + 1; i < (int)PlayerPrefsKey.BOOL_UserPrefs; i++) // FLOAT_UserPrefs
      PlayerPrefs.DeleteKey(((PlayerPrefsKey)i).ToString());
    for (int i = (int)PlayerPrefsKey.BOOL_UserPrefs + 1; i < (int)PlayerPrefsKey.STR_DevicePrefs; i++) // BOOL_DevicePrefs
      PlayerPrefs.DeleteKey(((PlayerPrefsKey)i).ToString());
  }

  [Button(nameof(DeleteAllDevicePrefs))]
  public void DeleteAllDevicePrefs()
  {
    for (int i = (int)PlayerPrefsKey.STR_DevicePrefs + 1; i < (int)PlayerPrefsKey.INT_DevicePrefs; i++) // STR_DevicePrefs
      PlayerPrefs.DeleteKey(((PlayerPrefsKey)i).ToString());
    for (int i = (int)PlayerPrefsKey.INT_DevicePrefs + 1; i < (int)PlayerPrefsKey.FLOAT_DevicePrefs; i++) // INT_DevicePrefs
      PlayerPrefs.DeleteKey(((PlayerPrefsKey)i).ToString());
    for (int i = (int)PlayerPrefsKey.FLOAT_DevicePrefs + 1; i < (int)PlayerPrefsKey.BOOL_DevicePrefs; i++)  // FLOAT_DevicePrefs
      PlayerPrefs.DeleteKey(((PlayerPrefsKey)i).ToString());
    for (int i = (int)PlayerPrefsKey.BOOL_DevicePrefs + 1; i < (int)PlayerPrefsKey.Count; i++)  // BOOL_DevicePrefs
      PlayerPrefs.DeleteKey(((PlayerPrefsKey)i).ToString());
  }
  #endregion

  #region Keys
  /// <summary>
  /// 수정 전 협업간에 충돌 우려가 있으니 반드시 다른 작업자에게 확인 후 수정 요망
  /// </summary>
  public enum PlayerPrefsKey
  {
    None = -1,

    #region STR_UserPrefs
    STR_UserPrefs,
    #endregion

    #region INT_UserPrefs
    INT_UserPrefs,

    USER_SAVED_LEVEL,
    USER_SAVED_EXP,

    USER_SAVED_STAGE, // 일반모드 진행 단계
    USER_SAVED_STAGE_RETRY_COUNT, // 일반모드 진행 단계 재시도 횟수

    USER_BEST_LEVEL, // 무한모드 최고 합성 단계

    #endregion

    #region FLOAT_UserPrefs
    FLOAT_UserPrefs,
    #endregion

    #region BOOL_UserPrefs
    BOOL_UserPrefs,
    #endregion

    #region STR_DevicePrefs
    STR_DevicePrefs,
    #endregion

    #region INT_DevicePrefs
    INT_DevicePrefs,

    MAX_SAVED_LEVEL,
    #endregion

    #region FLOAT_DevicePrefs
    FLOAT_DevicePrefs,
    #endregion

    #region BOOL_DevicePrefs
    BOOL_DevicePrefs,

    VIBRATION_ENABLED,
    #endregion

    Count,
  }
  #endregion

  public int UserSavedLevel
  {
    get => userPrefsInt[nameof(PlayerPrefsKey.USER_SAVED_LEVEL)];
    set => userPrefsInt[nameof(PlayerPrefsKey.USER_SAVED_LEVEL)] = value;
  }

  public int UserSavedExp
  {
    get => userPrefsInt[nameof(PlayerPrefsKey.USER_SAVED_EXP)];
    set => userPrefsInt[nameof(PlayerPrefsKey.USER_SAVED_EXP)] = value;
  }

  public int UserSavedStage
  {
    get => userPrefsInt[nameof(PlayerPrefsKey.USER_SAVED_STAGE)];
    set => userPrefsInt[nameof(PlayerPrefsKey.USER_SAVED_STAGE)] = value;
  }

  public int UserSavedStageRetryCount
  {
    get => userPrefsInt[nameof(PlayerPrefsKey.USER_SAVED_STAGE_RETRY_COUNT)];
    set => userPrefsInt[nameof(PlayerPrefsKey.USER_SAVED_STAGE_RETRY_COUNT)] = value;
  }

  public int UserBestLevel
  {
    get => userPrefsInt[nameof(PlayerPrefsKey.USER_BEST_LEVEL)];
    set => userPrefsInt[nameof(PlayerPrefsKey.USER_BEST_LEVEL)] = value;
  }

  public int MaxSavedLevel
  {
    get => devicePrefsInt[nameof(PlayerPrefsKey.MAX_SAVED_LEVEL)];
    set => devicePrefsInt[nameof(PlayerPrefsKey.MAX_SAVED_LEVEL)] = value;
  }

  public bool VibrationEnabled
  {
    get => devicePrefsBool[nameof(PlayerPrefsKey.VIBRATION_ENABLED)];
    set => devicePrefsBool[nameof(PlayerPrefsKey.VIBRATION_ENABLED)] = value;
  }

}