﻿#region License
/* The MIT License (MIT)
 * Copyright (c) 2011 Michael Stum, http://www.Stum.de <opensource@stum.de>
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace mstum.utils
{
    /// <summary>
    /// Read/Write .ini Files
    /// </summary>
    /// <remarks>
    /// It supports the simple .INI Format:
    /// 
    /// [SectionName]
    /// Key1=Value1
    /// Key2=Value2
    /// 
    /// [Section2]
    /// Key3=Value3
    /// 
    /// You can have empty lines (they are ignored), but comments are not supported
    /// Key4=Value4 ; This is supposed to be a comment, but will be part of Value4
    /// 
    /// Whitespace is not trimmed from the beginning and end of either Key and Value
    /// </remarks>
    public class IniFile
    {
        private Dictionary<string, Dictionary<string, string>> _iniFileContent;
        private readonly static Regex _sectionRegex = new Regex(@"(?<=\[)(?<SectionName>[^\]]+)(?=\])", RegexOptions.Compiled);
        private readonly static Regex _keyValueRegex = new Regex(@"(?<Key>[^=]+)=(?<Value>.+)", RegexOptions.Compiled);

        public IniFile() : this(null) { }

        public IniFile(string filename)
        {
            _iniFileContent = new Dictionary<string, Dictionary<string, string>>();
            if (filename != null) Load(filename);
        }

        /// <summary>
        /// Get a specific value from the .ini file
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="key"></param>
        /// <returns>The value of the given key in the given section, or NULL if not found</returns>
        public string GetValue(string sectionName, string key)
        {
            if (_iniFileContent.ContainsKey(sectionName) && _iniFileContent[sectionName].ContainsKey(key))
            {
                return _iniFileContent[sectionName][key];
            }
            return null;
        }

        /// <summary>
        /// Set a specific value in a section
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string sectionName, string key, string value)
        {
            if (!_iniFileContent.ContainsKey(sectionName)) _iniFileContent[sectionName] = new Dictionary<string, string>();
            _iniFileContent[sectionName][key] = value;
        }

        /// <summary>
        /// Get all the Values for a section
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns>A Dictionary with all the Key/Values for that section (maybe empty but never null)</returns>
        public Dictionary<string, string> GetSection(string sectionName)
        {
            if (_iniFileContent.ContainsKey(sectionName))
            {
                return new Dictionary<string, string>(_iniFileContent[sectionName]);
            }
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Set an entire sections values
        /// </summary>
        /// <remarks>
        /// This completely replaces the section, so entries not in the new sectionValues are removed
        /// </remarks>
        /// <param name="sectionName"></param>
        /// <param name="sectionValues"></param>
        public void SetSection(string sectionName, IDictionary<string, string> sectionValues)
        {
            if (sectionValues == null) return;
            _iniFileContent[sectionName] = new Dictionary<string, string>(sectionValues);
        }

        /// <summary>
        /// Load an .INI File
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool Load(string filename)
        {
            if (File.Exists(filename))
            {
                return LoadContents(File.ReadAllText(filename));
            }
            return false;
        }

        /// <summary>
        /// Save the content of this class to an INI File
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool Save(string filename)
        {
            var content = SaveContents();
            try
            {
                File.WriteAllText(filename, content);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Load an .INI File from a string
        /// </summary>
        /// <param name="fileContents"></param>
        /// <returns></returns>
        public bool LoadContents(string fileContents)
        {
            try
            {
                var content = fileContents.Replace("\r", string.Empty).Split('\n');
                _iniFileContent = new Dictionary<string, Dictionary<string, string>>();
                string currentSectionName = string.Empty;
                foreach (var line in content)
                {
                    Match m = _sectionRegex.Match(line);
                    if (m.Success)
                    {
                        currentSectionName = m.Groups["SectionName"].Value;
                    }
                    else
                    {
                        m = _keyValueRegex.Match(line);
                        if (m.Success)
                        {
                            string key = m.Groups["Key"].Value;
                            string value = m.Groups["Value"].Value;

                            var kvpList = _iniFileContent.ContainsKey(currentSectionName)
                                              ? _iniFileContent[currentSectionName]
                                              : new Dictionary<string, string>();
                            kvpList[key] = value;
                            _iniFileContent[currentSectionName] = kvpList;
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get the .ini File Contents as a string
        /// </summary>
        /// <returns></returns>
        public string SaveContents()
        {
            var sb = new StringBuilder();
            if (_iniFileContent != null)
            {
                foreach (var sectionName in _iniFileContent)
                {
                    sb.AppendFormat("[{0}]\r\n", sectionName.Key);
                    foreach (var keyValue in sectionName.Value)
                    {
                        sb.AppendFormat("{0}={1}\r\n", keyValue.Key, keyValue.Value);
                    }
                }
            }
            return sb.ToString();
        }
    }
}
