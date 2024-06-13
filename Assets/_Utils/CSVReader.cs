using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class CSVReader
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };

    public static List<Dictionary<string, object>> Read(string file)
    {
        var list = new List<Dictionary<string, object>>();
        TextAsset data = Resources.Load("SpreadSheets/" + file) as TextAsset;

        var lines = SplitCSVIntoLines(data.text);

        if (lines.Count <= 1) return list;

        var header = Regex.Split(lines[0], SPLIT_RE);
        for (var i = 1; i < lines.Count; i++) // Start from 1 to skip header
        {
            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "") continue;

            var entry = new Dictionary<string, object>();
            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                string value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                object finalvalue = value;
                int n;
                float f;
                if (int.TryParse(value, out n))
                {
                    finalvalue = n;
                }
                else if (float.TryParse(value, out f))
                {
                    finalvalue = f;
                }
                entry[header[j]] = finalvalue;
            }
            list.Add(entry);
        }

        // PrintList(list); // Call PrintList before returning

        return list;
    }

    private static List<string> SplitCSVIntoLines(string csvText)
    {
        List<string> lines = new List<string>();
        using (StringReader reader = new StringReader(csvText))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains("\""))
                {
                    while (CountQuotes(line) % 2 != 0)
                    {
                        line += "\n" + reader.ReadLine();
                    }
                }
                lines.Add(line);
            }
        }
        return lines;
    }

    private static int CountQuotes(string line)
    {
        int count = 0;
        foreach (char c in line)
        {
            if (c == '"') count++;
        }
        return count;
    }

    private static void PrintList(List<Dictionary<string, object>> list)
    {
        Debug.Log("Printing CSV List:");
        foreach (var dict in list)
        {
            string line = "";
            foreach (var kvp in dict)
            {
                line += $"{kvp.Key}: {kvp.Value}, ";
            }
            Debug.Log(line.TrimEnd(',', ' '));
        }
    }
}
