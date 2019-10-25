using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for JSMin
/// </summary>
public class JSMin : IMinify
{
    const char EOF = char.MinValue;
    char thisChar;
    char nextChar;
    char theLookahead = EOF;
    char[] input;
    char[] output;
    int in_idx = 0;
    int out_idx = 0;
    public JSMin()
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


    char getc()
    {
        if (in_idx == input.Length)
            return EOF;
        var c = input[in_idx];
        in_idx++;
        return c;
    }

    void putc(char c)
    {
        output[out_idx] = c;
        out_idx++;
    }
    /// <summary>
    /// get -- return the next character from stdin. Watch out for lookahead. 
    /// If the character is a control character, translate it to a space or
    ///  linefeed.
    /// </summary>
    /// <returns></returns>
    char get()
    {
        char c = theLookahead;
        theLookahead = EOF;
        if (c == EOF)
        {
            c = getc();
        }
        if (c >= ' ' || c == '\n' || c == EOF)
        {
            return c;
        }
        if (c == '\r')
        {
            return '\n';
        }
        return ' ';
    }


    /// <summary>
    /// peek -- get the next character without getting it.
    /// </summary>
    /// <returns></returns>
    char peek()
    {
        theLookahead = get();
        return theLookahead;
    }

    /// <summary>
    /// next -- get the next character, excluding comments. peek() is used to see
    /// if a '/' is followed by a '/' or '*'.
    /// </summary>
    /// <returns></returns>
    char next()
    {
        char c = get();
        if (c == '/')
        {
            switch (peek())
            {
                case '/':
                    for (; ; )
                    {
                        c = get();
                        if (c <= '\n')
                        {
                            return c;
                        }
                    }
                case '*':
                    get();
                    for (; ; )
                    {
                        switch (get())
                        {
                            case '*':
                                if (peek() == '/')
                                {
                                    get();
                                    return ' ';
                                }
                                break;
                            case EOF:
                                throw new Exception("Error: JSMIN Unterminated comment");
                        }
                    }
                default:
                    return c;
            }
        }
        return c;
    }


    /// <summary>
    /// action -- do something! What you do is determined by the argument:
    ///        1   Output A. Copy B to A. Get the next B.
    ///        2   Copy B to A. Get the next B. (Delete A).
    ///        3   Get the next B. (Delete B).
    ///   action treats a string as a single character. Wow!
    ///   action recognizes a regular expression if it is preceded by ( or , or =.
    /// </summary>
    /// <param name="d"></param>
    void action(int d)
    {
        switch (d)
        {
            case 1:
                putc(thisChar);
                goto case 2;
            case 2:
                thisChar = nextChar;
                if (thisChar == '\'' || thisChar == '"' || thisChar == '`')
                {
                    for (; ; )
                    {
                        putc(thisChar);
                        thisChar = get();
                        if (thisChar == nextChar)
                        {
                            break;
                        }
                        if (thisChar == '\\')
                        {
                            putc(thisChar);
                            thisChar = get();
                        }
                        if (thisChar == EOF)
                        {
                            throw new Exception("Error: JSMIN unterminated string literal.");
                        }
                    }
                }
                goto case 3;
            case 3:
                nextChar = next();
                if (nextChar == '/' && (thisChar == '(' || thisChar == ',' || thisChar == '=' ||
                                    thisChar == ':' || thisChar == '[' || thisChar == '!' ||
                                    thisChar == '&' || thisChar == '|' || thisChar == '?' ||
                                    thisChar == '{' || thisChar == '}' || thisChar == ';' ||
                                    thisChar == '\n'))
                {
                    putc(thisChar);
                    putc(nextChar);
                    for (; ; )
                    {
                        thisChar = get();
                        if (thisChar == '[')
                        {
                            for (; ; )
                            {
                                putc(thisChar);
                                thisChar = get();
                                if (thisChar == ']')
                                {
                                    break;
                                }
                                if (thisChar == '\\')
                                {
                                    putc(thisChar);
                                    thisChar = get();
                                }
                                if (thisChar == EOF)
                                {
                                    throw new Exception("Error: JSMIN unterminated set in Regular Expression literal.\n");
                                }
                            }
                        }
                        else if (thisChar == '/')
                        {
                            break;
                        }
                        else if (thisChar == '\\')
                        {
                            putc(thisChar);
                            thisChar = get();
                        }
                        if (thisChar == EOF)
                        {
                            throw new Exception("Error: JSMIN unterminated Regular Expression literal.\n");
                        }
                        putc(thisChar);
                    }
                    nextChar = next();
                }
                break;
        }
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

        thisChar = '\n';
        action(3);
        while (thisChar != EOF)
        {
            switch (thisChar)
            {
                case ' ':
                    if (isAlphanum(nextChar))
                    {
                        action(1);
                    }
                    else
                    {
                        action(2);
                    }
                    break;
                case '\n':
                    {
                        switch (nextChar)
                        {
                            case '{':
                            case '[':
                            case '(':
                            case '+':
                            case '-':
                                action(1);
                                break;
                            case ' ':
                                action(3);
                                break;
                            default:
                                if (isAlphanum(nextChar))
                                {
                                    action(1);
                                }
                                else
                                {
                                    action(2);
                                }
                                break;
                        }
                    }
                    break;
                default:
                    switch (nextChar)
                    {
                        case ' ':
                            if (isAlphanum(thisChar))
                            {
                                action(1);
                                break;
                            }
                            action(3);
                            break;
                        case '\n':
                            switch (thisChar)
                            {
                                case '}':
                                case ']':
                                case ')':
                                case '+':
                                case '-':
                                case '"':
                                case '\'':
                                case '`':
                                    action(1);
                                    break;
                                default:
                                    if (isAlphanum(thisChar))
                                    {
                                        action(1);
                                    }
                                    else
                                    {
                                        action(3);
                                    }
                                    break;
                            }
                            break;
                        default:
                            action(1);
                            break;
                    }
                    break;
            }
        }
        return (new string(output, 0, out_idx)).Trim();
    }

    public static string JsMinify(string rawJs)
    {
        IMinify min = new JSMin();
        return min.Minify(rawJs);
    }

    const string S_TAG = "<script";
    const string E_TAG = "</script>";
    public static string ScriptMinify(string script)
    {
        string openTag, jsContent;
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
        IMinify min = new JSMin(); 
        jsContent = min.Minify(script.Substring(ei, si - ei));
        return string.Concat(openTag, jsContent, script.Substring(si));
    }
}