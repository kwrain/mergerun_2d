using UnityEngine;
using UnityEngine.U2D;

#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAtlasSelector : MonoBehaviour
{
  [Header("Sprite Atlas Settings")]
  [Tooltip("사용할 스프라이트 아틀라스를 여기에 할당해주세요.")]
  [SerializeField] public SpriteAtlas spriteAtlas;

  [HideInInspector]
  [SerializeField] public string spriteName;

  private SpriteRenderer spriteRenderer;

  private void OnValidate()
  {
    if (spriteRenderer == null)
    {
      spriteRenderer = GetComponent<SpriteRenderer>();
    }

    if (spriteAtlas == null)
    {
      spriteRenderer.sprite = null;
      return;
    }

    Sprite loadedSprite = spriteAtlas.GetSprite(spriteName);
    spriteRenderer.sprite = loadedSprite;
  }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SpriteAtlasSelector))]
public class SpriteAtlasSelectorEditor : Editor
{
  // 변수를 직접 다루는 대신 SerializedProperty를 사용합니다.
  private SerializedProperty spriteAtlasProperty;
  private SerializedProperty spriteNameProperty;

  // 에디터가 활성화될 때 프로퍼티를 찾아옵니다.
  private void OnEnable()
  {
    spriteAtlasProperty = serializedObject.FindProperty("spriteAtlas");
    spriteNameProperty = serializedObject.FindProperty("spriteName");
  }

  public override void OnInspectorGUI()
  {
    // 변경사항을 기록하기 위해 객체를 업데이트합니다.
    serializedObject.Update();

    // 아틀라스 필드를 그립니다.
    EditorGUILayout.PropertyField(spriteAtlasProperty);

    // SerializedProperty에서 실제 SpriteAtlas 객체를 가져옵니다.
    SpriteAtlas currentAtlas = spriteAtlasProperty.objectReferenceValue as SpriteAtlas;

    if (currentAtlas != null)
    {
      Sprite[] sprites = new Sprite[currentAtlas.spriteCount];
      currentAtlas.GetSprites(sprites);

      string[] spriteNames = sprites.Select(s => s.name.Replace("(Clone)", "")).ToArray();

      // 현재 선택된 스프라이트 이름의 인덱스를 찾습니다.
      int currentIndex = System.Array.IndexOf(spriteNames, spriteNameProperty.stringValue);
      if (currentIndex < 0) currentIndex = 0;

      // 드롭다운 메뉴를 그리고, 변경이 생기면 selectedIndex에 새로운 인덱스가 저장됩니다.
      int selectedIndex = EditorGUILayout.Popup("Sprite Name", currentIndex, spriteNames);

      // 인덱스가 바뀌었다면, 프로퍼티 값을 새로운 이름으로 업데이트합니다.
      if (selectedIndex < spriteNames.Length)
      {
        spriteNameProperty.stringValue = spriteNames[selectedIndex];
      }
    }
    else
    {
      // 아틀라스가 없으면 이름도 초기화
      spriteNameProperty.stringValue = "";
    }

    // 모든 변경 사항을 적용합니다. 이 시점에 OnValidate()가 호출됩니다.
    serializedObject.ApplyModifiedProperties();
  }
}
#endif