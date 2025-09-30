using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.DemiLib;
using DG.Tweening;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using static DG.Tweening.DOTweenSequence.DOTweenAnimationData;


namespace DG.DOTweenEditor
{
  /**
* DOTweenSequenceInspector.cs
* 작성자 : doseon@fairsutdios.kr (LEE DO SEON)
* 작성일 : 2022년 12월 21일 오후 4시 41분
*/
  [CustomEditor(typeof(DOTweenSequence))]
  public class DOTweenSequenceInspector : Editor
  {
    private SerializedProperty autoGenerate;
    private SerializedProperty autoPlay;
    private SerializedProperty autoKill;
    
    private SerializedProperty hasOnStart;
    private SerializedProperty hasOnPlay;
    private SerializedProperty hasOnUpdate;
    private SerializedProperty hasOnStepComplete;
    private SerializedProperty hasOnComplete;
    private SerializedProperty hasOnRewind;
    private SerializedProperty hasOnAppendCallback;

    [SerializeField] private VisualTreeAsset uxml;
    [SerializeField] private VisualTreeAsset animationDataUXML;

    private DeColorPalette deColorPalette;

    private DeColorPalette DeColorPalette
    {
      get
      {
        deColorPalette = new DeColorPalette();
        return deColorPalette;
      }
    }

    private void OnEnable()
    {
      autoGenerate = serializedObject.FindProperty("autoGenerate");
      autoPlay = serializedObject.FindProperty("autoPlay");
      autoKill = serializedObject.FindProperty("autoKill");
  
      hasOnStart = serializedObject.FindProperty("hasOnStart");
      hasOnPlay = serializedObject.FindProperty("hasOnPlay");
      hasOnUpdate = serializedObject.FindProperty("hasOnUpdate");
      hasOnStepComplete = serializedObject.FindProperty("hasOnStepComplete");
      hasOnComplete = serializedObject.FindProperty("hasOnComplete");
      hasOnRewind = serializedObject.FindProperty("hasOnRewind");
      hasOnAppendCallback = serializedObject.FindProperty("hasOnAppendCallback");
    }

    private void OnDisable()
    {
      StopAllPreviews();
    }

    public override VisualElement CreateInspectorGUI()
    {
      var inspector = new VisualElement();
      uxml.CloneTree(inspector);

      var info = inspector.Q<VisualElement>("Info").Q<VisualElement>("Logo");
      var goToUxmlButton = info.Q<Button>("GoToUxmlButton");
      goToUxmlButton.RegisterCallback<MouseUpEvent>(evt =>
      {
        if (!goToUxmlButton.ContainsPoint(evt.localMousePosition)) return;
        EditorGUIUtility.PingObject(uxml);
        Selection.activeObject = uxml;
      });
      
      PreviewControlsContainer(inspector);
      OptionsContainer(inspector);
      AnimationListView(inspector);
      LoopContainer(inspector);
      EventsContainer(inspector);

      return inspector;
    }

    private void PreviewControlsContainer(VisualElement inspector)
    {
      var container = inspector.Q<VisualElement>("PreviewControls");
      var playButton = container.Q<Button>("Play");
      playButton.RegisterCallback<MouseUpEvent>(OnClickPlay);

      var stopButton = container.Q<Button>("Stop");
      stopButton.RegisterCallback<MouseUpEvent>(OnClickStop);

      void OnClickPlay(MouseUpEvent evt)
      {
        if (!playButton.ContainsPoint(evt.localMousePosition)) return;
        var sequence = target as DOTweenSequence;
        if (Application.isPlaying)
        {
          sequence.Play();
        }
        else
        {
          sequence.PlayInEditor();
          DOTweenEditorPreview.Start();
          EditorApplication.playModeStateChanged += StopAllPreviews;          
        }
      }

      void OnClickStop(MouseUpEvent evt)
      {
        if (!stopButton.ContainsPoint(evt.localMousePosition)) return;

        if (Application.isPlaying)
        {
          var sequence = target as DOTweenSequence;
          sequence.Stop();
        }
        else
        {
          StopAllPreviews();
          DOTweenEditorPreview.Stop();
          InternalEditorUtility.RepaintAllViews();
          EditorApplication.playModeStateChanged -= StopAllPreviews;
        }
      }
    }

