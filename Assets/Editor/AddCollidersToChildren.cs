using UnityEngine;
using UnityEditor;

public class AddCollidersToChildren
{
    [MenuItem("Tools/Add Mesh Colliders To Selected")]
    static void AddColliders()
    {
        GameObject selected = Selection.activeGameObject;

        if (selected == null)
        {
            Debug.LogError("Nenhum objeto selecionado.");
            return;
        }

        MeshRenderer[] renderers =
            selected.GetComponentsInChildren<MeshRenderer>(true);

        int count = 0;

        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer.GetComponent<MeshCollider>() == null)
            {
                renderer.gameObject.AddComponent<MeshCollider>();
                count++;
            }
        }

        Debug.Log($"Adicionados {count} MeshColliders.");
    }
}