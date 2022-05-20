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
		
		GUILayout.BeginHorizontal();
		// hex
		if (GUILayout.Button("Top", GUILayout.Height(25), GUILayout.Width(100)))
		{
			Vector3 posOffset = new Vector3(0f, 1f, 0f);
			GenerateHex(hex.transform.position + posOffset);
		}

		// grid unit
		if (GUILayout.Button("GU: Top", GUILayout.Height(25), GUILayout.Width(100)))
		{
			Vector3 posOffset = new Vector3(0f, 3f, 0f);
			GenerateHexGridUnit(hex.transform.position + posOffset);
		}
		GUILayout.EndHorizontal();
	
		GUILayout.BeginHorizontal();
		//hex
		if (GUILayout.Button("Top Right", GUILayout.Height(25), GUILayout.Width(100)))
		{
			Vector3 posOffset = new Vector3(Hex.hexOffsetX, 0.5f, 0f);
			GenerateHex(hex.transform.position + posOffset);
		}

		// grid unit
		if (GUILayout.Button("GU: Top Right", GUILayout.Height(25), GUILayout.Width(100)))
		{
			Vector3 posOffset = new Vector3(3f * Hex.hexOffsetX, 1.5f, 0f);
			GenerateHexGridUnit(hex.transform.position + posOffset);
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		// hex
		if (GUILayout.Button("Bottom Right", GUILayout.Height(25), GUILayout.Width(100)))
		{
			Vector3 posOffset = new Vector3(Hex.hexOffsetX, -0.5f, 0f);
			GenerateHex(hex.transform.position + posOffset);
		}

		// grid unit
		if (GUILayout.Button("GU: Bottom Right", GUILayout.Height(25), GUILayout.Width(100)))
		{
			Vector3 posOffset = new Vector3(3f * Hex.hexOffsetX, -1.5f, 0f);
			GenerateHexGridUnit(hex.transform.position + posOffset);
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		// hex
		if (GUILayout.Button("Bottom", GUILayout.Height(25), GUILayout.Width(100)))
		{
			Vector3 posOffset = new Vector3(0f, -1f, 0f);
			GenerateHex(hex.transform.position + posOffset);
		}

		// grid unit
		if (GUILayout.Button("GU: Bottom", GUILayout.Height(25), GUILayout.Width(100)))
		{
			Vector3 posOffset = new Vector3(0f, -3f, 0f);
			GenerateHexGridUnit(hex.transform.position + posOffset);
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		// hex
		if (GUILayout.Button("Bottom Left", GUILayout.Height(25), GUILayout.Width(100)))
		{
			Vector3 posOffset = new Vector3(-Hex.hexOffsetX, -0.5f, 0f);
			GenerateHex(hex.transform.position + posOffset);
		}

		// grid unit
		if (GUILayout.Button("GU: Bottom Left", GUILayout.Height(25), GUILayout.Width(100)))
		{
			Vector3 posOffset = new Vector3(-3f * Hex.hexOffsetX, -1.5f, 0f);
			GenerateHexGridUnit(hex.transform.position + posOffset);
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		// hex
		if (GUILayout.Button("Top Left", GUILayout.Height(25), GUILayout.Width(100)))
		{
			Vector3 posOffset = new Vector3(-Hex.hexOffsetX, 0.5f, 0f);
			GenerateHex(hex.transform.position + posOffset);
		}

		// grid unit
		if (GUILayout.Button("GU: Top Left", GUILayout.Height(25), GUILayout.Width(100)))
		{
			Vector3 posOffset = new Vector3(-3f * Hex.hexOffsetX, 1.5f, 0f);
			GenerateHexGridUnit(hex.transform.position + posOffset);
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		// grid unit
		if (GUILayout.Button("GU: Right", GUILayout.Height(25), GUILayout.Width(100)))
		{
			Vector3 posOffset = new Vector3(4f * Hex.hexOffsetX, 0f, 0f);
			GenerateHexGridUnit(hex.transform.position + posOffset);
			posOffset.x = 2f * Hex.hexOffsetX;
			GenerateHex(hex.transform.position + posOffset);
		}

		// grid unit
		if (GUILayout.Button("GU: Left", GUILayout.Height(25), GUILayout.Width(100)))
		{
			Vector3 posOffset = new Vector3(-4f * Hex.hexOffsetX, 0f, 0f);
			GenerateHexGridUnit(hex.transform.position + posOffset);
			posOffset.x = -2f * Hex.hexOffsetX;
			GenerateHex(hex.transform.position + posOffset);
		}
		GUILayout.EndHorizontal();
	}

	void GenerateHex(Vector3 pos)
	{
		GameObject newHex = PrefabUtility.InstantiatePrefab(Resources.Load("hex")) as GameObject;
		newHex.transform.position = pos;
		newHex.transform.SetAsLastSibling();
	}

	void GenerateHexGridUnit(Vector3 pos)
	{
		GameObject newHex = PrefabUtility.InstantiatePrefab(Resources.Load("hex grid unit")) as GameObject;
		newHex.transform.position = pos;
		newHex.transform.SetAsLastSibling();
	}
}
#endif
