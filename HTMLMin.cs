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
    int len, index;

    public HTMLMin()
    {

    }

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

    public string Minify(string rawCode)
    {
        input = rawCode.ToCharArray();
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
        IMinify min = new HTMLMin();
        return min.Minify(html);
    }

    public enum CodeType : int
    {
        Html = 0,
        Js = 1,
        Css = 2
    }

    public struct CodeItem
    {
        public CodeType ItemType { get; set; }
        public string OpenTag { get; set; }
        public string Content { get; set; }
        public string CloseTag { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public static CodeItem NewItem(
            CodeType itemType,
            string openTag,
            string content,
            string closeTag,
            int startIndex,
            int endIndex)
        {
            return new CodeItem
            {
                ItemType = itemType,
                OpenTag = openTag,
                Content = content,
                CloseTag = closeTag,
                StartIndex = startIndex,
                EndIndex = endIndex
            };
        }
    }

    public static string HtmlTidy(string html,
        bool minifyHtml = true,
        bool minifyJs = true,
        bool minifyCss = true,
        bool optimizeJsPostion = true,
        bool optimizeCssPostion = true)
    {
        //return html;
        const string SCRIPT_TAG_START = "<script";
        const string SCRIPT_TAG_END = "</script>";

        const string STYLE_TAG_START = "<style";
        const string STYLE_TAG_END = "</style>";

        const string HEAD_TAG_END = "</head>";
        const string BODY_TAG_END = "</body>";

        int startLength = STYLE_TAG_START.Length;
        int endLength = SCRIPT_TAG_END.Length;
        int startIdx, endIdx, offset = 0;
        int headIdx = html.IndexOf(HEAD_TAG_END, StringComparison.OrdinalIgnoreCase);
        int bodyEndIdx = -1;

        string scriptStr = "";
        List<CodeItem> lsParts = new List<CodeItem>();
        List<CodeItem> lsResult = new List<CodeItem>();
        int close, endblock;

        /*Javascript*/
        while ((startIdx = html.IndexOf(SCRIPT_TAG_START, offset, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            if ((endIdx = html.IndexOf(SCRIPT_TAG_END, startIdx + startLength, StringComparison.OrdinalIgnoreCase)) == -1)
                break;

            lsParts.Add(CodeItem.NewItem(CodeType.Html, "", html.Substring(offset, startIdx - offset), "", offset, startIdx));

            scriptStr = html.Substring(startIdx, endIdx + endLength - startIdx);
            close = scriptStr.IndexOf('>');
            if (close != -1)
            {
                endblock = endIdx - startIdx;
                lsParts.Add(CodeItem.NewItem(CodeType.Js,
                          scriptStr.Substring(0, close + 1),
                          scriptStr.Substring(close + 1, endblock - (close + 1)),
                          scriptStr.Substring(endblock),
                          startIdx,
                          endIdx + endLength
                          ));
            }
            offset = endIdx + endLength;
        }

        //if ((endIdx = html.IndexOf(BODY_TAG_END, offset, StringComparison.OrdinalIgnoreCase)) != -1)
        //{
        //    bodyEndIdx = endIdx;
        //    lsParts.Add(CodeItem.NewItem(CodeType.Html, "", html.Substring(offset, endIdx - offset), "", offset, endIdx));
        //    lsParts.Add(CodeItem.NewItem(CodeType.Html, "", html.Substring(endIdx), "", endIdx, html.Length - 1));  //tail
        //}
        //else
        //{
        lsParts.Add(CodeItem.NewItem(CodeType.Html, "", html.Substring(offset), "", offset, html.Length - 1));
        //}


        startLength = STYLE_TAG_START.Length;
        endLength = STYLE_TAG_END.Length;
        int i;
        for (i = 0; i < lsParts.Count; i++)
        {
            var code = lsParts[i];
            if (code.ItemType == CodeType.Js)
            {
                lsResult.Add(code);
            }
            else
            {
                offset = 0;
                while ((startIdx = code.Content.IndexOf(STYLE_TAG_START, offset, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    if ((endIdx = code.Content.IndexOf(STYLE_TAG_END, startIdx + startLength, StringComparison.OrdinalIgnoreCase)) == -1)
                        break;

                    lsResult.Add(CodeItem.NewItem(CodeType.Html, "", code.Content.Substring(offset, startIdx - offset), "", code.StartIndex + offset, code.StartIndex + startIdx));

                    scriptStr = code.Content.Substring(startIdx, endIdx + endLength - startIdx);
                    close = scriptStr.IndexOf('>');
                    if (close != -1)
                    {
                        endblock = endIdx - startIdx;
                        lsResult.Add(CodeItem.NewItem(CodeType.Css,
                         scriptStr.Substring(0, close + 1),
                         scriptStr.Substring(close + 1, endblock - (close + 1)),
                         scriptStr.Substring(endblock),
                         code.StartIndex + startIdx,
                         code.StartIndex + (endIdx + endLength)
                         ));
                    }
                    offset = endIdx + endLength;
                }
                lsResult.Add(CodeItem.NewItem(CodeType.Html, "", code.Content.Substring(offset), "", code.StartIndex + offset, code.StartIndex + code.Content.Length - 1));
            }
        }
        if (headIdx == -1 || bodyEndIdx == -1)
        {
            optimizeCssPostion = optimizeJsPostion = false;
        }

        StringBuilder sb = new StringBuilder();
        HTMLMin _htmlMin = new HTMLMin();
        CSSMin _cssMin = new CSSMin();
        JSMin _jsMin = new JSMin();

        for (i = 0; i < lsResult.Count; i++)
        {
            var code = lsResult[i];
            if (code.ItemType == CodeType.Html)
            {
                if (minifyHtml)
                    code.Content = _htmlMin.Minify(code.Content);
            }
            else if (code.ItemType == CodeType.Js)
            {
                if (minifyJs)
                    code.Content = _jsMin.Minify(code.Content);
            }
            if (code.ItemType == CodeType.Css)
            {
                if (minifyCss)
                    code.Content = _cssMin.Minify(code.Content);
            }
            //sb.Append("<hr>");
            //sb.AppendFormat("tt: {0} ({1}-{2})", i, code.StartIndex, code.EndIndex);
            //sb.Append("<hr>");
            sb.Append(code.OpenTag);
            sb.Append(code.Content);
            sb.Append(code.CloseTag);
        }
        return sb.ToString();
    }
}