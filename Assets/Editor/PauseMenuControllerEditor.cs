using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PauseMenuController))]
public class PauseMenuControllerEditor : Editor
{
    private void OnSceneGUI()
    {
        PauseMenuController controller = (PauseMenuController)target;
        if (controller._objectsToToggle == null) return;

        SerializedObject so = new SerializedObject(controller);
        SerializedProperty listProp = so.FindProperty("_objectsToToggle");

        for (int i = 0; i < controller._objectsToToggle.Count; i++)
        {
            GameObjectPair pair = controller._objectsToToggle[i];
            if (pair == null || pair.objectToTransform == null) continue;

            Vector3 objPos = pair.objectToTransform.transform.position;

            // If targetPosition hasn't been set yet, default it to the object's current position
            SerializedProperty elementProp = listProp.GetArrayElementAtIndex(i);
            SerializedProperty targetPosProp = elementProp.FindPropertyRelative("targetPosition");
            if (targetPosProp.vector3Value == Vector3.zero)
            {
                so.Update();
                targetPosProp.vector3Value = objPos;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            Vector3 targetPos = pair.targetPosition;

            // Draw the object's current/original position as a green wire sphere
            Handles.color = Color.green;
            Handles.DrawWireDisc(objPos, Vector3.forward, 0.3f);
            Handles.Label(objPos + Vector3.up * 0.4f, $"{pair.objectToTransform.name} (start)", EditorStyles.miniLabel);

            // Draw a line from object to target
            Handles.color = Color.yellow;
            Handles.DrawDottedLine(objPos, targetPos, 4f);

            // Draw a draggable handle at the target position
            Handles.color = Color.cyan;
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(targetPos, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(controller, "Move Pause Target Position");
                so.Update();
                targetPosProp.vector3Value = newPos;
                so.ApplyModifiedProperties();
            }

            Handles.Label(targetPos + Vector3.up * 0.4f, $"{pair.objectToTransform.name} (target)", EditorStyles.boldLabel);
        }
    }
}
