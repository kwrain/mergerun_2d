using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.DemiEditor;
using DG.Tweening;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace DG.DOTweenEditor
{
  /**
* DOTweenSequencerInspector.cs
* 작성자 : dev@fairstudios.kr
* 작성일 : 2022년 12월 23일 오전 11시 08분
*/
  [CustomEditor(typeof(DOTweenSequencer))]
  public class DOTweenSequencerInspector : Editor
  {
    private class DragAndDropManipulator : PointerManipulator
    {
      // The Label in the window that shows the stored asset, if any.
      Label dropLabel;

      // The stored asset object, if any.
      Object droppedObject = null;

      // The path of the stored asset, or the empty string if there isn't one.
      string assetPath = string.Empty;

      public Action<Object> onDragPerform;

      public DragAndDropManipulator(VisualElement root)
      {
        // The target of the manipulator, the object to which to register all callbacks, is the drop area.
        target = root.Q<VisualElement>(className: "drop-area");
        dropLabel = root.Q<Label>(className: "drop-area__label");
      }

      protected override void RegisterCallbacksOnTarget()
      {
        // Register a callback when the user presses the pointer down.
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        // Register callbacks for various stages in the drag process.
        target.RegisterCallback<DragEnterEvent>(OnDragEnter);
        target.RegisterCallback<DragLeaveEvent>(OnDragLeave);
        target.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
        target.RegisterCallback<DragPerformEvent>(OnDragPerform);
      }

      protected override void UnregisterCallbacksFromTarget()
      {
        // Unregister all callbacks that you registered in RegisterCallbacksOnTarget().
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<DragEnterEvent>(OnDragEnter);
        target.UnregisterCallback<DragLeaveEvent>(OnDragLeave);
        target.UnregisterCallback<DragUpdatedEvent>(OnDragUpdate);
        target.UnregisterCallback<DragPerformEvent>(OnDragPerform);
      }

      // This method runs when a user presses a pointer down on the drop area.
      void OnPointerDown(PointerDownEvent _)
      {
        // Only do something if the window currently has a reference to an asset object.
        if (droppedObject != null)
        {
          // Clear existing data in DragAndDrop class.
          DragAndDrop.PrepareStartDrag();

          // Store reference to object and path to object in DragAndDrop static fields.
          DragAndDrop.objectReferences = new[] {droppedObject};
          if (assetPath != string.Empty)
          {
            DragAndDrop.paths = new[] {assetPath};
          }
          else
          {
            DragAndDrop.paths = new string[] { };
          }

          // Start a drag.
          DragAndDrop.StartDrag(string.Empty);
        }
      }

      // This method runs if a user brings the pointer over the target while a drag is in progress.
      void OnDragEnter(DragEnterEvent _)
      {
        // Get the name of the object the user is dragging.
        var draggedName = string.Empty;
        if (DragAndDrop.paths.Length > 0)
        {
          assetPath = DragAndDrop.paths[0];
          var splitPath = assetPath.Split('/');
          draggedName = splitPath[splitPath.Length - 1];
        }
        else if (DragAndDrop.objectReferences.Length > 0)
        {
          draggedName = DragAndDrop.objectReferences[0].name;
        }

        // Change the appearance of the drop area if the user drags something over the drop area and holds it
        // there.
        dropLabel.text = $"Dropping '{draggedName}'...";
        target.AddToClassList("drop-area--dropping");
      }

      // This method runs if a user makes the pointer leave the bounds of the target while a drag is in progress.
      void OnDragLeave(DragLeaveEvent _)
      {
        assetPath = string.Empty;
        droppedObject = null;
        dropLabel.text = "Drag a DOTweenSequence or GameObject here...";
        target.RemoveFromClassList("drop-area--dropping");
      }

      // This method runs every frame while a drag is in progress.
      void OnDragUpdate(DragUpdatedEvent _)
      {
        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
      }

      // This method runs when a user drops a dragged object onto the target.
      void OnDragPerform(DragPerformEvent _)
      {
        // Set droppedObject and draggedName fields to refer to dragged object.
        droppedObject = DragAndDrop.objectReferences[0];
        string draggedName;
        if (assetPath != string.Empty)
        {
          var splitPath = assetPath.Split('/');
          draggedName = splitPath[splitPath.Length - 1];
        }
        else
        {
          draggedName = droppedObject.name;
        }

        // Visually update target to indicate that it now stores an asset.
        dropLabel.text = "Drag a DOTweenSequence or GameObject here...";
        target.RemoveFromClassList("drop-area--dropping");

        onDragPerform?.Invoke(droppedObject);
        // 추후 드래그 될 필요가 없으므로, null 로 변경한다.
        droppedObject = null;
      }
    }

    [SerializeField] private VisualTreeAsset uxml;
    [SerializeField] private VisualTreeAsset sequencerElementUXML;

    private VisualElement inspector;
    private DragAndDropManipulator manipulator;
    private DOTweenSequence previewSequence;
    private DOTweenSequencer sequencer;

    private DOTweenSequencer Sequencer
    {
      get
      {
        if (sequencer == null)
        {
          sequencer = target as DOTweenSequencer;
        }

        return sequencer;
      }
    }

    private void OnDisable()
    {
      if (previewSequence != null)
      {
        StopAllPreviews(previewSequence = null);
      }
    }

    public override VisualElement CreateInspectorGUI()
    {
      inspector = new VisualElement();
      uxml.CloneTree(inspector);
      
      var sequences = inspector.Q<VisualElement>("Sequences");
      var removed = new HashSet<DOTweenSequence>();
      foreach (var sequence in Sequencer.SequencesForEditor)
      {
        if (sequence == null)
        {
          removed.Add(sequence);
        }
        else
        {
          SetDOTweenSeqeunce(sequences, sequence);
        }
      }
      Sequencer.SequencesForEditor.RemoveAll(removed.Contains);

      var info = inspector.Q<VisualElement>("Info");
      var findButton = info.Q<Button>("FindAllSequences");
      findButton.RegisterCallback<MouseUpEvent>(evt =>
      {
        if (!findButton.ContainsPoint(evt.localMousePosition)) return;
        var children = sequences.Children().ToList();
        for (int i = children.Count - 1; i >= 0; i--)
        {
          sequences.Remove(children[i]);          
        }
        Sequencer.FindAllSequences();
        
        removed = new HashSet<DOTweenSequence>();
        foreach (var sequence in Sequencer.SequencesForEditor)
        {
          if (sequence == null)
          {
            removed.Add(sequence);
          }
          else
          {
            SetDOTweenSeqeunce(sequences, sequence);
          }
        }
        Sequencer.SequencesForEditor.RemoveAll(removed.Contains);
      });
      
      manipulator = new DragAndDropManipulator(inspector) { onDragPerform = CheckSequence };

      return inspector;

      void CheckSequence(Object o)
      {
        if (o.GetType() == typeof(DOTweenSequence))
        {
          var sequence = (DOTweenSequence) o;
          AddSequence(sequence);
        }
        else if (o.GetType() == typeof(GameObject))
        {
          var go = (GameObject) o;
          var sequences = go.GetComponentsInChildren<DOTweenSequence>(true);
          foreach (var sequence in sequences)
          {
            AddSequence(sequence);
          }
        }
      }

      void AddSequence(DOTweenSequence sequence)
      {
        if (sequence == null)
        {
          Debug.Log("null");
          return;
        }

        if (string.IsNullOrEmpty(sequence.ID))
        {
          Debug.Log("id empty");
          return;
        }

        // 중복 등록 체크
        foreach (var s in Sequencer.SequencesForEditor)
        {
          if (ReferenceEquals(s, sequence))
          {
            // 중복
            Debug.Log("already exist");
            return;
          }
        }

        Sequencer.SequencesForEditor.Add(sequence);
        SetDOTweenSeqeunce(sequences, sequence);
      }
    }

    private void SetDOTweenSeqeunce(VisualElement sequences, DOTweenSequence sequence)
    {
      var e = sequences.Q<VisualElement>(sequence.ID);
      Foldout foldout;
      VisualElement container;
      if (e == null)
      {
        e = sequencerElementUXML.Instantiate();
        e.name = sequence.ID;

        foldout = e.Q<Foldout>();
        foldout.name = foldout.text = sequence.ID;
        foldout.value = false;
        container = e.Q<GroupBox>("Sequences");
        container.style.display = foldout.value ? DisplayStyle.Flex : DisplayStyle.None;
        foldout.RegisterValueChangedCallback(evt =>
        {
          container.style.display = foldout.value ? DisplayStyle.Flex : DisplayStyle.None;
        });

        PreviewControlsContainer(e, sequence);
        sequences.Add(e);
      }
      else
      {
        foldout = e.Q<Foldout>();
        container = e.Q<VisualElement>("Sequences");
        container.style.display = foldout.value ? DisplayStyle.Flex : DisplayStyle.None;
      }

      var sequenceContainer = new VisualElement();
      var sequenceObject = new ObjectField
      {
        objectType = sequence.GetType(),
        value = sequence,
        label = sequence.gameObject.name
      };
      sequenceObject.SetEnabled(false);

      var buttons = new VisualElement
      {
        style = {flexDirection = FlexDirection.Row}
      };
      var radioButton = new RadioButton
      {
        text = "Complete",
        style =
        {
          flexShrink = 1,
          unityTextAlign = TextAnchor.MiddleCenter,
          backgroundColor = "585858".HexToColor()
        }
      };
      radioButton.style.marginLeft = radioButton.style.marginRight = 3;
      radioButton.style.marginTop = radioButton.style.marginBottom = 1; 
      radioButton.style.paddingLeft = radioButton.style.paddingRight = 3;
      radioButton.style.paddingTop = radioButton.style.paddingBottom = 1; 
      radioButton.style.borderLeftColor = radioButton.style.borderRightColor = 
        radioButton.style.borderTopColor = radioButton.style.borderBottomColor = "303030".HexToColor();
      radioButton.style.borderLeftWidth = radioButton.style.borderRightWidth =
        radioButton.style.borderTopWidth = radioButton.style.borderBottomWidth = 1;
      radioButton.style.borderTopLeftRadius = radioButton.style.borderTopRightRadius =
        radioButton.style.borderBottomLeftRadius = radioButton.style.borderBottomRightRadius = 3;

      radioButton.SetValueWithoutNotify(sequence.Complete);
      radioButton.RegisterCallback<MouseUpEvent>(evt =>
      {
        if (!radioButton.ContainsPoint(evt.localMousePosition) || !sequence.Complete ) return;
        sequence.Complete = !sequence.Complete;
        radioButton.SetValueWithoutNotify(sequence.Complete);
      });
      radioButton.RegisterValueChangedCallback(evt => { sequence.Complete = evt.newValue; });
      
      var selectButton = new Button
      {
        text = "Select", 
        style = {flexGrow = 1}
      };
      selectButton.RegisterCallback<MouseUpEvent>(evt =>
      {
        if (!selectButton.ContainsPoint(evt.localMousePosition)) return;
        EditorGUIUtility.PingObject(sequence);
        Selection.activeObject = sequence;
      });
      var openButton = new Button()
      {
        text = "Open",
        style = {flexGrow = 1}
      };
      openButton.RegisterCallback<MouseUpEvent>(evt =>
      {
        if (!openButton.ContainsPoint(evt.localMousePosition)) return;
        var propertyEditor = Type.GetType("UnityEditor.PropertyEditor, UnityEditor");
        var methods = propertyEditor.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
        var openPropertyEditor = propertyEditor.GetMethod("OpenPropertyEditor"
          , BindingFlags.NonPublic | BindingFlags.Static
          , null
          , CallingConventions.Any
          , new Type[] { typeof(IList<UnityEngine.Object>) }
          , null);

        var targets = new List<Object> {sequence};
        openPropertyEditor.Invoke(null, new object[] { targets });
      });
      var removeButton = new Button()
      {
        text = "-",
      };
      removeButton.RegisterCallback<MouseUpEvent>(evt =>
      {
        if (!removeButton.ContainsPoint(evt.localMousePosition)) return;
        if (EditorUtility.DisplayDialog("ㅈㄴ 위험함", "지울꺼임?", "ㅇㅇ", "ㄴㄴ"))
        {
          Sequencer.SequencesForEditor.Remove(sequenceObject.value as DOTweenSequence);
          container.Remove(sequenceContainer);
          if (container.childCount == 0)
          {
            sequences.Remove(e);
          }
        }
      });
      
      sequenceContainer.Add(sequenceObject);
      sequenceContainer.Add(buttons);
      buttons.Add(radioButton);
      buttons.Add(selectButton);
      buttons.Add(openButton);
      buttons.Add(removeButton);
      container.Add(sequenceContainer);
    }

    private void PreviewControlsContainer(VisualElement e, DOTweenSequence sequence)
    {
      var container = e.Q<VisualElement>("PreviewControls");
      var playButton = container.Q<Button>("Play");
      playButton.RegisterCallback<MouseUpEvent>(evt => { OnClickPlay(evt, sequence); });

      var stopButton = container.Q<Button>("Stop");
      stopButton.RegisterCallback<MouseUpEvent>(OnClickStop);
      
      var deleteButton = container.Q<Button>("Delete");
      deleteButton.RegisterCallback<MouseUpEvent>(evt =>
      {
        if (!deleteButton.ContainsPoint(evt.localMousePosition)) return;

        if (EditorUtility.DisplayDialog("ㅈㄴ 위험함", "지울꺼임?", "ㅇㅇ", "ㄴㄴ"))
        {
          var sequences = inspector.Q<VisualElement>("Sequences");
          sequences.Remove(e);
          for (var i = Sequencer.SequencesForEditor.Count - 1; i >= 0; i--)
          {
            if(sequence.ID != Sequencer.SequencesForEditor[i].ID)
              continue;

            Sequencer.SequencesForEditor.Remove(sequence);
          }

          if (previewSequence != null)
          {
            StopAllPreviews(previewSequence);
            previewSequence = null;
          }
        }
      });

      void OnClickPlay(MouseUpEvent evt, DOTweenSequence sequence)
      {
        if (!playButton.ContainsPoint(evt.localMousePosition)) return;
        previewSequence = sequence;
        previewSequence.PlayInEditor();
        DOTweenEditorPreview.Start();
        EditorApplication.playModeStateChanged += StopAllPreviews;
      }

      void OnClickStop(MouseUpEvent evt)
      {
        if (!stopButton.ContainsPoint(evt.localMousePosition)) return;

        StopAllPreviews(previewSequence);
        previewSequence = null;
        DOTweenEditorPreview.Stop();
        InternalEditorUtility.RepaintAllViews();
        EditorApplication.playModeStateChanged -= StopAllPreviews;
      }
    }

    private void StopAllPreviews(DOTweenSequence sequence)
    {
      if (sequence == null)
        return;
      
      foreach (var data in sequence.TweenAnimationDatas)
      {
        var anim = data.animation;
        anim.tween.Rewind();
        anim.tween.Kill();
        EditorUtility.SetDirty(anim); // Refresh views
      }

      sequence.StopTimer();
    }

    private void StopAllPreviews(PlayModeStateChange obj)
    {
      foreach (var s in Sequencer.SequencesForEditor)
      {
        StopAllPreviews(s);
      }
    }
  }
}