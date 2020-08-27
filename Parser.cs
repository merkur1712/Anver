using System;
using System.Collections.Generic;


namespace Anver
{
    class Parser
    {



        public static string parse(string source, string param)
        {

            string[] entrys = source.Split(new string[] { "<tr class='list" }, StringSplitOptions.None);

            List<string> relevantEntrys = new List<string>();

            foreach (string s in entrys)
            {
                if (s.Contains(param))
                {
                    string ss = s;
                    ss = ss.Replace("<br>", "\r\n");
                    ss = ss.Replace("</td>", " | ");
                   

                    if (s.Contains("Nachrichten zum Tag"))
                        ss = ss.Substring(10);
                    else
                        ss = "<" + ss;

                    while (ss.Contains("<"))
                    {
                        int start = ss.IndexOf("<");
                        int length = ss.IndexOf(">") - start;

                        ss = ss.Remove(start, length + 1);
                    }
                    ss = ss.Replace("&nbsp;", "   ");


                    relevantEntrys.Add(ss);
                }
            }

            return String.Join("\r\n", relevantEntrys.ToArray());
        }

    }
}