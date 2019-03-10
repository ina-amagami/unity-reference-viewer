/*
unity-reference-viewer

Copyright (c) 2019 ina-amagami (ina.amagami@gmail.com)

This software is released under the MIT License.
https://opensource.org/licenses/mit-license.php
*/

using UnityEditor;

namespace ReferenceViewer
{
	/// <summary>
	/// 環境ごとに実行内容を切り分け
	/// Execution contents for target OS.
	/// </summary>
	public class ReferenceViewerMenu : EditorWindow
	{
#region 設定ファイルのロード

		public const string AssetPathFromPluginRoot = "Editor/ReferenceViewerMenu.cs";

		private static ExcludeSettings settings = null;
		private static bool LoadSettings()
		{
			// インスタンスを経由して自身のファイルパスを取得
			// Retrieve its own file path via instance.
			var window = GetWindow<ReferenceViewerMenu>();
			string path = window.GetSettingFilePath();
			window.Close();

			settings = AssetDatabase.LoadAssetAtPath<ExcludeSettings>(path);
			if (settings == null)
			{
				UnityEngine.Debug.LogError("[ReferenceViewer] Failed to load exclude setting file.");
				return false;
			}
			return true;
		}

		private string GetSettingFilePath()
		{
			var thisObject = MonoScript.FromScriptableObject(this);
			var path = AssetDatabase.GetAssetPath(thisObject);
			return path.Replace(AssetPathFromPluginRoot, ExcludeSettings.AssetName);
		}

#endregion

#if UNITY_EDITOR_OSX

#region Mac/Spotlight

		[MenuItem("Assets/Find References In Project/By Spotlight", true)]
		static bool IsEnabledBySpotlight()
		{
			if (Selection.assetGUIDs == null || Selection.assetGUIDs.Length == 0)
			{
				return false;
			}
			return true;
		}

		[MenuItem("Assets/Find References In Project", false, 25)]
		[MenuItem("Assets/Find References In Project/By Spotlight", false, 25)]
		public static void FindReferencesBySpotlight()
		{
			if (!LoadSettings())
			{
				return;
			}

			string command = "mdfind -onlyin {0} {1}";
			Result result = ReferenceViewerProcessor.FindReferencesByCommand(command, settings.GetExcludeExtentions());
			if (result != null)
			{
				result.Type = Result.SearchType.OSX_Spotlight;
				ReferenceViewerWindow.CreateWindow(result);
			}
		}

#endregion

#region Mac/Grep

		[MenuItem("Assets/Find References In Project/By Grep", true)]
		static bool IsEnabledByGrep()
		{
			if (Selection.assetGUIDs == null || Selection.assetGUIDs.Length == 0)
			{
				return false;
			}
			return true;
		}
		
		[MenuItem("Assets/Find References In Project", false, 26)]
		[MenuItem("Assets/Find References In Project/By Grep", false, 26)]
		public static void FindReferencesByGrep()
		{
			if (!LoadSettings())
			{
				return;
			}

			string command = "grep {1} -rl {0} ";
			string[] excludes = settings.GetExcludeFiles();
			for (int i = 0; i < excludes.Length; ++i)
			{
				command += string.Format("--exclude='{0}' ", excludes[i]);
			}

			Result result = ReferenceViewerProcessor.FindReferencesByCommand(command);
			if (result != null)
			{
				result.Type = Result.SearchType.OSX_Grep;
				ReferenceViewerWindow.CreateWindow(result);
			}
		}

#endregion

#endif

#if UNITY_EDITOR_WIN

#region Win/FindStr
		
		[MenuItem("Assets/Find References In Project", true)]
		static bool IsEnabledByFindStr()
		{
			if (Selection.assetGUIDs == null || Selection.assetGUIDs.Length == 0)
			{
				return false;
			}
			return true;
		}
		
		[MenuItem("Assets/Find References In Project", false, 25)]
		public static void FindReferencesByFindStr()
		{
			if (!LoadSettings())
			{
				return;
			}
			
			string command = "/S {1} {0}\\*";
			Result result = ReferenceViewerProcessor.FindReferencesByCommand(command, settings.GetExcludeExtentions());
			if (result != null)
			{
				result.Type = Result.SearchType.WIN_FindStr;
				ReferenceViewerWindow.CreateWindow(result);
			}
		}

#endregion

#endif
	}
}