    private void StopAllPreviews()
    {
      var sequence = target as DOTweenSequence;
      if(sequence.TweenAnimationDatas == null || sequence.TweenAnimationDatas.Count == 0)
        return;
      foreach (var data in sequence.TweenAnimationDatas)
      {
        var anim = data.animation;
        if(anim == null) continue;
        anim.tween.Rewind();
        anim.tween.Kill();
        EditorUtility.SetDirty(anim); // Refresh views
      }

      sequence.StopTimer();
    }

    private void StopAllPreviews(PlayModeStateChange obj)
    {
      StopAllPreviews();
    }

    private void OptionsContainer(VisualElement inspector)
    {
      var sequence = target as DOTweenSequence;

      var container = inspector.Q<VisualElement>("Options");
      var autoGenerate = container.Q<Button>("AutoGenerate");
      autoGenerate.RegisterCallback<MouseUpEvent>(evt => OnClickButton(evt, autoGenerate, this.autoGenerate));
      Toggle(autoGenerate, this.autoGenerate);

      var autoPlay = container.Q<Button>("AutoPlay");
      autoPlay.RegisterCallback<MouseUpEvent>(evt => OnClickButton(evt, autoPlay, this.autoPlay));
      Toggle(autoPlay, this.autoPlay);

      var autoKill = container.Q<Button>("AutoKill");
      autoKill.RegisterCallback<MouseUpEvent>(evt => OnClickButton(evt, autoKill, this.autoKill));
      Toggle(autoKill, this.autoKill);

      void Toggle(Button button, SerializedProperty property)
      {
        var value = property.boolValue;
        button.style.backgroundColor = value ? (Color)DeColorPalette.bg.toggleOn : (Color)DeColorPalette.bg.toggleOff;
        button.style.color = value ? (Color)DeColorPalette.content.toggleOn : (Color)DeColorPalette.content.toggleOff;
      }

      void OnClickButton(MouseUpEvent evt, Button button, SerializedProperty property)
      {
        if (!button.ContainsPoint(evt.localMousePosition)) return;
        property.boolValue = !property.boolValue;
        serializedObject.ApplyModifiedProperties();
        Toggle(button, property);
      }
    }

    public static List<DOTweenSequence.DOTweenAnimationData> copiedList = new();

