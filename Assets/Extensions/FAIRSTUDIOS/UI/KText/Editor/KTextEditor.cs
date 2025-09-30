using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects, CustomEditor(typeof(KText)), InitializeOnLoad]
public class KTextEditor : Editor
{
  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();


  }
}
