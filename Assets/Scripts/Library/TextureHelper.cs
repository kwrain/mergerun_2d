using System.IO;
using UnityEngine;
#if UNITY_EDITOR
#endif

public class TextureHelper
{
  public static Texture2D GetTexture2D(int width, int height)
  {
    return GetTexture2D(width, height, default, default, 0);
  }
  public static Texture2D GetTexture2D(int width, int height, Color bgColor, Color bdColor, int thickness)
  {
    var texture = new Texture2D(width, height);
    FillColor(ref texture, new Rect(0, 0, width, height), bgColor, bdColor, thickness);
    return texture;
  }
  public static Texture2D CreateTexture(byte[] data, TextureFormat format = TextureFormat.RGBA32)
  {
    var texture = new Texture2D(2, 2, format, false);
    texture.name = "Texture";
    texture.LoadImage(data); //..this will auto-resize the texture dimensions.

    return texture;
  }
  public static Texture2D LoadTexture(string path, TextureFormat format = TextureFormat.RGBA32)
  {
    Texture2D texture = null;
    byte[] fileData;

    if (File.Exists(path))
    {
      fileData = File.ReadAllBytes(path);
      texture = new Texture2D(2, 2, format, false);
      texture.name = path;
      texture.LoadImage(fileData); //..this will auto-resize the texture dimensions.
    }

    return texture;
  }
  public static bool SaveTexture(Texture2D texture2d, string path)
  {
    var directory = Path.GetDirectoryName(path);
    if (!Directory.Exists(directory))
      Directory.CreateDirectory(directory);

    if (null == texture2d)
      return false;

    using (var f = new FileStream(path, FileMode.Create, FileAccess.Write))
    using (var b = new BinaryWriter(f))
    {
      byte[] bytes = null;
      switch (Path.GetExtension(path))
      {
        case ".png":
          bytes = texture2d.EncodeToPNG();
          break;
        case ".jpg":
          bytes = texture2d.EncodeToJPG();
          break;
      }

      if (null == bytes)
        return false;

      for (var i = 0; i < bytes.Length; i++)
        b.Write(bytes[i]);
    }

    return true;
  }

  public static void FillColor(ref Texture2D image, Rect rect, Color bgColor, Color bdColor, int thickness)
  {
    var y = 0;
    while (y < rect.height)
    {
      var x = 0;
      while (x < rect.width)
      {
        var isDrawBorder = false;
        if (thickness > 0)
        {
          if (x < thickness || rect.width - x <= thickness)
            isDrawBorder = true;
          else if (y < thickness || rect.height - y <= thickness)
            isDrawBorder = true;
        }

        image.SetPixel(x + (int)rect.x, y + (int)rect.y, isDrawBorder ? bdColor : bgColor);

        ++x;
      }
      ++y;
    }
    image.Apply();
  }

  public static Texture2D ChangeColor(Texture2D texture, Color color)
  {
    var colors = texture.GetPixels();
    for (var i = 0; i < colors.Length; i++)
    {
      if (colors[i].a > 0)
        colors[i] = color;
    }

    var newTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
    newTexture.SetPixels(colors);
    newTexture.Apply();

    return newTexture;
  }

  public static Texture2D RenderTextureToTexture2D(RenderTexture rednerTexture)
  {
    var tex = new Texture2D(rednerTexture.width, rednerTexture.height, TextureFormat.RGB24, false);
    RenderTexture.active = rednerTexture;
    tex.ReadPixels(new Rect(0, 0, rednerTexture.width, rednerTexture.height), 0, 0);
    tex.Apply();

    return tex;
  }

  public static Texture2D CropTexture(Texture2D source, Rect size)
  {
    var texture = new Texture2D((int)size.width, (int)size.height, source.format, true);

    var colorCrop = source.GetPixels((int)size.x, (int)size.y, (int)size.width, (int)size.height);
    texture.SetPixels(colorCrop);
    texture.Apply();

    return texture;
  }

