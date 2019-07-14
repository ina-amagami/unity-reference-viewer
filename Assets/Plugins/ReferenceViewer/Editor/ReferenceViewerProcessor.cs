/*
unity-reference-viewer

Copyright (c) 2019 ina-amagami (ina@amagamina.jp)

This software is released under the MIT License.
https://opensource.org/licenses/mit-license.php
*/

using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace ReferenceViewer
{
	public class Result
	{
		public enum SearchType
		{
			/// <summary>
			/// Mac/Spotlight
			/// </summary>
			OSX_Spotlight,
			/// <summary>
			/// Mac/Grep
			/// </summary>
			OSX_Grep,
			/// <summary>
			/// Mac/GitGrep
			/// </summary>
			OSX_GitGrep,
			/// <summary>
			/// Win/FindStr
			/// </summary>
			WIN_FindStr,
			/// <summary>
			/// Win/GitGrep
			/// </summary>
			WIN_GitGrep
		}
		public SearchType Type { get; set; }

		/// <summary>
		/// アセット毎の参照情報
		/// Reference information for each asset.
		/// </summary>
		public List<AssetReferenceData> Assets = new List<AssetReferenceData>();
	}

	/// <summary>
	/// 検索処理
	/// Search process implemention.
	/// </summary>
	public class ReferenceViewerProcessor
	{
		private const string ProgressBarTitle = "Find References In Project";

		/// <summary>
		/// GUIDとパスの対応情報
		/// Asset guid and filepath.
		/// </summary>
		public class AssetPath
		{
			public string GUID;
			public string Path;
		}

		/// <summary>
		/// OSコマンドで検索を実行
		/// Execute search with OS command.
		/// </summary>
		public static Result FindReferencesByCommand(Result.SearchType searchType, string command, List<string> excludeExtentionList = null)
		{
			// bash実行
			// Execution by bash.
			string file = "/bin/bash";
			if (searchType == Result.SearchType.WIN_FindStr)
			{
				// FindStr実行
				// Execution "findstr" command by windowsOS.
				file = "findstr";
			}
			else if (searchType == Result.SearchType.WIN_GitGrep)
			{
				// git実行
				// Execution "git" command by windowsOS.
				file = "git";
			}

			Result result = new Result();
			string applicationDataPathWithoutAssets = Application.dataPath.Replace("Assets", "");

			try
			{
				// パスを取得し、Projectビュー内の順番と同じになるようにソートする
				// Get the path, Sort so that it is the same as the order in the project view.
				List<AssetPath> paths = new List<AssetPath>();
				for (int i = 0; i < Selection.assetGUIDs.Length; ++i)
				{
					string guid = Selection.assetGUIDs[i];
					var assetPath = new AssetPath
					{
						GUID = guid,
						Path = AssetDatabase.GUIDToAssetPath(guid),
					};

					bool isDirectory = File.GetAttributes(assetPath.Path).Equals(FileAttributes.Directory);
					if (!isDirectory)
					{
						paths.Add(assetPath);
					}
					else
					{
						// ディレクトリを選択した場合は中のファイルも全て対象にする
						// When directory is selected, all the files in the target are also targeted.
						var includeFilePaths = Directory.GetFiles(assetPath.Path, "*.*", SearchOption.AllDirectories).Where(x => !x.EndsWith(".meta"));
						foreach (string path in includeFilePaths)
						{
							guid = AssetDatabase.AssetPathToGUID(path);
							if (string.IsNullOrEmpty(guid))
							{
								continue;
							}
							assetPath = new AssetPath
							{
								GUID = guid,
								Path = path
							};
							paths.Add(assetPath);
						}
					}
				}
				paths.Sort((a, b) => a.Path.CompareTo(b.Path));

				// アセット毎の参照情報の作成
				// Create reference information for each asset.
				int assetCount = paths.Count;
				for (int i = 0; i < assetCount; ++i)
				{
					string guid = paths[i].GUID;
					string path = paths[i].Path;
					string fileName = Path.GetFileName(path);
					var assetData = new AssetReferenceData(path);

					float progress = i / (float)assetCount;
					string progressText = string.Format("{0}% : {1}", (int)(progress * 100f), fileName);
					if (EditorUtility.DisplayCancelableProgressBar(ProgressBarTitle, progressText, progress))
					{
						// キャンセルしたら現時点での結果を返す
						// On canceled, return current result.
						EditorUtility.ClearProgressBar();
						return result;
					}

					var p = new Process();
					string cmd = string.Format(command, Application.dataPath, guid);
					p.StartInfo.FileName = file;
#if UNITY_EDITOR_OSX
					p.StartInfo.Arguments = "-c \" " + cmd + " \"";
#elif UNITY_EDITOR_WIN
					p.StartInfo.Arguments = cmd;
					p.StartInfo.CreateNoWindow = true;
#endif
					p.StartInfo.UseShellExecute = false;
					p.StartInfo.RedirectStandardOutput = true;
					p.Start();

					if (searchType == Result.SearchType.WIN_FindStr)
					{
						FindByFindStr(p, applicationDataPathWithoutAssets, path, assetData, excludeExtentionList);
					}
					else
					{
						FindByCommand(p, applicationDataPathWithoutAssets, path, assetData, excludeExtentionList);
					}

					p.Close();

					assetData.Apply();
					result.Assets.Add(assetData);
				}
				EditorUtility.ClearProgressBar();
				result.Type = searchType;
				return result;
			}
			catch (System.Exception e)
			{
				UnityEngine.Debug.LogError(e);
				EditorUtility.ClearProgressBar();
			}
			return null;
		}

		private static void FindByCommand(Process p, string applicationDataPathWithoutAssets, string path,
			AssetReferenceData assetData, List<string> excludeExtentionList = null)
		{
			p.WaitForExit();
			foreach (var line in p.StandardOutput.ReadToEnd().Split(new[] {"\n"}, StringSplitOptions.None))
			{
				if (string.IsNullOrWhiteSpace(line)) continue;

				// 出力不要な拡張子なら出力しない
				// Do not output if extensions that do not require output.
				var extension = Path.GetExtension(line);
				if (excludeExtentionList != null)
				{
					if (excludeExtentionList.Contains(extension))
					{
						continue;
					}
				}

				var projectFile = Path.Combine(Application.dataPath, line);
				// metaの中に参照を握っているケース
				if (extension == ".meta")
				{
					var assetPath = projectFile.Replace(".meta", "").Replace(applicationDataPathWithoutAssets, "");
					if (!string.Equals(assetPath, path, StringComparison.OrdinalIgnoreCase))
					{
						assetData.AddReference(assetPath);
					}

					continue;
				}

				assetData.AddReference(projectFile.Replace(applicationDataPathWithoutAssets, ""));
			}
		}

		private static void FindByFindStr(Process p, string applicationDataPathWithoutAssets, string path, AssetReferenceData assetData, List<string> excludeExtentionList = null)
		{
			List<string> assetPathList = new List<string>();
			while (p.StandardOutput.Peek() >= 0)
			{
				string line = p.StandardOutput.ReadLine();
				UnityEngine.Debug.Log(line);

				// 「ドライブ:ファイルパス:行数:行内容」 の形式で出力されるので、
				// ドライブからファイルパスの部分だけ取り出す
				// Output format is 「DiscDrive:FilePath:LineNumber:LineContent」
				// Pick out 「DiscDrive:FilePath」
				int driveIndex = line.IndexOf(':');
				if (driveIndex < 0)
				{
					continue;
				}

				int pathEndIndex = line.IndexOf(':', driveIndex + 1);
				if (pathEndIndex < 0)
				{
					continue;
				}

				string formatedPath = line.Substring(0, pathEndIndex);

				// 出力不要な拡張子なら出力しない
				// Do not output if extensions that do not require output.
				var extension = Path.GetExtension(formatedPath);
				if (excludeExtentionList != null)
				{
					if (excludeExtentionList.Contains(extension))
					{
						continue;
					}
				}

				// 重複排除
				// Deduplication.
				string assetPath = formatedPath.Replace(applicationDataPathWithoutAssets, "");
				if (assetPathList.Contains(assetPath))
				{
					continue;
				}

				// metaの中に参照を握っているケース
				if (extension == ".meta")
				{
					assetPath = assetPath.Replace(".meta", "");
					if (string.Equals(assetPath, path, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
				}

				assetPathList.Add(assetPath);

				assetData.AddReference(assetPath);
			}
			p.WaitForExit();
		}
	}
}