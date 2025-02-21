using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HadoLib.Tokenizer;

public class LanguageSyntaxModel
{
    public Dictionary<string, string> Colors { get; set; }
    public string[] Keywords { get; set; }
    public string[] Operators { get; set; }
    public string[] DataTypes { get; set; }
    public string[] Comments { get; set; }

}
