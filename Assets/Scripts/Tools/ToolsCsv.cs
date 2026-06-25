using UnityEngine;
using System;
using System.IO;
using UnityEditor;
using System.Collections.Generic;
using System.Text;

public class ToolsCsv : MonoBehaviour
{
    private static ToolsCsv instance;
    private static string allPathFile;
    private int countCategory = 0;
    private static int lastLine = 1;
    private static long indexStartLinePosition = 0;
    private static List<string> categoryList = new List<string>();

    public string m_gameTypeString = "Realist";

    private void Awake()
    {
        instance = this;
        string pathFile = Application.dataPath + "/EvaluationResult";

        if (!Directory.Exists(pathFile))
        {
            Directory.CreateDirectory(pathFile);
        }

        string date = DateTime.Now.ToString();
        date = date.Replace('/', '_');
        date = date.Replace(":", "");
        string nameFile = "/" + date.ToString() + "_" + m_gameTypeString+  "_MetricsResult.csv";

        Debug.Log(nameFile);
        allPathFile = pathFile + nameFile;

        File.CreateText(pathFile + nameFile);

    }

    public static void CreateCategory(string categoryName)
    {
        using (StreamWriter sw = new StreamWriter(allPathFile, true))
        {
            if (categoryList.Contains(categoryName))
            {
                Debug.LogError("Category already contain");
                return;
            }
            sw.Write(categoryName + ";");

            categoryList.Add(categoryName);
            instance.countCategory++;
        }
    }


    public static void CreateCategory(params string[] categoryName)
    {
        instance.countCategory = categoryName.Length;
        string data = "";
        for (int i = 0; i < categoryName.Length; i++)
        {
            data += categoryName[i] + ";";
        }
        using (StreamWriter sw = new StreamWriter(allPathFile, true))
        {
            sw.Write(data + "\n");   
        }
    }





    public static void WriteLine(string data)
    {
        using (StreamWriter sw = new StreamWriter(allPathFile, true))
        {
            sw.WriteLine(data);
        }
    }

    public static void WriteLine(params string[] values)
    {
        string data = "";
        for (int i = 0; i < values.Length; i++)
        {
            data += values[i] + ";";
        }
        using (StreamWriter sw = new StreamWriter(allPathFile, true))
        {
            sw.WriteLine(data);
        }
    }


    public static void WriteData(string categoryName, string data)
    {


        if (!categoryList.Contains(categoryName))
        {
            Debug.LogError("Category doesn't exist");
            return;
        }
        int index = categoryList.IndexOf(categoryName);

        int i = 0;
        int indexWriting = (int)indexStartLinePosition - 1;

        using (StreamReader sr = new StreamReader(allPathFile))
        {
            string allText = sr.ReadLine();
            while (i < index)
            {
                indexWriting = allText.IndexOf(';', indexWriting);
                i++;
            }

            string val = allText.Insert(indexWriting, data);
        }

        using (FileStream fs = File.Open(allPathFile, FileMode.Open, FileAccess.Write))
        {
            byte[] info = new UTF8Encoding(true).GetBytes(";" + data);
            fs.Seek((long)indexWriting, SeekOrigin.Begin);
            fs.Write(info, 0, info.Length);
        }
    }

    public static void AddLine()
    {
        using (FileStream fs = File.Open(allPathFile, FileMode.Append, FileAccess.Write))
        {
            indexStartLinePosition = fs.Position;
            string newLine = "\n";
            for (int i = 0; i < categoryList.Count; i++)
            {
                newLine += ";";
            }
            byte[] info = new UTF8Encoding(true).GetBytes(newLine);
            fs.Write(info);
            lastLine++;
        }
    }
}
