using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class FIlesCatalogBuilder : IPreprocessBuildWithReport
{
	private const string CATALOG_FILE_NAME = "__catalog__.txt";

	public int callbackOrder => int.MaxValue;

	public void OnPreprocessBuild(BuildReport report)
	{
		PrepareStreamingAssetsFilesCatalog();
	}

	[MenuItem("Tools/Build SA Catalog")]
	public static void PrepareStreamingAssetsFilesCatalog()
	{
		string catalogFilePath = Path.Combine(Application.streamingAssetsPath, CATALOG_FILE_NAME);
		FileUtil.DeleteFileOrDirectory(catalogFilePath);

		string[] paths = Directory.GetFiles(Application.streamingAssetsPath, "*.*", SearchOption.AllDirectories);
		using (var file = File.OpenWrite(catalogFilePath))
		{
			using (var writer = new StreamWriter(file))
			{
				foreach (string path in paths)
				{
					if (path.EndsWith(".meta"))
					{
						continue;
					}

					writer.WriteLine(path.Replace(Application.streamingAssetsPath, "").Replace("\\", "/").Substring(1));
				}
			}
		}
	}
}
