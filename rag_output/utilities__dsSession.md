# Documented File: dsSession.cs
**Repository Path:** `utilities\dsSession.cs`
**Primary Layer:** `utilities`
**Namespace:** `dsSessions`

## Source Code Representation
```csharp
using System;
using System.Collections;
using System.Reflection;
using System.Web.UI;

namespace dsSessions
{
    /// <summary>
    /// This class handles retention and display of selection criteria on all 
    /// datasearch forms.
    /// Retention is accomplished via a session variable which contains a list
    /// of field and value pairs. There is a seperate dictionary for each ds form.
    /// </summary>
    /// 


    public class dsSession
    {

        // this function loops through page controls and for text, hidden and checkbox fields
        // it checks if a value is stored in the appropriate Session datasearch dictionary
        // (The key value in the dictionary equals the fields id.)
        // If an entry is found, the field value is set to the value found in the dictionary
        public void LoadFieldsFromSession(Control parent, DataSearchDictionary DSDict)
        {
            foreach (Control c in parent.Controls)
            {
                string ltype = c.GetType().ToString();
                if (ltype == "System.Web.UI.HtmlControls.HtmlInputText" ||
                    ltype == "System.Web.UI.HtmlControls.HtmlInputHidden" ||
                    ltype == "System.Web.UI.HtmlControls.HtmlInputCheckBox")
                {
                    if (c.ID.IndexOf("txt") >= 0 || c.ID.IndexOf("sql") >= 0)
                    {
                        if (DSDict.Contains(c.ID))
                        {
                            Type controlType = c.GetType();

                            PropertyInfo ppropertyValue = controlType.GetProperty("Value");
                            string savedvalue = DSDict[c.ID];
                            ppropertyValue.SetValue(c, savedvalue, null);
                        }
                    }
                    if (c.ID.IndexOf("chk") >= 0)
                    {
                        // ID will only be in DSDict if box was checked
                        if (DSDict.Contains(c.ID))
                        {
                            // set box to checked
                            Type controlType = c.GetType();

                            PropertyInfo ppropertyValue = controlType.GetProperty("Checked");
                            ppropertyValue.SetValue(c, true, null);
                        }
                    }
                }
                if (c.Controls.Count > 0)
                {
                    LoadFieldsFromSession(c, DSDict);
                }
            }
        }
        // This function loops through page controls and for text, hidden and checkbox fields
        // whose ID's start with 'txt', 'sql', or 'chk' it does the following.
        // If the value of a text/hidden field is not blank, it adds/updates the dictionary
        // entry (a field id, field value pair).
        // If the field value is blank, it clears any existing entry for that field name from
        // the dictionary.
        // If checked=true for any checkbox, it adds/updates the dictionary entry.
        // If checked=false, it clears any existing entry for that field name from
        // the dictionary.

        public void LoadSessionFromFields(Control parent, DataSearchDictionary DSDict)
        {
            string controlId = "";
            string propertyValue;

            foreach (Control c in parent.Controls)
            {
                // limit scan to Text, Hidden and Checkbox fields
                string ltype = c.GetType().ToString();
                if (ltype == "System.Web.UI.HtmlControls.HtmlInputText" ||
                    ltype == "System.Web.UI.HtmlControls.HtmlInputHidden" ||
                    ltype == "System.Web.UI.HtmlControls.HtmlInputCheckBox")
                {
                    controlId = c.ID;
                    // get value for fields starting with txt and sql
                    if (controlId.IndexOf("txt") >= 0 || controlId.IndexOf("sql") >= 0)
                    {
                        // get field type
                        Type controlType = c.GetType();

                        // get property info associated with property "Value" 
                        PropertyInfo ppropertyValue = controlType.GetProperty("Value");

                        // get current value from field
                        propertyValue = ppropertyValue.GetValue(c, null).ToString();


                        if (propertyValue != "")
                        {
                            // if value not blank, add to or update dictionary
                            DSDict.Add(controlId, propertyValue);
                        }
                        else  // blank value - remove key if already in dictionary
                        {
                            if (DSDict.Contains(controlId))
                            {
                                DSDict.Remove(controlId);
                            }
                        }
                    }
                    // get checked status for fields starting with chk
                    if (controlId.IndexOf("chk") >= 0)
                    {
                        // get field type
                        Type controlType = c.GetType();

                        // get property info associated with property "Checked" 
                        PropertyInfo ppropertyValue = controlType.GetProperty("Checked");

                        // get current value from field
                        bool chkValue = (bool)ppropertyValue.GetValue(c, null);

                        // if box is checked, add to dictonary
                        if (chkValue)
                        {
                            DSDict.Add(controlId, "True");
                        }
                        else // if box not checked, delete entry from dictionary if already present 						
                        {
                            if (DSDict.Contains(controlId))
                            {
                                DSDict.Remove(controlId);
                            }
                        }
                    }
                }

                // if control has children, iterate through them
                if (c.Controls.Count > 0)
                {
                    LoadSessionFromFields(c, DSDict);
                }
            }
        }
    }
    // this class defines data/methods for dictionary 
    public class DataSearchDictionary : DictionaryBase
    {
        public String this[String key]
        {
            get
            {
                return ((String)Dictionary[key]);
            }
            set
            {
                Dictionary[key] = value;
            }
        }

        public ICollection Keys
        {
            get
            {
                return (Dictionary.Keys);
            }
        }

        public ICollection Values
        {
            get
            {
                return (Dictionary.Values);
            }
        }

        public void Add(String key, String value)
        {
            Dictionary.Add(key, value);
        }

        public bool Contains(String key)
        {
            return (Dictionary.Contains(key));
        }

        public void Remove(String key)
        {
            Dictionary.Remove(key);
        }
    }
}

```
