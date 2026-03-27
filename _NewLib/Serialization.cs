using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace _NewLib
{
    public class Serialization
    {
        public static bool ExportListAsSerialized<T>(List<T> listOfObjects, string exportFilePath, out string errMsg)
        {
            // 'out' requirement;
            errMsg = "";

            bool isSuccess = true;

            // Check that the prescribed file path can actually be written to.
            // Note: this will delete an existing file's content.
            if (!ImportExport.CanWriteToFile(exportFilePath, out errMsg))
            {
                isSuccess = false;
            }
            else
            {
                try
                {
                    FileStream stream = File.Open(exportFilePath, FileMode.Create);
                    BinaryFormatter formatter = new BinaryFormatter();

                    foreach (T t in listOfObjects)
                    {
                        formatter.Serialize(stream, t);
                    }

                    stream.Close();
                }
                catch (Exception e)
                {
                    isSuccess = false;
                    errMsg = e.Message;
                }
            }

            return isSuccess;
        }

        public static bool ImportListAsSerialized<T>(out List<T> listOfObjects, string importFilePath, out string errMsg)
        {
            // 'out' requirement;
            listOfObjects = new List<T>();
            errMsg = "";

            bool isSuccess = true;

            try
            {
                FileStream stream = File.Open(importFilePath, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();

                bool eof = false;
                while (!eof)
                {
                    try
                    {
                        T t = (T)formatter.Deserialize(stream);

                        listOfObjects.Add(t);
                    }
                    catch (Exception e)
                    {
                        if (e.Message.StartsWith("End of Stream"))
                        {
                            eof = true;
                        }
                        else
                        {
                            isSuccess = false;
                            errMsg = e.Message;
                        }
                    }
                }

                stream.Close();
            }
            catch (Exception e)
            {
                isSuccess = false;
                errMsg = e.Message;
            }

            return isSuccess;
        }

    }
}