    private void AnimationListView(VisualElement inspector)
    {
      var list = inspector.Q<ListView>("Animations");
      list.BindProperty(serializedObject);
      list.makeItem = MakeItem;

      var foldout = list.Q<Foldout>();
      foldout.bindingPath = "foldoutAnimations";
      foldout.AddManipulator(new ContextualMenuManipulator(evt =>
      {
        var temp = (target as DOTweenSequence).TweenAnimationDatas;
        var pasteStatus = copiedList?.Count > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
        evt.menu.AppendAction("Copy", x => { copiedList.Clear(); copiedList.AddRange(temp); }, DropdownMenuAction.AlwaysEnabled);
        evt.menu.AppendAction("Paste", x => (target as DOTweenSequence).SetTweenAnimationDatas(copiedList.ToList()), (a) => pasteStatus);
        evt.menu.AppendAction("Add Paste", x => temp.AddRange(copiedList.ToList()), (a) => pasteStatus);
        serializedObject.ApplyModifiedProperties();
      }));

      var refreshButton = inspector.Q<VisualElement>("RefreshAnimations").Q<Button>("RefreshButton");
      refreshButton.RegisterCallback<MouseUpEvent>(evt =>
      {
        if (!refreshButton.ContainsPoint(evt.localMousePosition)) return;
        list.RefreshItems();
      });

      VisualElement MakeItem()
      {
        var element = animationDataUXML.Instantiate();
        var animation = element.Q<ObjectField>("DOTweenAnimation");
        var openPropertyEditorButton = animation.Q<Button>("OpenPropertyEditorButton");
        openPropertyEditorButton.RegisterCallback<MouseUpEvent>(evt =>
        { OnButtonOpenPropertyEditor(evt, openPropertyEditorButton, animation); });
        
        var options = element.Q<VisualElement>("Options");
        var information = element.Q<VisualElement>("Information");
        var animationType = information.Q<Label>("AnimationType");
        var duration = information.Q<Label>("Duration");
        var delay = information.Q<Label>("Delay");
        var loops = information.Q<Label>("Loops");
        var comment = information.Q<Label>("Comment");
        animation.RegisterValueChangedCallback(evt =>
        {
          openPropertyEditorButton.style.display =
          animationType.style.display = 
          comment.style.display =
          options.style.display = animation.value ? DisplayStyle.Flex : DisplayStyle.None;
          if(animation.value != null)
          {
            var animEx = animation.value as DOTweenAnimationExtended;
            animationType.text = $"AnimationType : {animEx.animationType}";
            duration.text = $"Duration : {animEx.duration}초";
            delay.text = $"Delay : {animEx.delay}초";
            loops.text = $"Loops : {animEx.loops}회";
            comment.text = $"Comment : {animEx.comment}";
          }
        });

        var addType = options.Q<EnumField>("AddType");
        var stay = options.Q<FloatField>("Stay");
        stay.RegisterValueChangedCallback(_ => { ((DOTweenSequence) target).CheckDuration(); });
        var insertTime = options.Q<FloatField>("InsertTime");
        insertTime.RegisterValueChangedCallback(_ => { ((DOTweenSequence) target).CheckDuration(); });
        
        addType.RegisterValueChangedCallback(_ => { OnChangedAddType(addType, options); });
        
        ((DOTweenSequence) target).CheckDuration();

        return element;
      }

      void OnButtonOpenPropertyEditor(IMouseEvent evt, VisualElement button,  ObjectField objectField)
      {
        if (!button.ContainsPoint(evt.localMousePosition)) return;

        var propertyEditor = Type.GetType("UnityEditor.PropertyEditor, UnityEditor");
        var openPropertyEditor = propertyEditor.GetMethod("OpenPropertyEditor"
          , BindingFlags.NonPublic | BindingFlags.Static
          , null
          , CallingConventions.Any
          , new[] { typeof(IList<UnityEngine.Object>) }
          , null);

        var targets = new List<UnityEngine.Object> {objectField.value};
        openPropertyEditor.Invoke(null, new object[] { targets });
      }
      
      void OnChangedAddType(EnumField field, VisualElement options)
      {
        ((DOTweenSequence) target).CheckDuration();
        
        var type = (AddType) Enum.Parse(typeof(AddType), field.text);
        var onAppend = options.Q<VisualElement>("OnAppend");
        var insertTime = options.Q<VisualElement>("InsertTime");
        var onInsert = options.Q<VisualElement>("OnInsert");
        onAppend.style.display = DisplayStyle.None;
        insertTime.style.display = DisplayStyle.None;
        onInsert.style.display = DisplayStyle.None;

        switch (type)
        {
          case AddType.Append:
            onAppend.style.display = DisplayStyle.Flex;
            break;

          case AddType.Insert:
            insertTime.style.display = DisplayStyle.Flex;
            onInsert.style.display = DisplayStyle.Flex;
            break;
        }
      }
    }

