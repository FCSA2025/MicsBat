# Documented File: CSV.cs
**Repository Path:** `SendLAMLreports\CSV.cs`
**Primary Layer:** `SendLAMLreports`
**Namespace:** `SendLAMLreports`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SendLAMLreports
{
    /// <summary>
    /// This class provides methods that facilitate the parsing of comma-separated-values (CSV)
    /// in a line of text.
    /// </summary>
    public class CSV
    {
        /// <summary>
        /// This methods inputs a single line of text in CSV format and returns an
        /// string array giving the individual CSV text fields; the method handles
        /// fields that are quoted and contain commas; the returned value gives the
        /// number of fields read or -1 if the line parse attempt failed.
        /// </summary>
        /// <param name="line"> - a prescribed single line of text read from the import file.</param>
        /// <param name="fields"> - output array of CSV fields.</param>
        /// <returns></returns>
        public static int ParseFields(string line, out string[] fields)
        {
            // Any field may be contained in quotes. However fields that contain a line-break, 
            // comma, or quotation marks must be contained in quotes.
            //
            // To re-emphasize the above, line breaks within a field are allowed within a CSV as
            // long as they are wrapped in quotation marks, this is what trips most people up who
            // are simply reading line by line like it’s a regular text file.
            // 
            // Note that individual double-quote marks (") within a field can be 'escaped' by doing
            // double double-quote marks ("") - but this is not handled by this method.

            // 'out' requirement.
            fields = new string[0];

            string field = "";
            List<string> fieldsList = new List<string>();

            // A comma could occur inside a quoted string; this would confuse the 'split'.
            // Replace every comma inside a quoted string with the TILDE char '~'.

            List<char> characters = new List<char>();

            bool insideQuotedvalue = false;
            const char QUOTE = '"';
            const char TILDE = '~';
            const char COMMA = ',';

            int importOK = 1;

            if (!String.IsNullOrWhiteSpace(line))
            {
                // If we reach here the rawLine must contain some text.
                char[] chars = line.ToCharArray();

                for (int i = 0; i < line.Length; i++)
                {
                    char c = chars[i];

                    if (i == line.Length - 1)
                    {
                        // Be careful: the last character on a line could be a comma which appends a blank field.
                        if (c == COMMA)
                        {
                            field = new String(characters.ToArray());
                            fieldsList.Add(field);
                            fieldsList.Add("");
                        }
                        else
                        {
                            characters.Add(c);
                            field = new String(characters.ToArray());
                            fieldsList.Add(field);
                        }
                    }
                    else if (!insideQuotedvalue && (c == QUOTE))
                    {
                        insideQuotedvalue = true;
                        characters.Add(c);
                    }
                    else if (insideQuotedvalue && (c == QUOTE))
                    {
                        insideQuotedvalue = false;
                        characters.Add(c);
                    }
                    else if (insideQuotedvalue && (c == COMMA))
                    {
                        characters.Add(TILDE);
                    }
                    // This general case successfully handles the first character being a comma.
                    else if (!insideQuotedvalue && (c == COMMA))
                    {
                        field = new string(characters.ToArray());
                        fieldsList.Add(field);
                        characters = new List<char>();
                    }
                    else
                    {
                        characters.Add(c);
                    }
                }

                List<string> deQuotedFields = new List<string>();

                for (int i = 0; i < fieldsList.Count; i++)
                {
                    // Replace any TILDE by a COMMA.
                    string str = fieldsList[i].Replace(TILDE, COMMA);

                    str = str.Trim();

                    if (str != "")
                    {
                        if (str.First() == QUOTE) str = str.Substring(1, str.Length - 1);
                        if (str.Last() == QUOTE) str = str.Substring(0, str.Length - 1);
                    }

                    string reducedStr = str.Trim();

                    deQuotedFields.Add(reducedStr);

                    //Console.Error.Write("\n{0,3}: |{1}|", i + 1, reducedStr);
                }

                fields = deQuotedFields.ToArray();

                importOK = fields.Length;
            }

            return importOK;
        }






    }
}

```
