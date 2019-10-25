using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for JSMin
/// </summary>
public class CSSMin : IMinify
{
    const char EOF = char.MinValue;
    char[] input;
    char[] output;
    public CSSMin()
    {
       
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

    char peekc()
    {
        if (in_idx >= input.Length)
            return EOF;
        return input[in_idx];
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
    public string Minify(string rawCode)
    {
        if (string.IsNullOrWhiteSpace(rawCode))
            return "";
        input = rawCode.ToCharArray();
        output = new char[input.Length + 1];
        in_idx = 0;
        out_idx = 0;

        char lastChar = EOF;                   // current byte read
        char thisChar = EOF;                  // previous byte read
        char nextChar = EOF;                  // byte read in peek()
        bool endProcess = false;            // loop control
        bool ignore = false;                // if false then add byte to final output
        bool inComment = false;             // true when current bytes are part of a comment
        bool isDoubleSlashComment = false;  // '//' comment


        // main processing loop
        while (!endProcess)
        {
            endProcess = peekc() == EOF;    // check for EOF before reading
            if (endProcess)
                break;

            ignore = false;
            thisChar = getc();

            if (thisChar == '\t')
                thisChar = ' ';
            //else if (thisChar == '\t')
            //    thisChar = '\n';
            else if (thisChar == '\r')
                thisChar = '\n';

            if (thisChar == '\n')
                ignore = true;

            if (thisChar == ' ')
            {
                if ((lastChar == ' ') || isDelimiter(lastChar))
                    ignore = true;
                else
                {
                    endProcess = (peekc() == EOF); // check for EOF
                    if (!endProcess)
                    {
                        nextChar = peekc();
                        if (isDelimiter(nextChar))
                            ignore = true;
                    }
                }
            }


            if (!inComment && thisChar == '/')
            {
                nextChar = peekc();
                if (nextChar == '*')
                {
                    ignore = true;
                    inComment = true;
                }
            }

            // ignore all characters till we reach end of comment
            if (inComment)
            {
                while (true)
                {
                    thisChar = getc();
                    if (thisChar == '*')
                    {
                        nextChar = peekc();
                        if (nextChar == '/')
                        {
                            thisChar = getc();
                            inComment = false;
                            break;
                        }
                    }
                    else if (thisChar == EOF)
                    {
                        break;
                    }
                    if (isDoubleSlashComment && thisChar == '\n')
                    {
                        inComment = false;
                        break;
                    }

                } // while (true)
                ignore = true;
            } // if (inComment) 


            if (!ignore)
                putc(thisChar);

            lastChar = thisChar;
        } // while (!endProcess) 
        string result = (new string(output, 0, out_idx)).Trim();

        return result;
    }

    public static string CssMinify(string rawCss)
    {
        IMinify min = new CSSMin();
        return min.Minify(rawCss);
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
        IMinify min = new CSSMin(); 
        cssContent = min.Minify(script.Substring(ei, si - ei));
        return string.Concat(openTag, cssContent, script.Substring(si));
    }    
}