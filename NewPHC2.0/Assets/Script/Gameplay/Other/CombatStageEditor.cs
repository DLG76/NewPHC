#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CombatStage))]
public class CombatStageEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        /*SerializedProperty wavesProp = serializedObject.FindProperty("waves");
        EditorGUILayout.PropertyField(wavesProp, true);*/

        GUILayout.FlexibleSpace();

        SerializedProperty wavesProp = serializedObject.FindProperty("dungeon").FindPropertyRelative("_waves");

        if (GUILayout.Button("Add Wave"))
        {
            AddWave(wavesProp, typeof(Dungeon.Wave));
        }

        if (GUILayout.Button("Add Boss Wave"))
        {
            AddWave(wavesProp, typeof(Dungeon.BossWave));
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void AddWave(SerializedProperty wavesProp, System.Type type)
    {
        var instance = System.Activator.CreateInstance(type);
        wavesProp.InsertArrayElementAtIndex(wavesProp.arraySize);
        wavesProp.GetArrayElementAtIndex(wavesProp.arraySize - 1).managedReferenceValue = instance;
    }
}
#endif