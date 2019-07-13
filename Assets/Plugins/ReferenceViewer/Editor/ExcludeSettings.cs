/*
unity-reference-viewer

Copyright (c) 2019 ina-amagami (ina@amagamina.jp)

This software is released under the MIT License.
https://opensource.org/licenses/mit-license.php
*/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ReferenceViewer
{
	[CreateAssetMenu(menuName = "ReferenceViewer/ExcludeSettings")]
	public class ExcludeSettings : ScriptableObject
	{
		public const string AssetName = "ExcludeSettings.asset";

		/// <summary>
		/// 除外するファイル拡張子
		/// Mac/Spotlight検索ではバイナリファイルが誤って検知されることがあるため、指定しておくと効果的
		/// ファイルサイズが大きい形式も指定しておくとMac/Grep検索時に高速化できる
		/// Excluded file extension.
		/// In Mac Spotlight, binary files may be detected incorrectly, so it is effective to specify them.
		/// If you also specify a format with a large file size, you can speed up when searching for in Mac Grep.
		/// </summary>
		[SerializeField]
		private string[] excludeExtentions =
		{
			// Scripts
			".cs",
			".js",
			// Textures
			".png",
			".tga",
			".psd",
			// Models
			".fbx",
			".obj",
			// Audios
			".wav",
			".ogg",
			".mp3",
			// Fonts
			".otf",
			".ttf",
			// Movies
			".mp4",
			".3gp",
			".usm"
		};

		/// <summary>
		/// 検索対象外ファイル（Grepのみ有効）
		/// ファイルサイズが大きいものを指定しておくと高速化できる
		/// File not to be searched (valid only for Grep).
		/// Speed up by specifying a file with a large file size.
		/// </summary>
		[SerializeField]
		public string[] excludeFiles =
		{
			"Assets/StreamingAssets/*"
		};

		public List<string> GetExcludeExtentions()
		{
			List<string> excludes = new List<string>(excludeExtentions);
			return excludes;
		}

		public string[] GetExcludeFiles()
		{
			List<string> excludes = GetExcludeExtentions();
			excludes.AddRange(excludeFiles);
			for (int i = 0; i < excludes.Count; ++i)
			{
				// ファイルパス指定の場合もAssets/より上の部分が環境依存なのでワイルドカードで解決する
				// In the case of file path designation too, since it is environment dependent since the part above Assets/, it solves with wild card.
				excludes[i] = string.Format("*{0}", excludes[i]);
			}
			return excludes.ToArray();
		}
	}

	[CustomEditor(typeof(ExcludeSettings))]
	public class ExcludeSettingsEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			var asset = target as ExcludeSettings;
			if (asset == null)
			{
				return;
			}
			EditorGUILayout.Separator();

			EditorGUILayout.HelpBox(
				"Mac/Spotlight検索ではバイナリファイルを誤って検知することがあるため、\n" +
				"拡張子指定で除外すると正確性が上がります\n\n" +
				"In Mac Spotlight, May erroneously detect binary files.\n" +
				"Excluding by extension specification improves accuracy.",
				MessageType.Info);

			EditorGUILayout.HelpBox(
				"Mac/Grep検索ではサイズの大きいファイルを除外しておくと\n" +
				"検索速度が向上します\n\n" +
				"In Mac Grep, Excluding large files will improve search speed",
				MessageType.Info);
		}
	}
}