/*
 * Copyright (c) 2013 Calvin Rien
 *
 * Based on the JSON parser by Patrick van Bergen
 * http://techblog.procurios.nl/k/618/news/view/14605/14863/How-do-I-write-my-own-parser-for-JSON.html
 *
 * Simplified it so that it doesn't throw exceptions
 * and can be used in Unity iPhone with maximum code stripping.
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NON INFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

//
// Modified by Ian Copland on 2015-11-12:
// - Added MiniJSON to the SdkCore namespace to avoid issues if the SDK
//   user is also using MiniJSON in their own project.
//

//
// Modified by Adrien Peyromaure-Debord-Broca on 2020-03-17:
// - Aligned to Unity's standards.
// - Use C# 7 pattern matching where possible to simplify.
// - Fix float/double/decimal parsing/serialization to be culture invariant and use the advised format
// (see: https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings#the-round-trip-r-format-specifier).
// - Use invariant culture for serialization when possible.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace UnityEngine.GameFoundation.MiniJson
{
    /// <summary>
    ///     This class encodes and decodes JSON strings.
    ///     Spec. details, see http://www.json.org/
    ///     JSON uses Arrays and Objects. These correspond here to the data types IList and IDictionary.
    ///     All numbers are parsed to doubles.
    /// </summary>
    /// <example>
    ///     <code>
    /// using UnityEngine;
    /// using System.Collections;
    /// using System.Collections.Generic;
    /// using MiniJSON;
    /// 
    /// public class MiniJSONTest : MonoBehaviour
    /// {
    ///     void Start ()
    ///     {
    ///         var jsonString = "{ \"array\": [1.44,2,3], " +
    ///                          "\"object\": {\"key1\":\"value1\", \"key2\":256}, " +
    ///                          "\"string\": \"The quick brown fox \\\"jumps\\\" over the lazy dog \", " +
    ///                          "\"unicode\": \"\\u3041 Men\u00fa sesi\u00f3n\", " +
    ///                          "\"int\": 65536, " +
    ///                          "\"float\": 3.1415926, " +
    ///                          "\"bool\": true, " +
    ///                          "\"null\": null }";
    /// 
    ///         var dict = Json.Deserialize(jsonString) as Dictionary&lt;string,object&gt;;
    /// 
    ///         Debug.Log("deserialized: " + dict.GetType());
    ///         Debug.Log("dict['array'][0]: " + ((List&lt;object&gt;) dict["array"])[0]);
    ///         Debug.Log("dict['string']: " + (string) dict["string"]);
    ///         Debug.Log("dict['float']: " + (double) dict["float"]); // floats come out as doubles
    ///         Debug.Log("dict['int']: " + (long) dict["int"]); // ints come out as longs
    ///         Debug.Log("dict['unicode']: " + (string) dict["unicode"]);
    /// 
    ///         var str = Json.Serialize(dict);
    /// 
    ///         Debug.Log("serialized: " + str);
    ///     }
    /// }
    /// </code>
    /// </example>
    static class Json
    {
        /// <summary>
        ///     Parses the string json into a value
        /// </summary>
        /// <param name="json">A JSON string.</param>
        /// <returns>
        ///     An List&lt;object&gt;, a Dictionary&lt;string, object&gt;, a double, an integer,a string, null, true, or false
        /// </returns>
        public static object Deserialize(string json)
        {
            // save the string for debug information
            return json == null ? null : Parser.Parse(json);
        }

        sealed class Parser : IDisposable
        {
            const string k_WordBreak = "{}[],:\"";

            static bool IsWordBreak(char c)
            {
                return char.IsWhiteSpace(c) || k_WordBreak.IndexOf(c) != -1;
            }

            enum Token
            {
                None,
                CurlyOpen,
                CurlyClose,
                SquaredOpen,
                SquaredClose,
                Colon,
                Comma,
                String,
                Number,
                True,
                False,
                Null
            }

            StringReader m_Json;

            Parser(string jsonString)
            {
                m_Json = new StringReader(jsonString);
            }

            public static object Parse(string jsonString)
            {
                using (var instance = new Parser(jsonString))
                {
                    return instance.ParseValue();
                }
            }

            public void Dispose()
            {
                m_Json.Dispose();
                m_Json = null;
            }

            Dictionary<string, object> ParseObject()
            {
                var table = new Dictionary<string, object>();

                // ditch opening brace
                m_Json.Read();

                // {
                while (true)
                {
                    switch (getNextToken)
                    {
                        case Token.None:
                        {
                            return null;
                        }
                        case Token.Comma:
                        {
                            continue;
                        }
                        case Token.CurlyClose:
                        {
                            return table;
                        }
                        default:
                        {
                            // name
                            var name = ParseString();
                            if (name == null) return null;

                            // :
                            if (getNextToken != Token.Colon) return null;

                            // ditch the colon
                            m_Json.Read();

                            // value
                            table[name] = ParseValue();
                            break;
                        }
                    }
                }
            }

            List<object> ParseArray()
            {
                var array = new List<object>();

                // ditch opening bracket
                m_Json.Read();

                // [
                var parsing = true;
                while (parsing)
                {
                    var nextToken = getNextToken;

                    switch (nextToken)
                    {
                        case Token.None:
                        {
                            return null;
                        }
                        case Token.Comma:
                        {
                            continue;
                        }
                        case Token.SquaredClose:
                        {
                            parsing = false;
                            break;
                        }
                        default:
                        {
                            var value = ParseByToken(nextToken);
                            array.Add(value);
                            break;
                        }
                    }
                }

                return array;
            }

            object ParseValue()
            {
                var nextToken = getNextToken;
                return ParseByToken(nextToken);
            }

            object ParseByToken(Token token)
            {
                switch (token)
                {
                    case Token.String:
                    {
                        return ParseString();
                    }
                    case Token.Number:
                    {
                        return ParseNumber();
                    }
                    case Token.CurlyOpen:
                    {
                        return ParseObject();
                    }
                    case Token.SquaredOpen:
                    {
                        return ParseArray();
                    }
                    case Token.True:
                    {
                        return true;
                    }
                    case Token.False:
                    {
                        return false;
                    }
                    case Token.Null:
                    {
                        return null;
                    }
                    default:
                    {
                        return null;
                    }
                }
            }

            string ParseString()
            {
                var s = new StringBuilder();
                char c;

                // ditch opening quote
                m_Json.Read();

                var parsing = true;
                while (parsing)
                {
                    if (m_Json.Peek() == -1)
                        break;

                    c = getNextChar;
                    switch (c)
                    {
                        case '"':
                        {
                            parsing = false;
                            break;
                        }
                        case '\\':
                        {
                            if (m_Json.Peek() == -1)
                            {
                                parsing = false;
                                break;
                            }

                            c = getNextChar;
                            switch (c)
                            {
                                case '"':
                                case '\\':
                                case '/':
                                {
                                    s.Append(c);
                                    break;
                                }
                                case 'b':
                                {
                                    s.Append('\b');
                                    break;
                                }
                                case 'f':
                                {
                                    s.Append('\f');
                                    break;
                                }
                                case 'n':
                                {
                                    s.Append('\n');
                                    break;
                                }
                                case 'r':
                                {
                                    s.Append('\r');
                                    break;
                                }
                                case 't':
                                {
                                    s.Append('\t');
                                    break;
                                }
                                case 'u':
                                {
                                    var hex = new char[4];
                                    for (var i = 0; i < 4; i++)
                                        hex[i] = getNextChar;

                                    s.Append((char)Convert.ToInt32(new string(hex), 16));
                                    break;
                                }
                            }

                            break;
                        }

                        default:
                        {
                            s.Append(c);
                            break;
                        }
                    }
                }

                return s.ToString();
            }

            object ParseNumber()
            {
                var number = getNextWord;

                if (number.IndexOf('.') == -1)
                {
                    long.TryParse(number, out var parsedInt);
                    return parsedInt;
                }

                double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedDouble);
                return parsedDouble;
            }

            void EatWhitespace()
            {
                while (char.IsWhiteSpace(peekNextChar))
                {
                    m_Json.Read();

                    if (m_Json.Peek() == -1)
                        break;
                }
            }

            char peekNextChar => Convert.ToChar(m_Json.Peek());

            char getNextChar => Convert.ToChar(m_Json.Read());

            string getNextWord
            {
                get
                {
                    var word = new StringBuilder();

                    while (!IsWordBreak(peekNextChar))
                    {
                        word.Append(getNextChar);

                        if (m_Json.Peek() == -1)
                            break;
                    }

                    return word.ToString();
                }
            }

            Token getNextToken
            {
                get
                {
                    EatWhitespace();

                    if (m_Json.Peek() == -1)
                        return Token.None;

                    switch (peekNextChar)
                    {
                        case '{':
                        {
                            return Token.CurlyOpen;
                        }
                        case '}':
                        {
                            m_Json.Read();
                            return Token.CurlyClose;
                        }
                        case '[':
                        {
                            return Token.SquaredOpen;
                        }
                        case ']':
                        {
                            m_Json.Read();
                            return Token.SquaredClose;
                        }
                        case ',':
                        {
                            m_Json.Read();
                            return Token.Comma;
                        }
                        case '"':
                        {
                            return Token.String;
                        }
                        case ':':
                        {
                            return Token.Colon;
                        }
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                        case '-':
                        {
                            return Token.Number;
                        }
                    }

                    switch (getNextWord)
                    {
                        case "false":
                        {
                            return Token.False;
                        }
                        case "true":
                        {
                            return Token.True;
                        }
                        case "null":
                        {
                            return Token.Null;
                        }
                    }

                    return Token.None;
                }
            }
        }

        /// <summary>
        ///     Converts a IDictionary / IList object or a simple type (string, int, etc.) into a JSON string
        /// </summary>
        /// <param name="obj">A Dictionary&lt;string, object&gt; / List&lt;object&gt;</param>
        /// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
        public static string Serialize(object obj)
        {
            return Serializer.Serialize(obj);
        }

        sealed class Serializer
        {
            StringBuilder m_Builder;

            Serializer()
            {
                m_Builder = new StringBuilder();
            }

            public static string Serialize(object obj)
            {
                var instance = new Serializer();

                instance.SerializeValue(obj);

                return instance.m_Builder.ToString();
            }

            void SerializeValue(object value)
            {
                switch (value)
                {
                    case null:
                    {
                        m_Builder.Append("null");
                        break;
                    }
                    case string stringValue:
                    {
                        SerializeString(stringValue);
                        break;
                    }
                    case bool boolValue:
                    {
                        m_Builder.Append(boolValue ? "true" : "false");
                        break;
                    }
                    case IList listValue:
                    {
                        SerializeArray(listValue);
                        break;
                    }
                    case IDictionary dictionaryValue:
                    {
                        SerializeObject(dictionaryValue);
                        break;
                    }
                    case char charValue:
                    {
                        SerializeString(new string(charValue, 1));
                        break;
                    }
                    default:
                    {
                        SerializeOther(value);
                        break;
                    }
                }
            }

            void SerializeObject(IDictionary obj)
            {
                var first = true;

                m_Builder.Append('{');

                foreach (var e in obj.Keys)
                {
                    if (!first)
                        m_Builder.Append(',');

                    SerializeString(e.ToString());
                    m_Builder.Append(':');

                    SerializeValue(obj[e]);

                    first = false;
                }

                m_Builder.Append('}');
            }

            void SerializeArray(IList anArray)
            {
                m_Builder.Append('[');

                var first = true;

                foreach (var obj in anArray)
                {
                    if (!first)
                        m_Builder.Append(',');

                    SerializeValue(obj);

                    first = false;
                }

                m_Builder.Append(']');
            }

            void SerializeString(string str)
            {
                m_Builder.Append('\"');

                var charArray = str.ToCharArray();
                foreach (var c in charArray)
                {
                    switch (c)
                    {
                        case '"':
                        {
                            m_Builder.Append("\\\"");
                            break;
                        }
                        case '\\':
                        {
                            m_Builder.Append("\\\\");
                            break;
                        }
                        case '\b':
                        {
                            m_Builder.Append("\\b");
                            break;
                        }
                        case '\f':
                        {
                            m_Builder.Append("\\f");
                            break;
                        }
                        case '\n':
                        {
                            m_Builder.Append("\\n");
                            break;
                        }
                        case '\r':
                        {
                            m_Builder.Append("\\r");
                            break;
                        }
                        case '\t':
                        {
                            m_Builder.Append("\\t");
                            break;
                        }
                        default:
                        {
                            var codepoint = Convert.ToInt32(c);
                            if (codepoint >= 32 && codepoint <= 126)
                            {
                                m_Builder.Append(c);
                            }
                            else
                            {
                                m_Builder.Append("\\u");
                                m_Builder.Append(codepoint.ToString("x4"));
                            }

                            break;
                        }
                    }
                }

                m_Builder.Append('\"');
            }

            void SerializeOther(object value)
            {
                switch (value)
                {
                    // NOTE: decimals lose precision during serialization.
                    // They always have, I'm just letting you know.
                    // Previously floats and doubles lost precision too.
                    case float floatValue:
                    {
                        m_Builder.Append(floatValue.ToString("G9", CultureInfo.InvariantCulture));
                        break;
                    }
                    case int _:
                    case uint _:
                    case long _:
                    case sbyte _:
                    case byte _:
                    case short _:
                    case ushort _:
                    case ulong _:
                    {
                        m_Builder.Append(value);
                        break;
                    }
                    case double _:
                    case decimal _:
                    {
                        m_Builder.Append(Convert.ToDouble(value).ToString("G17", CultureInfo.InvariantCulture));
                        break;
                    }

                    //Since other base types are IConvertible too, this case needs to be after them.
                    //Make sure the serialization is culture independent as much as possible.
                    case IConvertible convertibleValue:
                    {
                        SerializeString(convertibleValue.ToString(CultureInfo.InvariantCulture));
                        break;
                    }
                    default:
                    {
                        SerializeString(value.ToString());
                        break;
                    }
                }
            }
        }
    }
}
