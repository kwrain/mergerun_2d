#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace FAIRSTUDIOS.UI
{
  /// <summary>
  /// Atlas image.
  /// </summary>
  [AddComponentMenu("K.UI/AtlasImage")]
  public class AtlasImage : Image
  {
    [SerializeField] private SpriteAtlas m_SpriteAtlas;
    [SerializeField] private string m_SpriteName;
    [SerializeField] private bool m_IsLocalizeImage;
    [SerializeField, Min(0.01f)] private float m_NativeSizeRatio = 1;
		private string _lastAtlasName = string.Empty;
		private string _lastSpriteName = string.Empty;
    private ELanguageCode lastLanguageCode = ELanguageCode.None;

    public bool IsLocalizeImage => m_IsLocalizeImage;

    private string _spriteName
    {
      get { return m_SpriteName; }
      set
      {
        if (m_SpriteName != value)
        {
          m_SpriteName = value;
          SetAllDirty();
        }
      }
    }


    /// <summary>
    /// 스프라이트 이름
    /// <para> - 아틀라스 내에 같은 이름의 Sprite가없는 경우 AtlasImage은 기본 스프라이트를 표시합니다. </para>
    /// </summary>
    public string spriteName
    {
      get { return _spriteName; }
      set
      {
        // if (IsLocalizeImage == false)
        //   _spriteName = RTSAtlas.GetValue(value, spriteAtlas.name);
        // else
        //   _spriteName = RTSAtlas.GetValueLocalize(value, spriteAtlas.name.Split("_")[0]);

        _spriteName = value;
      }
    }

    public SpriteAtlas spriteAtlas
		{
			get { return m_SpriteAtlas; }
			set
			{
				if (m_SpriteAtlas != value)
				{
					m_SpriteAtlas = value;
					SetAllDirty();
				}
			}
		}
		public bool isLocalizeImage
    {
			get { return m_IsLocalizeImage;}
			private set { m_IsLocalizeImage = value;}
    }

    protected override void Start()
    {
      base.Start();

      if(Application.isPlaying == true)
      {
        if (isLocalizeImage == false)
          return;

        SetNativeSize();

        // ResourceManager 에 리소스 등록.
        LoadSpriteInAtlas();
      }
    }

    private void LoadSpriteInAtlas()
    {
      ResourceManager.Instance.AddAtlas(spriteAtlas);
      // lds - 24.8.19, 스프라이트 로드시 null인경우 이전 스프라이트를 유지하도록 함.
      var newSprite = ResourceManager.Instance.LoadSpriteInAtlas(_lastAtlasName, _spriteName, isLocalizeImage);
      if(newSprite != null)
      {
        sprite = newSprite;
      }
    }

    protected override void OnEnable()
    {
      base.OnEnable();

      if(Application.isPlaying == true)
      {
        if(isLocalizeImage == true)
          Localize.OnChangedLanguageCode += ChangeLocalizeSpriteAtlas;
        ChangeLocalizeSpriteAtlas();
      }
    }

    protected override void OnDisable()
    {
      if (Application.isPlaying == true)
      {
        if(isLocalizeImage == true)
          Localize.OnChangedLanguageCode -= ChangeLocalizeSpriteAtlas;
      }

      base.OnDisable();
    }

    private void ChangeLocalizeSpriteAtlas()
    {
      if(Application.isPlaying == false) return;
      if(isLocalizeImage == false) return;

      if(lastLanguageCode != Localize.ELanguageCode)
      {
        var atlas = ResourceManager.Instance.GetLocalizeAtlas(_lastAtlasName, lastLanguageCode);
        if(atlas == null)
        {
          Debug.LogError($"현재 {Localize.ELanguageCode}에 해당하는 스프라이트 아틀라스가 없거나, 해당 국가에 대한 로컬라이즈를 지원하지 않습니다. ({_lastAtlasName} / {_lastSpriteName})");
          return;
        }
        spriteAtlas = atlas;
        lastLanguageCode = Localize.ELanguageCode;
        SetNativeSize();
      }
    }

    public override void SetNativeSize()
    {
      if (sprite != null)
      {
        var w = sprite.rect.width / pixelsPerUnit;
        var h = sprite.rect.height / pixelsPerUnit;
        rectTransform.anchorMax = rectTransform.anchorMin;
        rectTransform.sizeDelta = new Vector2(w, h) * m_NativeSizeRatio;
        SetAllDirty();
      }

      // base.SetNativeSize();
    }

    #if UNITY_EDITOR

    [ContextMenu("ChangeToImageComponent")]
    public void ChangeToImageComponent()
    {
      var sprite = this.sprite;
      var go = gameObject;
      DestroyImmediate(go.GetComponent<AtlasImage>());
      var image = go.AddComponent<Image>();
      image.sprite = sprite;
      image.SetNativeSize();
      EditorUtility.SetDirty(go);
    }

    #endif

    /// <summary>
    /// Sets the material dirty.
    /// </summary>
    public override void SetMaterialDirty()
    {
      // Animation에서 스프라이트 변경하는 처리.
      // Animation이나 스크립트는 "sprite"을 변경하면 스프라이트 이름에 반영됩니다.
      if (_lastSpriteName == _spriteName && sprite)
      {
        m_SpriteName = sprite.name.Replace("(Clone)", string.Empty);
      }

      if ((spriteAtlas != null && _lastAtlasName != spriteAtlas.name) || _lastSpriteName != _spriteName)
      {
        if (spriteAtlas != null)
        {
          _lastAtlasName = spriteAtlas.name;
        }

        _lastSpriteName = _spriteName;

        if (string.IsNullOrEmpty(_spriteName) == true)
        {
          sprite = null;
        }
        else if (spriteAtlas != null)
        {
#if UNITY_EDITOR
          if(Application.isPlaying)
          {
            LoadSpriteInAtlas();
          }
          else
          {
            sprite = spriteAtlas.GetSprite(_spriteName);
          }
#else
          LoadSpriteInAtlas();
#endif
        }
      }
      else if (sprite != null)
      {
        sprite = sprite;
      }

      base.SetMaterialDirty();
    }

    protected AtlasImage() : base()
    {
    }

    /// <summary>
    /// Raises the populate mesh event.
    /// </summary>
    /// <param name="toFill">To fill.</param>
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
      if (!overrideSprite)
      {
        toFill.Clear();
        return;
      }

      base.OnPopulateMesh(toFill);
    }
  }
}