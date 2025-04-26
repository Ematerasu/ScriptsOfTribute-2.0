using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    private bool showPlayer1 = true;
    private bool showPlayer2 = true;
    private bool showTavern = true;
    private bool showPatrons = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //DrawDefaultInspector();

        GameManager manager = (GameManager)target;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("_soTGameManager"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_uiManager"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_aiManager"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_patronManager"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("HumanPlayer"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("CurrentTurn"));

        GUILayout.Space(10);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("debugMode"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("debugPlayerStartsFirst"));

        if (manager.IsDebugMode)
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("=== DEBUG SETUP ===", EditorStyles.boldLabel);

            showPlayer1 = EditorGUILayout.Foldout(showPlayer1, "Player 1 (Hand & Draw Pile)", true);
            if (showPlayer1)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("debugPlayer1Hand"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("debugPlayer1DrawPile"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("debugPlayer1Prestige"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("debugPlayer1Power"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("debugPlayer1Coins"));
                EditorGUI.indentLevel--;
            }

            showPlayer2 = EditorGUILayout.Foldout(showPlayer2, "Player 2 (Hand & Draw Pile)", true);
            if (showPlayer2)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("debugPlayer2Hand"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("debugPlayer2DrawPile"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("debugPlayer2Prestige"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("debugPlayer2Power"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("debugPlayer2Coins"));
                EditorGUI.indentLevel--;
            }

            showTavern = EditorGUILayout.Foldout(showTavern, "Tavern Available Cards", true);
            if (showTavern)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("debugTavernCards"), true);
                EditorGUI.indentLevel--;
            }

            showPatrons = EditorGUILayout.Foldout(showPatrons, "Patrons (excluding Treasury)", true);
            if (showPatrons)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("debugPatrons"), true);
                EditorGUI.indentLevel--;
            }

            GUILayout.Space(5);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
