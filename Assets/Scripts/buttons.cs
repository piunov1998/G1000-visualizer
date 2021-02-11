using System;
using System.Data;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SimpleFileBrowser;

public class buttons : MonoBehaviour
{
    public DataTable dt = new DataTable();
    public string[] lines;  

    void Start()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("csv files", ".csv"));
        FileBrowser.SetDefaultFilter(".csv");
    }

    void onSuccess(string[] paths)
    {   
        GameObject.FindGameObjectWithTag("File Status").GetComponent<Text>().text = "File loaded!" + "(" + paths.Length + ")";
        DataTabletConstructor(paths);
    }

    void onCancel()
    {
        return;
    }

    void DataTabletConstructor(string[] paths)
    {
        using (StreamReader sr = new StreamReader(paths[0]))
        {
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.StartsWith("#"))
                {
                    GameObject.FindGameObjectWithTag("File Status").GetComponent<Text>().text += ("\n" + line);
                }
                else
                {
                    Array.Resize(ref lines, lines.Length + 1);
                    lines[lines.Length - 1] = line;
                }
            }
            Debug.Log("Loaded " + lines.Length + " strings");
            string[] headers = lines[0].Split(',');
            foreach (string header in headers)
            {
                dt.Columns.Add(header.Trim());
            }
            for (int i = 1; i < (lines.Length); i++)
            {
                string[] rows = lines[i].Split(',');
                for (int k = 1; k < (rows.Length); k++)
                {
                    rows[k] = rows[k].Trim();
                }
                if (rows.Length < headers.Length)
                {
                    Array.Resize(ref rows, headers.Length);
                }
                DataRow dr = dt.NewRow();
                for (int j = 0; j < headers.Length; j++)
                {    
                    dr[j] = rows[j];
                }
                dt.Rows.Add(dr);
            }
        }

        DataHolder.data = dt;
        DataHolder.Opened = true;

    }

    public void OpenFile()
    {
        FileBrowser.ShowLoadDialog(onSuccess, onCancel, false, false, null, "Load", "Select");
    }

    public void PlayPressed()
    {
        SceneManager.LoadScene("FlyScene");
    }

    public void ExitPressed()
    {
        UnityEngine.Application.Quit();
    }
}