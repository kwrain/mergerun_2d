using UnityEditor;

public class GlobalDataAccessorEditor : Editor
{
  [MenuItem("Build/Open Global Data Accessor")]
  public static void OpenInspector()
  {
    Selection.activeObject = GlobalDataAccessor.Instance;
  }
}