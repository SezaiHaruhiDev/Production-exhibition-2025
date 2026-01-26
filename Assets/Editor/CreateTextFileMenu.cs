using System;
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 新規テキストファイルを作成する
/// EditorMenuスクリプトです
/// 
/// 使い方：
///   Project View上で右クリック → Create/TextFile をクリック
///   選択している階層にテキストファイルが生成されます
/// </summary>
public class CreateTextFileMenu
{
    /// <summary>
    /// Projectビュー上で選択している階層に新規テキストファイルを作成します
    /// </summary>
    [MenuItem("Assets/Create/TextFile")]
    public static void CreateTextFile()
    {
        var path = Application.dataPath;
        var selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (selectedPath.Length != 0)
        {
            if (!IsFolder(selectedPath))
            {
                selectedPath = selectedPath.Substring(0, selectedPath.LastIndexOf("/", StringComparison.CurrentCulture));
            }
            path = path.Remove(path.Length - "Assets".Length, "Assets".Length);
            path += selectedPath;
        }

        var fileName = "new text.txt";
        path += "/" + fileName;
        int cnt = 0;
        while (File.Exists(path))
        {
            if (path.Contains(fileName))
            {
                cnt++;
                var newFileName = "new text " + cnt + ".txt";
                path = path.Replace(fileName, newFileName);
                fileName = newFileName;
            }
            else
            {
                Debug.LogError("path dont contain " + fileName);
                break;
            }
        }

        // 空のテキストを書き込む.
        File.WriteAllText(path, "", System.Text.Encoding.UTF8);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 指定パスがフォルダかどうかチェックします
    /// </summary>
    /// <param name="path"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsFolder(string path)
    {
        try
        {
            return File.GetAttributes(path).Equals(FileAttributes.Directory);
        }
        catch (Exception ex)
        {
            if (ex.GetType() == typeof(FileNotFoundException))
            {
                return false;
            }
            throw ex;
        }
    }
}