  public static Texture2D CombineTexture(Texture2D background, Texture2D watermark, int startX, int startY)
  {
    var newTex = new Texture2D(background.width, background.height, background.format, false);
    for (var x = 0; x < background.width; x++)
    {
      for (var y = 0; y < background.height; y++)
      {
        if (x >= startX && y >= startY && x < watermark.width && y < watermark.height)
        {
          var bgColor = background.GetPixel(x, y);
          var wmColor = watermark.GetPixel(x - startX, y - startY);

          var final_color = Color.Lerp(bgColor, wmColor, wmColor.a / 1.0f);

          newTex.SetPixel(x, y, final_color);
        }
        else
          newTex.SetPixel(x, y, background.GetPixel(x, y));
      }
    }

    newTex.Apply();
    return newTex;
  }
  public static Texture2D CombineTexture(Texture2D texture1, Texture2D texture2, Color? color = null)
  {
    var width = texture1.width > texture2.width ? texture1.width : texture2.width;
    var height = texture1.height > texture2.height ? texture1.height : texture2.height;

    var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

    for (var x = 0; x < width; x++)
    {
      for (var y = 0; y < height; y++)
      {
        var color1 = Color.clear;
        if (x < texture1.width && y < texture1.height)
          color1 = texture1.GetPixel(x, y);

        var color2 = Color.clear;
        if (x < texture2.width && y < texture2.height)
          color2 = texture2.GetPixel(x, y);

        if (color1.a > 0 || color2.a > 0)
        {
          if (color.HasValue)
            texture.SetPixel(x, y, color.Value);
          else
            texture.SetPixel(x, y, color1.a > 0 ? color1 : color2);
        }
        else
          texture.SetPixel(x, y, Color.clear);
      }
    }

    texture.Apply();

    return texture;
  }
  public static Texture2D CombineTextureIgnoreShare(Texture2D texture1, Texture2D texture2, Color? color = null)
  {
    var width = texture1.width > texture2.width ? texture1.width : texture2.width;
    var height = texture1.height > texture2.height ? texture1.height : texture2.height;

    var texture = new Texture2D(width, height, texture1.format, false);

    for (var x = 0; x < width; x++)
    {
      for (var y = 0; y < height; y++)
      {
        var color1 = Color.clear;
        if (x < texture1.width && y < texture1.height)
          color1 = texture1.GetPixel(x, y);

        var color2 = Color.clear;
        if (x < texture2.width && y < texture2.height)
          color2 = texture2.GetPixel(x, y);

        if (color1.a > 0 && color2.a > 0)
        {
          texture.SetPixel(x, y, Color.clear);
        }
        else
        {
          if (color1.a > 0 || color2.a > 0)
          {
            if (color.HasValue)
              texture.SetPixel(x, y, color.Value);
            else
              texture.SetPixel(x, y, color1.a > 0 ? color1 : color2);
          }
          else
            texture.SetPixel(x, y, Color.clear);
        }
      }
    }

    texture.Apply();

    return texture;
  }

  public static Texture2D ClippingTexture(Texture2D source, Texture2D mask, Color? color = null)
  {
    if (source.width != mask.width || source.height != mask.height)
    {
      mask = ResizeTexture(mask, source.width, source.height);
    }

    if (null == mask)
      return null;

    var texture = new Texture2D(source.width, source.height, source.format, false);

    for (var x = 0; x < source.width; x++)
    {
      for (var y = 0; y < source.height; y++)
      {
        var sourceColor = source.GetPixel(x, y);
        var maskColor = Color.clear;
        if (x < mask.width && y < mask.height)
          maskColor = mask.GetPixel(x, y);

        if (sourceColor.a == 1 && maskColor.a == 1)
        {
          texture.SetPixel(x, y, Color.clear);
        }
        else
        {
          if (sourceColor.a == 1)
          {
            if (color.HasValue)
              texture.SetPixel(x, y, color.Value);
            else
              texture.SetPixel(x, y, sourceColor);
          }
          else
            texture.SetPixel(x, y, Color.clear);
        }
      }
    }

    texture.Apply();

    return texture;
  }

  public static Texture2D RescaleTexture(Texture2D source, int targetWidth, int targetHeight, Color? color = null)
  {
    var texture = new Texture2D(targetWidth, targetHeight, source.format, false);
    var rpixels = texture.GetPixels(0);
    var incX = (1.0f / (float)targetWidth);
    var incY = (1.0f / (float)targetHeight);
    for (var px = 0; px < rpixels.Length; px++)
    {
      rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
      if (rpixels[px].a > 0 && color.HasValue)
        rpixels[px] = color.Value;
    }
    texture.SetPixels(rpixels, 0);
    texture.Apply();

    return texture;
  }
  public static Texture2D RescaleTexture(Texture2D source, Vector2 size, Color? color = null)
  {
    return RescaleTexture(source, (int)size.x, (int)size.y, color);
  }

