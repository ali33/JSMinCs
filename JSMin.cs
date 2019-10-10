using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for JSMin
/// </summary>
public class JSMin
{
    const char EOF = char.MinValue;
    char theA;
    char theB;
    char theLookahead = EOF;
    char[] input;
    char[] output;
    public JSMin(string js)
    {
        input = js.ToCharArray();
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
                putc(theA);
                goto case 2;
            case 2:
                theA = theB;
                if (theA == '\'' || theA == '"' || theA == '`')
                {
                    for (; ; )
                    {
                        putc(theA);
                        theA = get();
                        if (theA == theB)
                        {
                            break;
                        }
                        if (theA == '\\')
                        {
                            putc(theA);
                            theA = get();
                        }
                        if (theA == EOF)
                        {
                            throw new Exception("Error: JSMIN unterminated string literal.");
                        }
                    }
                }
                goto case 3;
            case 3:
                theB = next();
                if (theB == '/' && (theA == '(' || theA == ',' || theA == '=' ||
                                    theA == ':' || theA == '[' || theA == '!' ||
                                    theA == '&' || theA == '|' || theA == '?' ||
                                    theA == '{' || theA == '}' || theA == ';' ||
                                    theA == '\n'))
                {
                    putc(theA);
                    putc(theB);
                    for (; ; )
                    {
                        theA = get();
                        if (theA == '[')
                        {
                            for (; ; )
                            {
                                putc(theA);
                                theA = get();
                                if (theA == ']')
                                {
                                    break;
                                }
                                if (theA == '\\')
                                {
                                    putc(theA);
                                    theA = get();
                                }
                                if (theA == EOF)
                                {
                                    throw new Exception("Error: JSMIN unterminated set in Regular Expression literal.\n");
                                }
                            }
                        }
                        else if (theA == '/')
                        {
                            break;
                        }
                        else if (theA == '\\')
                        {
                            putc(theA);
                            theA = get();
                        }
                        if (theA == EOF)
                        {
                            throw new Exception("Error: JSMIN unterminated Regular Expression literal.\n");
                        }
                        putc(theA);
                    }
                    theB = next();
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
    public string Minify()
    {
        theA = '\n';
        action(3);
        while (theA != EOF)
        {
            switch (theA)
            {
                case ' ':
                    if (isAlphanum(theB))
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
                        switch (theB)
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
                                if (isAlphanum(theB))
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
                    switch (theB)
                    {
                        case ' ':
                            if (isAlphanum(theA))
                            {
                                action(1);
                                break;
                            }
                            action(3);
                            break;
                        case '\n':
                            switch (theA)
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
                                    if (isAlphanum(theA))
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
}