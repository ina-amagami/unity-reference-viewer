/*
unity-reference-viewer

Copyright (c) 2019 ina-amagami (ina@amagamina.jp)

This software is released under the MIT License.
https://opensource.org/licenses/mit-license.php
*/

using System;
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

	public struct CommandInfo
	{
		public readonly string Command;
		public readonly string Arguments;
		public readonly string NewLine;

		private const string Null = "\0";

		private CommandInfo(string command, string arguments, string newline)
		{
			Command = command;
			Arguments = arguments;
			NewLine = newline;
		}

		internal static readonly CommandInfo OSXSpotlit = new CommandInfo("mdfind", "-onlyin '{0}' -0 {1}", Null);
		internal static readonly CommandInfo OSXGrep = new CommandInfo("grep", "{1} -rl --null '{0}'", Null);
		internal static readonly CommandInfo OSXGit = new CommandInfo("git", "-C '{0}' grep -z -l {1}", Null);
		internal static readonly CommandInfo WinFindstr = new CommandInfo("findstr.exe", "/M /S {1} *", Environment.NewLine);
		internal static readonly CommandInfo WinGit = new CommandInfo("git.exe", "-C \"{0}\" grep -z -l {1}", Null);
	}

	public static class SearchTypeExtensions
	{
		private static readonly Dictionary<Result.SearchType, CommandInfo> Commands =
			new Dictionary<Result.SearchType, CommandInfo>
			{
				{Result.SearchType.OSX_Spotlight, CommandInfo.OSXSpotlit},
				{Result.SearchType.OSX_Grep, CommandInfo.OSXGrep},
				{Result.SearchType.OSX_GitGrep, CommandInfo.OSXGit},
				{Result.SearchType.WIN_FindStr, CommandInfo.WinFindstr},
				{Result.SearchType.WIN_GitGrep, CommandInfo.WinGit}
			};

		public static CommandInfo Command(this Result.SearchType searchType)
		{
			return Commands[searchType];
		}

		public static string AppendArguments(this Result.SearchType searchType, List<string> excludes)
		{
			if (searchType == Result.SearchType.OSX_Grep)
			{
				string appendArguments = string.Empty;
				foreach (string exclude in excludes)
				{
					appendArguments += string.Format(" --exclude='{0}'", exclude);
				}

				return appendArguments;
			}

			return string.Empty;
		}
	}
}