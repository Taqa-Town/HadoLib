using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI;

namespace HadoLib.Tokenizer;
public class TokenizingModel
{
    private LanguageSyntaxModel _syntaxModel;
    private List<TokenDefinition> _tokenDefinitions;
    public Dictionary<TokenType, SolidColorBrush> TokenTypesAndColors { get; private set; }
    
    public TokenizingModel(LanguageSyntaxModel syntaxModel) 
    {
        _syntaxModel = syntaxModel;
        TokenTypesAndColors = GetTokenTypesAndColors();
        _tokenDefinitions = [
            new(TokenType.Whitespace, @"^(\s)+"),

            new(TokenType.NewLine, @"^(\n)+"),

            new(TokenType.Keyword, @$"^\b({string.Join('|', _syntaxModel.Keywords)})\b"),

            new(TokenType.Operator, GenerateOperatorRegex(_syntaxModel.Operators)),

            new(TokenType.Identifier, @"^\b[a-zA-Z_][a-zA-Z0-9_]*\b"),

            new(TokenType.DataType, @$"^\b({string.Join('|', _syntaxModel.DataTypes)})\b"),

            //this regex is not perfect, but it's good enough for most cases, it covers all known comment types
            new(TokenType.Comment, @"^(?://.*|#.*|--.*|;.*|/\*[\s\S]*?\*/|"""""".*?""""""|'''.*?'''|=begin[\s\S]*?=end|<!--[\s\S]*?-->)"),
            
            new(TokenType.StringLiteral, @"^(""([^""\\]|\\.)*""|'([^'\\]|\\.)*')"),

            new(TokenType.NumberLiteral, @"^\b\d+(\.\d+)?\b")
            ];
    }
    public List<(string Text, SolidColorBrush Color)> Tokenize(string input)
    {
        List<(string, SolidColorBrush)> tokens = [];
        int currentIndex = 0;

        while (currentIndex < input.Length)
        {
            bool matchFound = false;

            foreach (var definition in _tokenDefinitions)
            {
                var match = definition.RegexOP.Match(input.Substring(currentIndex));
                if (match.Success)
                {
                    string value = match.Value;
                    var tokenColor = TokenTypesAndColors.TryGetValue(definition.Type, out var color) ? color : new SolidColorBrush(Colors.White);
                    tokens.Add((value, tokenColor));
                    currentIndex += value.Length;
                    matchFound = true;
                    break;
                }
            }

            if (!matchFound) currentIndex++;
        }

        return tokens;
    }


    #region helper methods

    private static string GenerateOperatorRegex(string[] operators)
    {
        var escapedOperators = operators.Select(Regex.Escape);      
        var sortedOperators = escapedOperators.OrderByDescending(op => op.Length);
        string operatorsPattern = string.Join("|", sortedOperators);
        return $@"\b({operatorsPattern})\b";
    }

    private Dictionary<TokenType, SolidColorBrush> GetTokenTypesAndColors()
    {
        var types = _syntaxModel.Colors;
        Dictionary<TokenType, SolidColorBrush> tokenTypes = [];

        foreach(var type in types)
        {
            if (Enum.TryParse<TokenType>(type.Key, out var tokenType))
            {
                tokenTypes.Add(tokenType, GetBrushFromHex(type.Value));
            }
        }
        var transparent = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        tokenTypes.Add(TokenType.Whitespace, transparent);
        tokenTypes.Add(TokenType.NewLine, transparent);
        return tokenTypes;
    }
    
    private static SolidColorBrush GetBrushFromHex(string hex)
    {
        hex = hex.Replace("#", string.Empty);

        byte a = 255;
        byte r, g, b;

        if (hex.Length == 6)
        {
            r = Convert.ToByte(hex.Substring(0, 2), 16);
            g = Convert.ToByte(hex.Substring(2, 2), 16);
            b = Convert.ToByte(hex.Substring(4, 2), 16);
        }
        else if (hex.Length == 8)
        {
            a = Convert.ToByte(hex.Substring(0, 2), 16);
            r = Convert.ToByte(hex.Substring(2, 2), 16);
            g = Convert.ToByte(hex.Substring(4, 2), 16);
            b = Convert.ToByte(hex.Substring(6, 2), 16);
        }
        else
        {
            throw new FormatException("Invalid color format");
        }
        return new SolidColorBrush(Color.FromArgb(a, r, g, b));
    }

    #endregion
}
