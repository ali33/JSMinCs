using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for JSMin
/// </summary>
public class CSSMin
{
    const char EOF = char.MinValue;
    char theA;
    char theB;
    char theLookahead = EOF;
    char[] input;
    char[] output;
    public CSSMin(string css)
    {
        input = css.ToCharArray();
        output = new char[input.Length + 1];
    }

    /// <summary>
    /// isAlphanum -- 
    /// </summary>
    /// <param name="c"></param>
    /// <returns>return true if the character is 
    /// a letter, digit, underscore,
    /// dollar sign, or non-ASCII character.</returns>
    bool isAlphanum(char c)
    {
        return ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') ||
            (c >= 'A' && c <= 'Z') || c == '_' || c == '$' || c == '\\' ||
            c > 126);
    }

    /// <summary>
    /// Check if a byte is a delimiter 
    /// </summary>
    /// <param name="c">byte to check</param>
    /// <returns>retval - 1 if yes. else 0</returns>
    private bool isDelimiter(char c)
    {
        return (c == '(' || c == ',' || c == '=' || c == ':' ||
            c == '[' || c == '!' || c == '&' || c == '|' ||
            c == '?' || c == '+' || c == '-' || c == '~' ||
            c == '*' || c == '/' || c == '{' || c == '\n' ||
            c == ';'
        );
    }

    int in_idx = 0;
    char getc()
    {
        if (in_idx == input.Length)
            return EOF;
        var c = input[in_idx];
        in_idx++;
        return c;
    }

    int out_idx = 0;
    void putc(char c)
    {
        output[out_idx] = c;
        out_idx++;
    } 

    /// <summary>
    /// jsmin -- Copy the input to the output, deleting the characters which are
    ///    insignificant to JavaScript. Comments will be removed. Tabs will be
    ///     replaced with spaces. Carriage returns will be replaced with linefeeds.
    ///    Most spaces and linefeeds will be removed.
    /// </summary>
    public string Minify()
    {
        bool ignore = false;                // if false then add byte to final output
        bool inComment = false;             // true when current bytes are part of a comment
        bool isDoubleSlashComment = false;  // '//' comment
        theA = '\n';
        while (theA != EOF)
        {
            ignore = false;

            theA = getc();
            if (theA == '\t')
                theA = ' ';
            else if (theA == '\t')
                theA = '\n';
            else if (theA == '\r')
                theA = '\n';

            if (theA == '\n')
                ignore = true;

            if (theA == ' ')
            {
                if ((theB == ' ') || isDelimiter(theB))
                    ignore = true;
                else
                {
                    theB = getc();
                    if (theB != EOF)
                    {
                        theB = getc();
                        if (isDelimiter(theB))
                            ignore = true;
                    }
                }
            }


            if (theA == '/')
            {
                theB = getc();
                if (theB == '/' || theB == '*')
                {
                    ignore = true;
                    inComment = true;
                    if (theB == '/')
                        isDoubleSlashComment = true;
                    else
                        isDoubleSlashComment = false;
                }
                if (theB == '/')
                {
                    int x = 0;
                    x = x + 1;
                }

            }

            // ignore all characters till we reach end of comment
            if (inComment)
            {
                while (true)
                {
                    theA = getc();
                    if (theA == '*')
                    {
                        theB = getc();
                        if (theB == '/')
                        {
                            theA = getc();
                            inComment = false;
                            break;
                        }
                    }
                    if (isDoubleSlashComment && theA == '\n')
                    {
                        inComment = false;
                        break;
                    }

                } // while (true)
                ignore = true;
            } // if (inComment) 


            if (!ignore)
                putc(theA);

            theB = theA;
        } // while (!endProcess) 
        return (new string(output, 0, out_idx)).Trim();
    }

    public static string CssMinify(string rawCss)
    {
        CSSMin min = new CSSMin(rawCss);
        return min.Minify();
    }

    const string S_TAG = "<style";
    const string E_TAG = "</style>";
    public static string ScriptMinify(string script)
    {
        string openTag, cssContent;
        int si, ei;
        si = script.IndexOf(S_TAG, 0, StringComparison.OrdinalIgnoreCase);
        if (si == -1)
            return script;

        ei = script.IndexOf(">", si);
        if (ei == -1)
            return script;

        openTag = script.Substring(0, ei);

        si = script.IndexOf(E_TAG, ei, StringComparison.OrdinalIgnoreCase);

        if (si == -1)
            return script;

        ei++;
        CSSMin min = new CSSMin(script.Substring(ei, si - ei));
        cssContent = min.Minify();
        return string.Concat(openTag, cssContent, script.Substring(si));
    }
}