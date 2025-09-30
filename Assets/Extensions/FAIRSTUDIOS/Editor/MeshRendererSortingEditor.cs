using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

[CanEditMultipleObjects, CustomEditor(typeof(MeshRenderer))]
public class MeshRendererSortingEditor : Editor
{
  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();

    MeshRenderer renderer = target as MeshRenderer;

    var layers = SortingLayer.layers;

    EditorGUILayout.BeginHorizontal();
    EditorGUI.BeginChangeCheck();
    int newId = DrawSortingLayersPopup(renderer.sortingLayerID);
    if (EditorGUI.EndChangeCheck())
    {
      renderer.sortingLayerID = newId;
    }
    EditorGUILayout.EndHorizontal();

    EditorGUILayout.BeginHorizontal();
    EditorGUI.BeginChangeCheck();
    int order = EditorGUILayout.IntField("Order in Layer", renderer.sortingOrder);
    if (EditorGUI.EndChangeCheck())
    {
      renderer.sortingOrder = order;
    }
    EditorGUILayout.EndHorizontal();

    MeshRenderer[] meshRenderers = Array.ConvertAll(targets, _t => _t as MeshRenderer);
    foreach (MeshRenderer meshRenderer in meshRenderers)
    {
      meshRenderer.sortingLayerName = renderer.sortingLayerName;
      meshRenderer.sortingOrder = renderer.sortingOrder;
    }
  }

  int DrawSortingLayersPopup(int layerID)
  {
    var layers = SortingLayer.layers;
    var names = layers.Select(l => l.name).ToArray();
    if (!SortingLayer.IsValid(layerID))
    {
      layerID = layers[0].id;
    }
    var layerValue = SortingLayer.GetLayerValueFromID(layerID);
    var newLayerValue = EditorGUILayout.Popup("Sorting Layer", layerValue, names);
    return layers[newLayerValue].id;
  }

}