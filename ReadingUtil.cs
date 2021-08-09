using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace pkuManager
{
    // At one point the methods inside this class made sense to me....
    static class ReadingUtil
    {
        // Inputs a external text file to SearchStreamReaderForString
        private static int SearchFileForString(string fileLoc, string key, out int subid)
        {
            return SearchStreamReaderForString(new System.IO.StreamReader(fileLoc), key, out subid);
        }

        // Overloaded method of above that doesn't output subid
        private static int SearchFileForString(string fileLoc, string key)
        {
            return SearchFileForString(fileLoc, key, out int subid);
        }

        // Inputs an internal resource text file to SearchStreamReaderForString
        private static int SearchResourceForString(string fileLoc, string key, out int subid)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(fileLoc);
            writer.Flush();
            stream.Position = 0;
            return SearchStreamReaderForString(new StreamReader(stream), key, out subid);
        }

        // Overloaded method of above that doesn't output subid
        private static int SearchResourceForString(string fileLoc, string key)
        {
            return SearchResourceForString(fileLoc, key, out int subid);
        }

        // Inputs an internal resource text file to SearchStreamReaderForString
        private static int SearchResourceForStringAtIndex(string fileLoc, string key, out int subid, int index)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(fileLoc);
            writer.Flush();
            stream.Position = 0;
            return SearchStreamReaderForString(new StreamReader(stream), key, out subid, true, index);
        }

        // Inputs an internal resource text file to SearchStreamReaderAtIndex
        private static string SearchResourceAtIndex(string fileLoc, int index, int subid)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(fileLoc);
            writer.Flush();
            stream.Position = 0;
            return SearchStreamReaderAtIndex(new StreamReader(stream), index, subid);
        }

        // Searches a given txt file (as a stream reader) for a string and returns the line (and optionally subid) it occurs on
        // Returns -1 for no match
        // checks in lower case
        // files can also use "JUMP TO / [NUMBER]" lines to jump around index numbers
        private static int SearchStreamReaderForString(StreamReader stream, string key, out int subid, bool useIndex = false, int index = 0)
        {
            if (key == null)
            {
                subid = -1;
                return -1;
            }

            key = key.ToLower(); //lowercase checking

            int counter = 0;
            string line;
            string[] list;
            int maxList = 20; //Maximum amount of variations of the key
            string[] delimiter = { " / " };

            while ((line = stream.ReadLine()) != null || (useIndex && counter < index))
            {
                list = line.Split(delimiter, maxList, StringSplitOptions.None);
                for (int i = 0; i < list.Length; i++)
                {
                    if (list[0] == "JUMP TO")
                    {
                        counter = int.Parse(list[1]) - 1;
                        break;
                    }
                    else if (list[i].ToLower().Equals(key) && ((useIndex && index == counter) || !useIndex))
                    {
                        subid = i;
                        return counter;
                    }
                }
                counter++;
            }
            subid = -1;
            return -1; //No match found
        }

        //old search method
        /*
        // Searches a given txt file (as a stream reader) for a string at the given line number and returns the line number if it exists
        // Returns -1 for no match
        // checks in lower case
        // files can also use "JUMP TO / [NUMBER]" lines to jump around index numbers
        public static int SearchStreamReaderForStringAtIndex(StreamReader stream, string key, out int subid, int index)
        {
            if (key == null)
            {
                subid = -1;
                return -1;
            }

            key = key.ToLower(); //lowercase checking

            int counter = 0;
            string line;
            string[] list;
            int maxList = 20; //Maximum amount of variations of the key
            string[] delimiter = { " / " };

            while ((line = stream.ReadLine()) != null || counter < index)
            {
                list = line.Split(delimiter, maxList, StringSplitOptions.None);
                for (int i = 0; i < list.Length; i++)
                {
                    if (list[0] == "JUMP TO")
                    {
                        counter = int.Parse(list[1]) - 1;
                        break;
                    }
                    else if (index == counter && list[i].ToLower().Equals(key))
                    {
                        subid = i;
                        return counter;
                    }
                }
                counter++;
            }
            subid = -1;
            return -1; //No match found
        }*/

        // Searches a given txt file (as a stream reader) for a string at the given line number & subid and returns that string if it exists
        // Returns null for no match
        // files can also use "JUMP TO / [NUMBER]" lines to jump around index numbers
        private static string SearchStreamReaderAtIndex(StreamReader stream, int index, int subid)
        {
            int counter = 0;
            string line;
            string[] list;
            int maxList = 20; //Maximum amount of variations of the key
            string[] delimiter = { " / " };

            while ((line = stream.ReadLine()) != null || counter < index)
            {
                list = line.Split(delimiter, maxList, StringSplitOptions.None);
                for (int i = 0; i < list.Length; i++)
                {
                    if (list[0] == "JUMP TO")
                    {
                        counter = int.Parse(list[1]) - 1;
                        break;
                    }
                    else if (index == counter) //found index
                    {
                        if (list.Length - 1 < subid)
                            return null; //no entry under that subid found
                        else
                            return list[subid];
                    }
                }
                counter++;
            }
            return null; //No match found
        }

        // OLD STUFF ABOVE
        // NEW API BELOW

        // Returns the ID and subid of a search term from the given resource file.
        // Returns (-1,-1) if no match is found.
        public static (int, int) getIDAndSubIDFromFile(string filename, string search)
        {
            int id = SearchResourceForString((string)Properties.Resources.ResourceManager.GetObject(filename), search, out int subid);
            return (id, subid);
        }

        // Returns the ID of a search term from the given resource file.
        // Returns -1 if no match is found.
        public static int getIDFromFile(string filename, string search)
        {
            return getIDAndSubIDFromFile(filename, search).Item1;
        }

        // Returns the string from the file at id/subid (default subid is 0)
        // Returns null if no match is found.
        public static string getStringFromFile(string filename, int id, int subid = 0)
        {
            return ReadingUtil.SearchResourceAtIndex((string)Properties.Resources.ResourceManager.GetObject(filename), id, subid);
        }
    }
}