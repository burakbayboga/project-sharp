using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(Hex))]
public class HexInspector : Editor
{
	Hex hex;
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		hex = target as Hex;

		if (GUILayout.Button("Top", GUILayout.Height(25), GUILayout.Width(100)))
		{
			Vector3 posOffset = new Vector3(0f, 1f, 0f);
			GenerateHex(hex.transform.position + posOffset);
		}
	
		if (GUILayout.Button("Top Right", GUILayout.Height(25), GUILayout.Width(100)))
		{
			Vector3 posOffset = new Vector3(Hex.hexOffsetX, 0.5f, 0f);
			GenerateHex(hex.transform.position + posOffset);
		}	

		if (GUILayout.Button("Bottom Right", GUILayout.Height(25), GUILayout.Width(100)))
		{
			Vector3 posOffset = new Vector3(Hex.hexOffsetX, -0.5f, 0f);
			GenerateHex(hex.transform.position + posOffset);
		}

		if (GUILayout.Button("Bottom", GUILayout.Height(25), GUILayout.Width(100)))
		{
			Vector3 posOffset = new Vector3(0f, -1f, 0f);
			GenerateHex(hex.transform.position + posOffset);
		}

		if (GUILayout.Button("Bottom Left", GUILayout.Height(25), GUILayout.Width(100)))
		{
			Vector3 posOffset = new Vector3(-Hex.hexOffsetX, -0.5f, 0f);
			GenerateHex(hex.transform.position + posOffset);
		}

		if (GUILayout.Button("Top Left", GUILayout.Height(25), GUILayout.Width(100)))
		{
			Vector3 posOffset = new Vector3(-Hex.hexOffsetX, 0.5f, 0f);
			GenerateHex(hex.transform.position + posOffset);
		}
	}

	void GenerateHex(Vector3 pos)
	{
		GameObject newHex = PrefabUtility.InstantiatePrefab(Resources.Load("Hex")) as GameObject;
		newHex.transform.position = pos;
		newHex.transform.SetAsLastSibling();
	}
}
#endif
