using UnityEditor;
using UnityEngine;

// Editor tool: shows an enemy's live state, target and health in its Inspector during Play mode.
// Lives in an "Editor" folder, so Unity leaves it out of builds.
[CustomEditor(typeof(Enemy))]
public class EnemyInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (!Application.isPlaying) return;

        var enemy = (Enemy)target;
        string stateName = enemy.enemyStateMachine?.currentState?.GetType().Name ?? "None";

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Live (Play mode)", EditorStyles.boldLabel);

        // Read-only fields: greyed out so they can't be edited.
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.TextField("Current State", stateName);
            EditorGUILayout.ObjectField("Target", enemy.target, typeof(Transform), true);
            if (enemy.health != null)
                EditorGUILayout.IntField("Health", enemy.health.currentHealth);
        }

        // Redraw every frame so the values stay current.
        Repaint();
    }
}