  public static Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight, Color? color = null)
  {
    if (source.width >= targetWidth && source.height >= targetHeight)
      return null;

    var texture = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
    var overWidth = source.width == targetWidth ? 0 : (targetWidth - source.width) / 2;
    var overHeight = source.height == targetHeight ? 0 : (targetHeight - source.height) / 2;

    var color2 = Color.clear;
    for (var x = 0; x < targetWidth; x++)
    {
      for (var y = 0; y < targetHeight; y++)
      {
        if ((x < overWidth || x + overWidth >= source.width) && (y < overHeight || y + overHeight >= source.height))
        {
          color2 = Color.clear;
        }
        else
        {
          color2 = source.GetPixel(x - overWidth, y - overHeight);
          if (color2.a > 0 && color.HasValue)
            color2 = color.Value;
        }

        texture.SetPixel(x, y, color2);
      }
    }

    texture.Apply();

    return texture;
  }

  public static Texture2D FlipTexture(Texture2D original)
  {
    var flipped = new Texture2D(original.width, original.height);

    var x = original.width;
    var y = original.height;

    for (var i = 0; i < x; i++)
    {
      for (var j = 0; j < y; j++)
      {
        flipped.SetPixel(x - i - 1, j, original.GetPixel(i, j));
      }
    }

    flipped.Apply();

    return flipped;
  }

  public static Texture2D ScreenCapture(string path)
  {
    var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
    texture.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0, true);
    texture.Apply();
    var captureScreenShot = texture.EncodeToPNG();
    File.WriteAllBytes(path, captureScreenShot);

    return texture;
  }
  
  public static Texture2D ScreenCapture(string path, int width, int height)
  {
    var texture = new Texture2D(width, height, TextureFormat.RGB24, true);
    texture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, true);
    texture.Apply();
    var captureScreenShot = texture.EncodeToPNG();
    File.WriteAllBytes(path, captureScreenShot);

    return texture;
  }

  public static Texture2D ScreenCapture(Camera camera, string path, int width, int height)
  {
    var temp = RenderTexture.active; 
    
    camera.targetTexture = new RenderTexture(width, height, 24);
    camera.Render();
    RenderTexture.active = camera.targetTexture;
    
    var texture = new Texture2D(width, height);
    texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
    texture.Apply();
 
    var bytes = texture.EncodeToPNG();
    File.WriteAllBytes(path, bytes);

    RenderTexture.active = temp;
    camera.targetTexture = null;
    camera.Render();

    return texture;
  }

  private static Texture2D watermark_logo;
  private static Texture2D watermark_copyright;

  /// <summary>
  /// lds - 22.6.27 <br/>
  /// 다중 카메라 원 캡처, 그려지는 순서는 카메라의 뎁스값이 정함.
  /// </summary>
  /// <param name="cameras">스크린샷 대상 카메라들</param>
  /// <param name="path">파일이 저장될 경로</param>
  /// <param name="fileName">파일 이름</param>
  /// <param name="width">이미지 너비</param>
  /// <param name="height">이미지 높이</param>
  /// <param name="position">카메라 위치</param>
  /// <param name="orthographicSize">카메라 OrthographicSize</param>
  /// <returns></returns>
  public static Texture2D ScreenCapture(Camera[] cameras, string path, string fileName, int width, int height, Vector3? position = null, float? orthographicSize = null, bool addCopyright = true, bool addLogo = true)
  {
    var renderTextureActive = RenderTexture.active;
    var renderTexture = new RenderTexture(width, height, 24);
    RenderTexture.active = renderTexture;
    foreach(var camera in cameras)
    {
      if(camera.enabled == false) continue;
      var pos = camera.transform.position;
      var size = camera.orthographicSize;
      if(position.HasValue)
      {
        camera.transform.position = position.Value;
      }
      if(orthographicSize != null)
      {
        camera.orthographicSize = orthographicSize.Value;
      }

      camera.targetTexture = renderTexture;
      camera.Render();
      camera.targetTexture = null;

      camera.transform.position = pos;
      camera.orthographicSize = size;
    }

    var texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
    texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
    if(addCopyright)
    {
      if (watermark_copyright == null)
      {
        var copyRightTexture = ResourceManager.Instance.Load<Texture2D>("Images/watermark_copyright");
        watermark_copyright = new Texture2D(copyRightTexture.width, copyRightTexture.height, TextureFormat.ARGB32, false);
        watermark_copyright.SetPixels(copyRightTexture.GetPixels());
        watermark_copyright.Apply();
      }

      texture = AddWatermarkCopyRight2(texture, watermark_copyright);
    }

    if(addLogo)
    {
      if (watermark_logo == null)
      {
        var logoTexture = ResourceManager.Instance.Load<Texture2D>("Images/watermark_logo");
        watermark_logo = new Texture2D(logoTexture.width, logoTexture.height, TextureFormat.ARGB32, false);
        watermark_logo.SetPixels(logoTexture.GetPixels());
        watermark_logo.Apply();
      }
      texture = AddWatermarkLogo(texture, watermark_logo);
    }

    texture.Apply();

    // 파일 저장 경로에 폴더가 없으면 생성
    var di = new System.IO.DirectoryInfo(path);
    if(di.Exists == false)
      di.Create();

    var filePath = System.IO.Path.Combine(path, fileName);

    var bytes = texture.EncodeToPNG();
    File.WriteAllBytes(filePath, bytes);

    RenderTexture.active = renderTextureActive;

    return texture;
  }

  /// <summary>
  /// 중앙 하단 정렬 <br/>
  /// TODO : 급하게 적용하여서 코드 분산 되어있음. 추후에 합쳐야함
  /// </summary>
  /// <param name="background"></param>
  /// <param name="watermark"></param>
  /// <returns></returns>
  public static Texture2D AddWatermarkCopyRight(Texture2D background, Texture2D watermark)
  {
    int startX = (int)(background.width * 0.5) - (int)(watermark.width * 0.5);// - watermark.width;
    int startY = 0;

    for (int x = startX; x < startX + watermark.width; x++)
    {
      for (int y = startY; y < startY + watermark.height; y++)
      {
        Color bgColor = background.GetPixel(x, y);
        Color wmColor = watermark.GetPixel(x - startX, y - startY);

        Color final_color = Color.Lerp(bgColor, wmColor, wmColor.a / 1.0f);

        background.SetPixel(x, y, final_color);
      }
    }

    return background;
  }

  /// <summary>
  /// 좌측 하단 정렬 <br/>
  /// TODO : 급하게 적용하여서 코드 분산 되어있음. 추후에 합쳐야함
  /// </summary>
  /// <param name="background"></param>
  /// <param name="watermark"></param>
  /// <returns></returns>
  public static Texture2D AddWatermarkCopyRight2(Texture2D background, Texture2D watermark)
  {
    int startX = 0;
    int startY = 0;

    for (int x = startX; x < startX + watermark.width; x++)
    {
      for (int y = startY; y < startY + watermark.height; y++)
      {
        Color bgColor = background.GetPixel(x, y);
        Color wmColor = watermark.GetPixel(x - startX, y - startY);

        Color final_color = Color.Lerp(bgColor, wmColor, wmColor.a / 1.0f);

        background.SetPixel(x, y, final_color);
      }
    }

    return background;
  }

  public static Texture2D AddWatermarkLogo(Texture2D background, Texture2D watermark)
  {
    int startX = background.width - watermark.width;// - watermark.width;
    int startY = 0;

    for (int x = startX; x < startX + watermark.width; x++)
    {
      for (int y = startY; y < startY + watermark.height; y++)
      {
        Color bgColor = background.GetPixel(x, y);
        Color wmColor = watermark.GetPixel(x - startX, y - startY);

        Color final_color = Color.Lerp(bgColor, wmColor, wmColor.a / 1.0f);

        background.SetPixel(x, y, final_color);
      }
    }

    return background;
  }

  public static void DestroyTexture2D(ref Texture2D texture)
  {
    if (null != texture)
    {
#if UNITY_EDITOR
      UnityEngine.Object.DestroyImmediate(texture);
#else
      UnityEngine.Object.Destroy(texture);
#endif
    }

    texture = null;
  }
}