using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public class LevelGridBuilder : Editor
{
    void OnSceneGUI()
    {
        Event e = Event.current;

        if(e.type == EventType.MouseDown)
        {
            if(e.button == 0 || e.button == 1)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit)) 
                {
                    // Hiện menu tại vị trí chuột
                    ShowContextMenu(hit.collider.gameObject, e.button);
                    e.Use(); // Chặn event gốc của Unity
                }
            }
        }
    }

    private void ShowContextMenu(GameObject target, int mouseButton)
    {
        GenericMenu menu = new GenericMenu();

        if (mouseButton == 0) // Menu cho Setup BLOCK (Chuột trái)
        {
            // menu.AddLabel("--- SETUP BLOCK ---");
            menu.AddItem(new GUIContent("Strawberry"), false, () => ApplyChange(target, "Strawberry", false));
            menu.AddItem(new GUIContent("Flower"), false, () => ApplyChange(target, "Flower", false));
            menu.AddItem(new GUIContent("Gift Box"), false, () => ApplyChange(target, "Gift", false));
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Hidden (Question Mark)"), false, () => ApplyChange(target, "Unknown", true));
        }
        else // Menu cho Setup SLOT (Chuột phải)
        {
            // menu.AddLabel("--- SETUP SLOT ---");
            menu.AddItem(new GUIContent("Empty Slot"), false, () => ApplyChange(target, "Empty", false));
            menu.AddItem(new GUIContent("Locked Slot"), true, () => ApplyChange(target, "Locked", false));
        }
    }

    private void ApplyChange(GameObject obj, string id, bool isHidden)
    {
        Undo.RecordObject(obj, "Update Level Element");
        
        // // Giả sử bạn có một script TileController trên mỗi block
        // var tile = obj.GetComponent<TileController>();
        // if (tile != null) 
        // {
        //     tile.Setup(id, isHidden);
        //     EditorUtility.SetDirty(obj);
        //     Debug.Log($"Applied {id} to {obj.name}");
        // }
        Debug.Log("ApplyChange");
    }
}