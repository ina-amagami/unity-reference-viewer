﻿/*
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
		public static Result FindReferencesByCommand(Result.SearchType searchType, List<string> excludeExtentionList)
		{
			CommandInfo commandInfo = searchType.Command();
			string[] eol = {commandInfo.NewLine};
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

					using (var p = new Process())
					{
						string arguments = string.Format(commandInfo.Arguments, Application.dataPath, guid);
						arguments += searchType.AppendArguments(excludeExtentionList);
						p.StartInfo.FileName = commandInfo.Command;
						p.StartInfo.Arguments = arguments;
						p.StartInfo.CreateNoWindow = true;
						p.StartInfo.UseShellExecute = false;
						p.StartInfo.RedirectStandardOutput = true;
						p.StartInfo.WorkingDirectory = Application.dataPath;
						p.Start();

						var sr = p.StandardOutput;
						FindByCommand(sr, applicationDataPathWithoutAssets, path, eol, assetData, excludeExtentionList);

						p.WaitForExit();
					}

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

		private static void FindByCommand(StreamReader sr, string applicationDataPathWithoutAssets, string path,
			string[] eol, AssetReferenceData assetData, List<string> excludeExtentionList = null)
		{
			foreach (var line in sr.ReadToEnd().Split(eol, StringSplitOptions.None))
			{
				if (line == null || line.Trim() == "") continue;

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
#if UNITY_EDITOR_WIN
					assetPath = assetPath.Replace("\\", "/");
#endif
					if (!string.Equals(assetPath, path, StringComparison.OrdinalIgnoreCase))
					{
						assetData.AddReference(assetPath);
					}

					continue;
				}

				assetData.AddReference(projectFile.Replace(applicationDataPathWithoutAssets, ""));
			}
		}
	}
}