    private void LoopContainer(VisualElement inspector)
    {
      var container = inspector.Q<VisualElement>("Loop");
      var loopType = container.Q<EnumField>("LoopType");
      var loops = container.Q<IntegerField>("Loops");
      loops.BindProperty(serializedObject); // BindProperty에 의해 loops의 value 변경이 감지됨.
      loops.RegisterValueChangedCallback(evt => { SetLoopType(loopType, loops, evt.newValue); });
      void SetLoopType(VisualElement element, IntegerField integerField, int value)
      {
        if (value != 0 && value != 1)
        {
          element.style.display = DisplayStyle.Flex;
        }
        else
        {
          element.style.display = DisplayStyle.None;
        }

        integerField.SetValueWithoutNotify(value);
        ((DOTweenSequence) target).CheckDuration();
      }
    }

    private void EventsContainer(VisualElement inspector)
    {
      var sequence = target as DOTweenSequence;

      var container = inspector.Q<VisualElement>("EventsContainer");
      var events = inspector.Q<VisualElement>("Events");

      #region Buttons

      var buttons = container.Q<VisualElement>("Buttons_0");
      var startButton = buttons.Q<Button>("OnStart");
      var onStart = events.Q<PropertyField>("OnStart");
      startButton.RegisterCallback<MouseUpEvent>(evt =>
        OnClickButton(evt, startButton, hasOnStart, onStart));
      Toggle(startButton, hasOnStart, onStart);
      var playButton = buttons.Q<Button>("OnPlay");
      var onPlay = events.Q<PropertyField>("OnPlay");
      playButton.RegisterCallback<MouseUpEvent>(evt => OnClickButton(evt, playButton, hasOnPlay, onPlay));
      Toggle(playButton, hasOnPlay, onPlay);
      var updateButton = buttons.Q<Button>("OnUpdate");
      var onUpdate = events.Q<PropertyField>("OnUpdate");
      updateButton.RegisterCallback<MouseUpEvent>(evt =>
        OnClickButton(evt, updateButton, hasOnUpdate, onUpdate));
      Toggle(updateButton, hasOnUpdate, onUpdate);

      buttons = container.Q<VisualElement>("Buttons_1");
      var stepButton = buttons.Q<Button>("OnStep");
      var onStep = events.Q<PropertyField>("OnStep");
      stepButton.RegisterCallback<MouseUpEvent>(evt =>
        OnClickButton(evt, stepButton, hasOnStepComplete, onStep));
      Toggle(stepButton, hasOnStepComplete, onStep);
      var completeButton = buttons.Q<Button>("OnComplete");
      var onComplete = events.Q<PropertyField>("OnComplete");
      completeButton.RegisterCallback<MouseUpEvent>(evt =>
        OnClickButton(evt, completeButton, hasOnComplete, onComplete));
      Toggle(completeButton, hasOnComplete, onComplete);
      var rewindButton = buttons.Q<Button>("OnRewind");
      var onRewind = events.Q<PropertyField>("OnRewind");
      rewindButton.RegisterCallback<MouseUpEvent>(evt =>
        OnClickButton(evt, rewindButton, hasOnRewind, onRewind));
      Toggle(rewindButton, hasOnRewind, onRewind);
      var appendButton = buttons.Q<Button>("OnAppend");
      var onAppend = events.Q<PropertyField>("OnAppend");
      appendButton.RegisterCallback<MouseUpEvent>(evt =>
        OnClickButton(evt, appendButton, hasOnAppendCallback, onAppend));
      Toggle(appendButton, hasOnAppendCallback, onAppend);

      #endregion

      void Toggle(Button button, SerializedProperty property, PropertyField unityEvent)
      {
        var value = property.boolValue;
        button.style.backgroundColor = value ? (Color)DeColorPalette.bg.toggleOn : (Color)DeColorPalette.bg.toggleOff;
        button.style.color = value ? (Color)DeColorPalette.content.toggleOn : (Color)DeColorPalette.content.toggleOff;
        unityEvent.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
      }

      void OnClickButton(MouseUpEvent evt, Button button, SerializedProperty property, PropertyField unityEvent)
      {
        if (!button.ContainsPoint(evt.localMousePosition)) return;
        property.boolValue = !property.boolValue;
        serializedObject.ApplyModifiedProperties();
        Toggle(button, property, unityEvent);
      }
    }
  }
}