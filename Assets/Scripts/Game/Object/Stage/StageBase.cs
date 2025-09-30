using System.Collections.Generic;
using FAIRSTUDIOS.SODB.Utils;
using UnityEngine;

public class StageBase : MonoBehaviour
{
  [SerializeField] private Map map;

  [Header("[Settings]")]
  [SerializeField] private Transform startPosition;
}