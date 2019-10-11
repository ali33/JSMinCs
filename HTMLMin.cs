using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
/// <summary>
/// Summary description for HTMLMin
/// </summary>
public class HTMLMin : IMinify
{
    const char EOF = char.MinValue;
    char[] input;
    public HTMLMin(string html)
    {
        input = html.ToCharArray();
    }

    int len, index;

    private char getc(int i)
    {
        return input[i];
    }

    private char nextc(int i)
    {
        if (i < input.Length)
            return input[i];
        else
            return EOF;
    }

    private char prevc(int i)
    {
        if (i > -1)
            return input[i];
        else
            return EOF;
    }

    public string Minify()
    {
        len = input.Length;
        index = 0;
        int i = 0;
        bool skip = false;
        bool incomment = false;
        char ch;
        for (; i < len; i++)
        {
            ch = getc(i);
            switch (ch)
            {                
                case '\u0020':
                case '\u00A0':
                case '\u1680':
                case '\u2000':
                case '\u2001':
                case '\u2002':
                case '\u2003':
                case '\u2004':
                case '\u2005':
                case '\u2006':
                case '\u2007':
                case '\u2008':
                case '\u2009':
                case '\u200A':
                case '\u202F':
                case '\u205F':
                case '\u3000':
                case '\u2028':
                case '\u2029':
                case '\u0009':
                case '\u000A':
                case '\u000B':
                case '\u000C':
                case '\u000D':
                case '\u0085':
                    if (skip) continue;
                    input[index++] = ch;
                    skip = true;
                    continue;
                case '<':
                    if (!incomment && nextc(i + 1) == '!' && nextc(i + 2) == '-' && nextc(i + 3) == '-')
                    {
                        skip = true;
                        incomment = true;
                        continue;
                    }
                    if (!incomment)
                        input[index++] = ch;
                    continue;
                case '>':
                    if (incomment)
                    {
                        skip = true;
                        if (prevc(i - 1) == '-' && prevc(i - 2) == '-')
                            incomment = false;
                        continue;
                    }
                    if (!incomment)
                        input[index++] = ch;
                    continue;
                default:
                    skip = false;
                    if (!incomment)
                        input[index++] = ch;
                    continue;
            }
        }
        return new string(input, 0, index);
    }

    public static string HtmlMinify(string html)
    {
        HTMLMin min = new HTMLMin(html);
        return min.Minify();
    }

    public static string HtmlTidy(string html,
        bool removeHtmlWhiteSpace = false,
        bool removeJsWhiteSpace = false,
        bool removeCssWhiteSpace = false)
    {
        const string SCRIPT_TAG_START = "<script";
        const string SCRIPT_TAG_END = "</script>";

        const string STYLE_TAG_START = "<style";
        const string STYLE_TAG_END = "</style>";

        const string HEAD_TAG_END = "</head>";
        const string BODY_TAG_END = "</body>";

        StringBuilder sb = new StringBuilder(html.Length);
        string tail = "";

        int startLength = STYLE_TAG_START.Length;
        int endLength = SCRIPT_TAG_END.Length;
        int start_idx, end_idx, offset = 0;
        int head_idx = html.IndexOf(HEAD_TAG_END, StringComparison.OrdinalIgnoreCase);

        string scriptStr = "";
        List<string> lsScripts = new List<string>();
        List<string> lsStyles = new List<string>();
        int close, endblock;

        Func<string, string> removeHtmlFx;

        if (removeHtmlWhiteSpace)
            removeHtmlFx = HtmlMinify;
        else
            removeHtmlFx = (x) => x;

        /*Javascript*/
        while ((start_idx = html.IndexOf(SCRIPT_TAG_START, offset, StringComparison.OrdinalIgnoreCase)) != -1)
        {

            if ((end_idx = html.IndexOf(SCRIPT_TAG_END, start_idx + startLength, StringComparison.OrdinalIgnoreCase)) == -1)
                break;

            scriptStr = html.Substring(start_idx, end_idx + endLength - start_idx);
            if (removeJsWhiteSpace)
            {
                close = scriptStr.IndexOf('>');
                if (close != -1)
                {
                    endblock = end_idx - start_idx;
                    scriptStr = string.Concat(scriptStr.Substring(0, close + 1),
                        JSMin.JsMinify(scriptStr.Substring(close + 1, endblock - (close + 1))),
                        scriptStr.Substring(endblock));
                }
            }
            sb.Append(html.Substring(offset, start_idx - offset));
            offset = end_idx + endLength;
            lsScripts.Add(scriptStr);
        }


        if ((end_idx = html.IndexOf(BODY_TAG_END, offset, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            sb.Append(html.Substring(offset, end_idx - offset));
            tail = html.Substring(end_idx);
        }
        else
        {
            sb.Append(html.Substring(offset));
        }

        html = sb.ToString();

        /*Css*/
        sb.Clear();
        offset = 0;

        if (head_idx != -1)
            head_idx = html.IndexOf(HEAD_TAG_END, StringComparison.OrdinalIgnoreCase);

        startLength = STYLE_TAG_START.Length;
        endLength = STYLE_TAG_END.Length;

        while ((start_idx = html.IndexOf(STYLE_TAG_START, offset, StringComparison.OrdinalIgnoreCase)) != -1)
        {


            if ((end_idx = html.IndexOf(STYLE_TAG_END, start_idx + startLength, StringComparison.OrdinalIgnoreCase)) == -1)
                break;

            scriptStr = html.Substring(start_idx, end_idx + endLength - start_idx);
            if (removeCssWhiteSpace)
            {
                close = scriptStr.IndexOf('>');
                if (close != -1)
                {
                    endblock = end_idx - start_idx;
                    scriptStr = string.Concat(scriptStr.Substring(0, close + 1),
                        CSSMin.CssMinify(scriptStr.Substring(close + 1, endblock - (close + 1))),
                        scriptStr.Substring(endblock));
                }
            }

            sb.Append(removeHtmlFx(html.Substring(offset, start_idx - offset)));

            offset = end_idx + endLength;

            if (start_idx > head_idx)
            {
                lsStyles.Add(scriptStr);
            }
            else
            {
                sb.Append(scriptStr);
            }
        }
        sb.Append(removeHtmlFx(html.Substring(offset)));


        html = sb.ToString();
        sb.Clear();

        if (head_idx != -1)
            head_idx = html.IndexOf(HEAD_TAG_END, StringComparison.OrdinalIgnoreCase);

        if (head_idx == -1)
        {
            sb.Append(html);
            sb.Append(string.Concat(lsScripts));
            sb.Append(removeHtmlFx(tail));
        }
        else
        {
            sb.Append(html.Substring(0, head_idx));
            sb.Append(string.Concat(lsStyles));
            sb.Append(html.Substring(head_idx));
            sb.Append(string.Concat(lsScripts));
            sb.Append(removeHtmlFx(tail));
        }
        return sb.ToString();
    }
}