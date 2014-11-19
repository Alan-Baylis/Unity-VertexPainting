﻿//
// VertexPainterEditor.cs
//
// Author(s):
//       Baptiste Dupy <baptiste.dupy@gmail.com>
//
// Copyright (c) 2014
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(VertexPainter))]
public class VertexPainterEditor : Editor
{
	#region Static Members

	private string[] s_channelLabels = { "Red", "Green", "Blue", "Alpha" };

	#endregion

	#region Private Members

	/// <summary>
	/// Editor's target painter
	/// </summary>
	private VertexPainter m_painter;

	private bool m_advancedFoldout = false;

	#endregion

	#region SceneGUI stuff

	public void OnSceneGUI()
	{
		if(m_painter == null)
			m_painter = (VertexPainter) target;

		m_painter.SelectedObject = HandleUtility.PickGameObject(Event.current.mousePosition,false);

		if(m_painter.SelectedObject == null)
			return;


		if(Event.current.isMouse)
		{
			RaycastHit hit;
			
			Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out hit);

			DrawBrush(hit.point,hit.normal);

			if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
			{
				PaintVertices(hit.point);
				Event.current.Use();
			}
		}

		HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
	}

	/// <summary>
	/// Paints the vertices around brush position according to painter's current color and size.
	/// </summary>
	/// <param name="penPoint">brushPosition.</param>
	private void PaintVertices(Vector3 brushPosition)
	{
		Mesh mesh = m_painter.SelectedObject.GetComponent<MeshFilter>().mesh;

		if(mesh.colors == null || mesh.colors.Length != mesh.vertices.Length)
		{
			Debug.Log("VertexPainter: Setup black vertices for current selected object");
			mesh.colors = new Color[mesh.vertices.Length];
		}

		Vector3 localPenPoint = m_painter.SelectedObject.transform.InverseTransformPoint(brushPosition);

		Color[] colors = mesh.colors;

		for(int i=0 ; i<mesh.vertices.Length ; i++)
		{
			Vector3 worldPos = m_painter.SelectedObject.transform.TransformPoint(mesh.vertices[i]);

			if(Vector3.Distance(worldPos,brushPosition) < m_painter.BrushRadius )
			{
				colors[i] = ApplyBrush(colors[i]);
			}
		}

		mesh.colors = colors;
	}

	/// <summary>
	/// Applies the brush channel to the color.
	/// </summary>
	/// <returns>The brush.</returns>
	/// <param name="inColor">In color.</param>
	Color ApplyBrush(Color inColor)
	{
		switch(m_painter.BrushChannel)
		{
		case ColorChannel.RED:
			inColor.r = Mathf.Clamp01(inColor.r + m_painter.BrushIntensity);
			break;
		case ColorChannel.GREEN:
			inColor.g = Mathf.Clamp01(inColor.g + m_painter.BrushIntensity);
			break;
		case ColorChannel.BLUE:
			inColor.b = Mathf.Clamp01(inColor.b + m_painter.BrushIntensity);
			break;
		case ColorChannel.ALPHA:
			inColor.a = Mathf.Clamp01(inColor.a + m_painter.BrushIntensity);
			break;
		}

		return inColor;
	}

	private void DrawBrush(Vector3 brush, Vector3 normal)
	{
		Handles.color = Color.white;
		Handles.DrawWireDisc(brush, normal, m_painter.BrushRadius);
	}

	#endregion

	#region Inspector stuff

	public override void OnInspectorGUI()
	{
		m_painter.BrushRadius = EditorGUILayout.Slider("Size",m_painter.BrushRadius,0,20);

		m_painter.BrushIntensity = EditorGUILayout.Slider("Intensity",m_painter.BrushIntensity,-1,1);

		m_painter.BrushChannel = (ColorChannel) GUILayout.Toolbar(
			(int)m_painter.BrushChannel,s_channelLabels);

		EditorGUILayout.Space();

		string previewButtonLabel;

		if(!m_painter.IsPreviewingRaw)
			previewButtonLabel = "Show Raw Painting";
		else
			previewButtonLabel = "Hide Raw Painting";

		if(GUILayout.Button(previewButtonLabel))
		{
			m_painter.IsPreviewingRaw = !m_painter.IsPreviewingRaw;
		}

		EditorGUILayout.Space();

		if(GUILayout.Button("Reset"))
		{
			m_painter.ResetMeshColors();
		}

		EditorGUILayout.Space();

		if((m_advancedFoldout = EditorGUILayout.Foldout(m_advancedFoldout,"Advanced")))
		{
			m_painter.PreviewMaterial = (Material) EditorGUILayout.ObjectField("Preview Material",m_painter.PreviewMaterial,typeof(Material),false);
		}
	}

	#endregion
}