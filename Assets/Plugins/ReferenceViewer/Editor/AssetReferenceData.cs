/*
unity-reference-viewer

Copyright (c) 2019 ina-amagami (ina_amagami@gc-career.com)

This software is released under the MIT License.
https://opensource.org/licenses/mit-license.php
*/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ReferenceViewer
{
	public class AssetData
	{
		public string Path { get; private set; }
		public Object Asset { get; private set; }
		public GUIContent ListItem { get; private set; }

		public AssetData(string path, bool isCreateListItem = true)
		{
			Path = path;
			Asset = AssetDatabase.LoadMainAssetAtPath(path);
			if (Asset == null)
			{
				return;
			}

			if (isCreateListItem)
			{
				CreateListItem(Asset.name);
			}
		}

		protected void CreateListItem(string displayName)
		{
			Texture icon = AssetDatabase.GetCachedIcon(Path);
			ListItem = new GUIContent(displayName, icon, Path);
		}
	}

	public class AssetReferenceData : AssetData
	{
		public List<AssetData> References { get; private set; }
		public bool IsFoldout { get; set; }

		public AssetReferenceData(string path) : base(path, isCreateListItem: false)
		{
			References = null;
			IsFoldout = false;
		}

		public void AddReference(string path)
		{
			if (References == null)
			{
				References = new List<AssetData>();
			}
			References.Add(new AssetData(path));
		}

		public void Apply()
		{
			string displayName = Asset.name;
			if (References == null)
			{
				displayName += "<i>（No References）</i>";
			}
			CreateListItem(displayName);
		}
	}
}