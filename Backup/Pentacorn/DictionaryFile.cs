using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pentacorn
{
    class DictionaryFile
    {
        public DictionaryFile() {}

        public DictionaryFile(TextReader tr) { Load(tr); }

        public DictionaryFile(IEnumerable<string> lines) { Load(lines); }

        public void Load(TextReader tr)            
        {
            Load(EnumerableEx.Generate(tr.ReadLine(), line => line != null, line => line = tr.ReadLine(), line => line));
        }

        public void Load(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                var match = Pattern.Match(line);
                if (match.Success)
                {
                    var key = match.Groups["Key"].Value ?? "";
                    var value = match.Groups["Value"].Value ?? "";
                    
                    Dict[key] = value;
                }
            }
        }

        public void Set<T>(string key, T value)
        {
            Dict[key] = value.ToString();
        }
        
        public string Get(string key, string dflt = "")
        {
            return Get<string>(key, dflt);
        }

        public T Get<T>(string key, T dflt = default(T))
        {
            string value;
            if (Dict.TryGetValue(key, out value))
                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(value);

            Dict.Add(key, dflt.ToString());
            return dflt;
        }

        public void Save(StreamWriter sw)
        {
            var lines = from kvp in Dict
                        orderby kvp.Key
                        select String.Format("{0} = {1}", kvp.Key, kvp.Value);
            
            foreach (var line in lines)
                sw.WriteLine(line);
        }

        private IDictionary<string, string> Dict = new Dictionary<string, string>();
        
        private static Regex Pattern = new Regex(@"((\s)*(?<Key>([^\=^\s^\n]+))[\s^\n]*
                                                 # key part (surrounding whitespace stripped)
                                                 \=
                                                 (\s)*(?<Value>([^\n^\s]+(\n){0,1})))
                                                 # value part (surrounding whitespace stripped)
                                                 ", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.CultureInvariant);
    }
}
