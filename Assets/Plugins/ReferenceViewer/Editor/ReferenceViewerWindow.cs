/*
unity-reference-viewer

Copyright (c) 2019 ina-amagami (ina.amagami@gmail.com)

This software is released under the MIT License.
https://opensource.org/licenses/mit-license.php
*/

using UnityEngine;
using UnityEditor;

namespace ReferenceViewer
{
	public class ReferenceViewerWindow : EditorWindow
	{
		private const string HeaderLabelTitle = "【Find References In Project - Result】";

		public static void CreateWindow(Result result)
		{
			var window = GetWindow<ReferenceViewerWindow>();
			window.SetResult(result);
		}

		private Result result = null;
		private GUIStyle labelStyle = null;
		private Vector2 scrollPosition = Vector2.zero;

		public void SetResult(Result result)
		{
			this.result = result;
			scrollPosition = Vector2.zero;
			SetupGUIStyle();
			Repaint();
		}

		private void SetupGUIStyle()
		{
			labelStyle = new GUIStyle(EditorStyles.label);
			labelStyle.fixedHeight = 18f;
			labelStyle.richText = true;

			RectOffset margin = labelStyle.margin;
			margin.top = 0;
			margin.bottom = 0;
			labelStyle.margin = margin;
		}

		private void OnGUI()
		{
			if (result == null)
			{
				return;
			}
			EditorGUILayout.Separator();
			GUILayout.Label(HeaderLabelTitle);

			var iconSize = EditorGUIUtility.GetIconSize();
			EditorGUIUtility.SetIconSize(Vector2.one * 16f);

			// アセット毎の参照リスト
			// Reference list for each asset.
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			foreach (var referenceData in result.Assets)
			{
				if (referenceData.Asset == null)
				{
					// 削除済み
					// Deleted.
					continue;
				}
				DrawReferences(referenceData);
			}
			EditorGUILayout.EndScrollView();

			EditorGUIUtility.SetIconSize(iconSize);

			// Spotlightの時のみヘルプ情報
			// Help information only for Spotlight.
			if (result.Type == Result.SearchType.OSX_Spotlight)
			{
				EditorGUILayout.Separator();
				EditorGUILayout.HelpBox(
					"Assets内のインデックスが作られていない場合など、正しく検索できないことがあります。" +
					"正確に検索するにはGrep版を使用して下さい。\n\n" +
					"Spotlight is not be able to search correctly, for example, when an file index in Assets is not created. " +
					"Please use Grep version to search exactly.",
					MessageType.Info
				);
				EditorGUILayout.Separator();
			}
		}

		private void DrawReferences(AssetReferenceData data)
		{
			// 参照が無い場合はFoldoutではなくButtonで表示する
			// If there is no reference, display with Button instead of Foldout.
			if (data.References == null)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(16f);
				if (GUILayout.Button(data.ListItem, labelStyle))
				{
					Selection.activeObject = data.Asset;
					EditorGUIUtility.PingObject(Selection.activeObject);
				}
				GUILayout.EndHorizontal();
				return;
			}

			bool prevFoldout = data.IsFoldout;
			if (data.IsFoldout = EditorGUILayout.Foldout(data.IsFoldout, data.ListItem, true))
			{
				foreach (var assetData in data.References)
				{
					// 削除されたか、そもそもアセットではない
					// It was deleted or not an asset.
					if (assetData.Asset == null)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Space(32f);
						GUILayout.Label(string.Format("<i>（Missing）</i>{0}", assetData.Path), labelStyle);
						GUILayout.EndHorizontal();
						continue;
					}
					DrawAssetData(assetData);
				}
			}

			// アセットをクリックした時にProjectビューで選択状態にする
			// Select asset in project view when clicking asset.
			if (prevFoldout != data.IsFoldout)
			{
				Selection.activeObject = data.Asset;
				EditorGUIUtility.PingObject(Selection.activeObject);
			}
		}

		private void DrawAssetData(AssetData data)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(32f);

			// アセットをクリックした時にProjectビューで選択状態にする
			// Select asset in project view when clicking asset.
			if (GUILayout.Button(data.ListItem, labelStyle))
			{
				Selection.activeObject = data.Asset;
				EditorGUIUtility.PingObject(Selection.activeObject);
			}

			GUILayout.EndHorizontal();
		}
	}
}