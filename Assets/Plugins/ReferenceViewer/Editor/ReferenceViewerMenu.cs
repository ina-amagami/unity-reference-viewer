/*
unity-reference-viewer

Copyright (c) 2019 ina-amagami (ina@amagamina.jp)

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

			Result result = ReferenceViewerProcessor.FindReferencesByCommand(Result.SearchType.OSX_Spotlight, settings.GetExcludeExtentions());
			if (result != null)
			{
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

			Result result = ReferenceViewerProcessor.FindReferencesByCommand(Result.SearchType.OSX_Grep, settings.GetExcludeExtentions());
			if (result != null)
			{
				ReferenceViewerWindow.CreateWindow(result);
			}
		}

#endregion

#region Mac/GitGrep

		[MenuItem("Assets/Find References In Project/By GitGrep", true)]
		static bool IsEnabledByGitGrep()
		{
			if (Selection.assetGUIDs == null || Selection.assetGUIDs.Length == 0)
			{
				return false;
			}
			return true;
		}

		[MenuItem("Assets/Find References In Project", false, 27)]
		[MenuItem("Assets/Find References In Project/By GitGrep", false, 27)]
		public static void FindReferencesByGitGrep()
		{
			if (!LoadSettings())
			{
				return;
			}

			Result result = ReferenceViewerProcessor.FindReferencesByCommand(Result.SearchType.OSX_GitGrep, settings.GetExcludeExtentions());
			if (result != null)
			{
				ReferenceViewerWindow.CreateWindow(result);
			}
		}

#endregion

#endif

#if UNITY_EDITOR_WIN

#region Win/FindStr

		[MenuItem("Assets/Find References In Project/By FindStr", true)]
		static bool IsEnabledByFindStr()
		{
			if (Selection.assetGUIDs == null || Selection.assetGUIDs.Length == 0)
			{
				return false;
			}
			return true;
		}

		[MenuItem("Assets/Find References In Project", false, 25)]
		[MenuItem("Assets/Find References In Project/By FindStr", false, 25)]
		public static void FindReferencesByFindStr()
		{
			if (!LoadSettings())
			{
				return;
			}

			Result result = ReferenceViewerProcessor.FindReferencesByCommand(Result.SearchType.WIN_FindStr, settings.GetExcludeExtentions());
			if (result != null)
			{
				ReferenceViewerWindow.CreateWindow(result);
			}
		}

#endregion

#region Win/GitGrep
		
		[MenuItem("Assets/Find References In Project/By GitGrep", true)]
		static bool IsEnabledByGitGrep()
		{
			if (Selection.assetGUIDs == null || Selection.assetGUIDs.Length == 0)
			{
				return false;
			}

			string pathEnv = System.Environment.GetEnvironmentVariable("Path", System.EnvironmentVariableTarget.Process);
			if (pathEnv == null || pathEnv.Trim() == "")
			{
				return false;
			}
			string[] paths = pathEnv.Split(';');
			foreach (string path in paths)
			{
				if (System.IO.File.Exists(System.IO.Path.Combine(path, Result.SearchType.WIN_GitGrep.Command().Command)))
				{
					return true;
				}
			}
			return false;
		}
		
		[MenuItem("Assets/Find References In Project", false, 27)]
		[MenuItem("Assets/Find References In Project/By GitGrep", false, 27)]
		public static void FindReferencesByGitGrep()
		{
			if (!LoadSettings())
			{
				return;
			}

			Result result = ReferenceViewerProcessor.FindReferencesByCommand(Result.SearchType.WIN_GitGrep, settings.GetExcludeExtentions());
			if (result != null)
			{
				ReferenceViewerWindow.CreateWindow(result);
			}
		}

#endregion

#endif
	}
}