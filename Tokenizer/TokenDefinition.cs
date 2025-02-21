using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HadoLib.Tokenizer;

public class TokenDefinition
{
    public TokenType Type { get; set; }
    public Regex RegexOP { get; set; }
    public TokenDefinition(TokenType type, string pattern)
    {
        Type = type;
        RegexOP = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }
}
