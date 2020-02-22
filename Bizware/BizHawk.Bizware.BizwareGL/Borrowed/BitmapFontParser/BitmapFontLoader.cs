﻿//public domain assumed from cyotek.com

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;

namespace Cyotek.Drawing.BitmapFont
{
  // Parsing class for bitmap fonts generated by AngelCode BMFont
  // http://www.angelcode.com/products/bmfont/

  public static class BitmapFontLoader
  {
    #region  Public Class Methods

		///// <summary>
		///// Loads a bitmap font from a file, attempting to auto detect the file type
		///// </summary>
		///// <param name="fileName">Name of the file to load.</param>
		///// <returns></returns>
		//public static BitmapFont LoadFontFromFile(string fileName)
		//{
		//  BitmapFont result;

		//  if (string.IsNullOrEmpty(fileName))
		//    throw new ArgumentNullException("fileName", "File name not specified");
		//  else if (!File.Exists(fileName))
		//    throw new FileNotFoundException(string.Format("Cannot find file '{0}'", fileName), fileName);

		//  using (FileStream file = File.OpenRead(fileName))
		//  {
		//    using (TextReader reader = new StreamReader(file))
		//    {
		//      string line;

		//      line = reader.ReadLine();

		//      if (line.StartsWith("info "))
		//        result = BitmapFontLoader.LoadFontFromTextFile(fileName);
		//      else if (line.StartsWith("<?xml"))
		//        result = BitmapFontLoader.LoadFontFromXmlFile(fileName);
		//      else
		//        throw new InvalidDataException("Unknown file format.");
		//    }
		//  }

		//  return result;
		//}

    /// <summary>
    /// Loads a bitmap font from a text file.
    /// </summary>
    /// <param name="fileName">Name of the file to load.</param>
    public static BitmapFont LoadFontFromTextFile(string fileName)
    {
      BitmapFont font;
      IDictionary<int, Page> pageData;
      IDictionary<Kerning, int> kerningDictionary;
      IDictionary<char, Character> charDictionary;
      string resourcePath;
      string[] lines;

      if (string.IsNullOrEmpty(fileName))
        throw new ArgumentNullException(nameof(fileName), "File name not specified");
      else if (!File.Exists(fileName))
        throw new FileNotFoundException(string.Format("Cannot find file '{0}'", fileName), fileName);

      pageData = new SortedDictionary<int, Page>();
      kerningDictionary = new Dictionary<Kerning, int>();
      charDictionary = new Dictionary<char, Character>();
      font = new BitmapFont();

      resourcePath = Path.GetDirectoryName(fileName);
      lines = File.ReadAllLines(fileName);

      foreach (string line in lines)
      {
        string[] parts;

        parts = BitmapFontLoader.Split(line, ' ');

        if (parts.Length != 0)
        {
          switch (parts[0])
          {
            case "info":
              font.FamilyName = BitmapFontLoader.GetNamedString(parts, "face");
              font.FontSize = BitmapFontLoader.GetNamedInt(parts, "size");
              font.Bold = BitmapFontLoader.GetNamedBool(parts, "bold");
              font.Italic = BitmapFontLoader.GetNamedBool(parts, "italic");
              font.Charset = BitmapFontLoader.GetNamedString(parts, "charset");
              font.Unicode = BitmapFontLoader.GetNamedBool(parts, "unicode");
              font.StretchedHeight = BitmapFontLoader.GetNamedInt(parts, "stretchH");
              font.Smoothed = BitmapFontLoader.GetNamedBool(parts, "smooth");
              font.SuperSampling = BitmapFontLoader.GetNamedInt(parts, "aa");
              font.Padding = BitmapFontLoader.ParsePadding(BitmapFontLoader.GetNamedString(parts, "padding"));
              font.Spacing = BitmapFontLoader.ParsePoint(BitmapFontLoader.GetNamedString(parts, "spacing"));
              font.OutlineSize = BitmapFontLoader.GetNamedInt(parts, "outline");
              break;
            case "common":
              font.LineHeight = BitmapFontLoader.GetNamedInt(parts, "lineHeight");
              font.BaseHeight = BitmapFontLoader.GetNamedInt(parts, "base");
              font.TextureSize = new Size
              (
                BitmapFontLoader.GetNamedInt(parts, "scaleW"),
                BitmapFontLoader.GetNamedInt(parts, "scaleH")
              );
              font.Packed = BitmapFontLoader.GetNamedBool(parts, "packed");
              font.AlphaChannel = BitmapFontLoader.GetNamedInt(parts, "alphaChnl");
              font.RedChannel = BitmapFontLoader.GetNamedInt(parts, "redChnl");
              font.GreenChannel = BitmapFontLoader.GetNamedInt(parts, "greenChnl");
              font.BlueChannel = BitmapFontLoader.GetNamedInt(parts, "blueChnl");
              break;
            case "page":
              int id;
              string name;
              string textureId;

              id = BitmapFontLoader.GetNamedInt(parts, "id");
              name = BitmapFontLoader.GetNamedString(parts, "file");
              textureId = Path.GetFileNameWithoutExtension(name);

              pageData.Add(id, new Page(id, Path.Combine(resourcePath, name)));
              break;
            case "char":
              Character charData;

              charData = new Character
              {
                Char = (char)BitmapFontLoader.GetNamedInt(parts, "id"),
                Bounds = new Rectangle
                (
                BitmapFontLoader.GetNamedInt(parts, "x"),
                BitmapFontLoader.GetNamedInt(parts, "y"),
                BitmapFontLoader.GetNamedInt(parts, "width"),
                BitmapFontLoader.GetNamedInt(parts, "height")
                ),
                Offset = new Point
                (
                  BitmapFontLoader.GetNamedInt(parts, "xoffset"),
                  BitmapFontLoader.GetNamedInt(parts, "yoffset")
                ),
                XAdvance = BitmapFontLoader.GetNamedInt(parts, "xadvance"),
                TexturePage = BitmapFontLoader.GetNamedInt(parts, "page"),
                Channel = BitmapFontLoader.GetNamedInt(parts, "chnl")
              };
              charDictionary.Add(charData.Char, charData);
              break;
            case "kerning":
              Kerning key;

              key = new Kerning((char)BitmapFontLoader.GetNamedInt(parts, "first"), (char)BitmapFontLoader.GetNamedInt(parts, "second"), GetNamedInt(parts, "amount"));

              if (!kerningDictionary.ContainsKey(key))
                kerningDictionary.Add(key, key.Amount);
              break;
          }
        }
      }

      font.Pages = BitmapFontLoader.ToArray(pageData.Values);
      font.Characters = charDictionary;
      font.Kernings = kerningDictionary;

      return font;
    }

    /// <summary>
    /// Loads a bitmap font from an XML file.
    /// </summary>
    /// <param name="fileName">Name of the file to load.</param>
    public static BitmapFont LoadFontFromXmlFile(Stream stream)
    {
      XmlDocument document;
      BitmapFont font;
      IDictionary<int, Page> pageData;
      IDictionary<Kerning, int> kerningDictionary;
      IDictionary<char, Character> charDictionary;
      XmlNode root;
      XmlNode properties;

      document = new XmlDocument();
      pageData = new SortedDictionary<int, Page>();
      kerningDictionary = new Dictionary<Kerning, int>();
      charDictionary = new Dictionary<char, Character>();
      font = new BitmapFont();

      document.Load(stream);
      root = document.DocumentElement;

      // load the basic attributes
      properties = root.SelectSingleNode("info");
      font.FamilyName = properties.Attributes["face"].Value;
      font.FontSize = Convert.ToInt32(properties.Attributes["size"].Value);
      font.Bold = Convert.ToInt32(properties.Attributes["bold"].Value) != 0;
      font.Italic = Convert.ToInt32(properties.Attributes["italic"].Value) != 0;
      font.Unicode = Convert.ToInt32(properties.Attributes["unicode"].Value) != 0;
      font.StretchedHeight = Convert.ToInt32(properties.Attributes["stretchH"].Value);
      font.Charset = properties.Attributes["charset"].Value;
      font.Smoothed = Convert.ToInt32(properties.Attributes["smooth"].Value) != 0;
      font.SuperSampling = Convert.ToInt32(properties.Attributes["aa"].Value);
      font.Padding = BitmapFontLoader.ParsePadding(properties.Attributes["padding"].Value);
      font.Spacing = BitmapFontLoader.ParsePoint(properties.Attributes["spacing"].Value);
      font.OutlineSize = Convert.ToInt32(properties.Attributes["outline"].Value);

      // common attributes
      properties = root.SelectSingleNode("common");
      font.BaseHeight = Convert.ToInt32(properties.Attributes["lineHeight"].Value);
      font.LineHeight = Convert.ToInt32(properties.Attributes["base"].Value);
      font.TextureSize = new Size
      (
        Convert.ToInt32(properties.Attributes["scaleW"].Value),
        Convert.ToInt32(properties.Attributes["scaleH"].Value)
      );
      font.Packed = Convert.ToInt32(properties.Attributes["packed"].Value) != 0;
      font.AlphaChannel = Convert.ToInt32(properties.Attributes["alphaChnl"].Value);
      font.RedChannel = Convert.ToInt32(properties.Attributes["redChnl"].Value);
      font.GreenChannel = Convert.ToInt32(properties.Attributes["greenChnl"].Value);
      font.BlueChannel = Convert.ToInt32(properties.Attributes["blueChnl"].Value);

      // load texture information
      foreach (XmlNode node in root.SelectNodes("pages/page"))
      {
        Page page;

        page = new Page();
        page.Id = Convert.ToInt32(node.Attributes["id"].Value);
        page.FileName = node.Attributes["file"].Value;

        pageData.Add(page.Id, page);
      }
      font.Pages = BitmapFontLoader.ToArray(pageData.Values);

      // load character information
      foreach (XmlNode node in root.SelectNodes("chars/char"))
      {
        Character character;

        character = new Character();
        character.Char = (char)Convert.ToInt32(node.Attributes["id"].Value);
        character.Bounds = new Rectangle
        (
          Convert.ToInt32(node.Attributes["x"].Value),
          Convert.ToInt32(node.Attributes["y"].Value),
          Convert.ToInt32(node.Attributes["width"].Value),
          Convert.ToInt32(node.Attributes["height"].Value)
        );
        character.Offset = new Point
        (
          Convert.ToInt32(node.Attributes["xoffset"].Value),
          Convert.ToInt32(node.Attributes["yoffset"].Value)
        );
        character.XAdvance = Convert.ToInt32(node.Attributes["xadvance"].Value);
        character.TexturePage = Convert.ToInt32(node.Attributes["page"].Value);
        character.Channel = Convert.ToInt32(node.Attributes["chnl"].Value);

        charDictionary.Add(character.Char, character);
      }
      font.Characters = charDictionary;

      // loading kerning information
      foreach (XmlNode node in root.SelectNodes("kernings/kerning"))
      {
        Kerning key;

        key = new Kerning((char)Convert.ToInt32(node.Attributes["first"].Value), (char)Convert.ToInt32(node.Attributes["second"].Value), Convert.ToInt32(node.Attributes["amount"].Value));

        if (!kerningDictionary.ContainsKey(key))
          kerningDictionary.Add(key, key.Amount);
      }
      font.Kernings = kerningDictionary;

      return font;
    }

    #endregion  Public Class Methods

    #region  Private Class Methods

    /// <summary>
    /// Returns a boolean from an array of name/value pairs.
    /// </summary>
    /// <param name="parts">The array of parts.</param>
    /// <param name="name">The name of the value to return.</param>
    private static bool GetNamedBool(string[] parts, string name)
    {
      return BitmapFontLoader.GetNamedInt(parts, name) != 0;
    }

    /// <summary>
    /// Returns an integer from an array of name/value pairs.
    /// </summary>
    /// <param name="parts">The array of parts.</param>
    /// <param name="name">The name of the value to return.</param>
    private static int GetNamedInt(string[] parts, string name)
    {
      return Convert.ToInt32(BitmapFontLoader.GetNamedString(parts, name));
    }

    /// <summary>
    /// Returns a string from an array of name/value pairs.
    /// </summary>
    /// <param name="parts">The array of parts.</param>
    /// <param name="name">The name of the value to return.</param>
    private static string GetNamedString(string[] parts, string name)
    {
      string result;

      result = "";
      name = name.ToLowerInvariant();

      foreach (string part in parts)
      {
        int nameEndIndex;

        nameEndIndex = part.IndexOf("=");
        if (nameEndIndex != -1)
        {
          string namePart;
          string valuePart;

          namePart = part.Substring(0, nameEndIndex).ToLowerInvariant();
          valuePart = part.Substring(nameEndIndex + 1);

          if (namePart == name)
          {
            if (valuePart.StartsWith("\"") && valuePart.EndsWith("\""))
              valuePart = valuePart.Substring(1, valuePart.Length - 2);

            result = valuePart;
            break;
          }
        }
      }

      return result;
    }

    /// <summary>
    /// Creates a Padding object from a string representation
    /// </summary>
    /// <param name="s">The string.</param>
    private static Padding ParsePadding(string s)
    {
      string[] parts;

      parts = s.Split(',');

      return new Padding()
      {
        Left = Convert.ToInt32(parts[3].Trim()),
        Top = Convert.ToInt32(parts[0].Trim()),
        Right = Convert.ToInt32(parts[1].Trim()),
        Bottom = Convert.ToInt32(parts[2].Trim())
      };
    }

    /// <summary>
    /// Creates a Point object from a string representation
    /// </summary>
    /// <param name="s">The string.</param>
    private static Point ParsePoint(string s)
    {
      string[] parts;

      parts = s.Split(',');

      return new Point()
      {
        X = Convert.ToInt32(parts[0].Trim()),
        Y = Convert.ToInt32(parts[1].Trim())
      };
    }

    /// <summary>
    /// Splits the specified string using a given delimiter, ignoring any instances of the delimiter as part of a quoted string.
    /// </summary>
    /// <param name="s">The string to split.</param>
    /// <param name="delimiter">The delimiter.</param>
    private static string[] Split(string s, char delimiter)
    {
      string[] results;

      if (s.Contains("\""))
      {
        List<string> parts;
        int partStart;

        partStart = -1;
        parts = new List<string>();

        do
        {
          int partEnd;
          int quoteStart;
          int quoteEnd;
          bool hasQuotes;

          quoteStart = s.IndexOf("\"", partStart + 1);
          quoteEnd = s.IndexOf("\"", quoteStart + 1);
          partEnd = s.IndexOf(delimiter, partStart + 1);

          if (partEnd == -1)
            partEnd = s.Length;

          hasQuotes = quoteStart != -1 && partEnd > quoteStart && partEnd < quoteEnd;
          if (hasQuotes)
            partEnd = s.IndexOf(delimiter, quoteEnd + 1);

          parts.Add(s.Substring(partStart + 1, partEnd - partStart - 1));

          if (hasQuotes)
            partStart = partEnd - 1;

          partStart = s.IndexOf(delimiter, partStart + 1);
        } while (partStart != -1);

        results = parts.ToArray();
      }
      else
        results = s.Split(new char[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

      return results;
    }

    /// <summary>
    /// Converts the given collection into an array
    /// </summary>
    /// <typeparam name="T">Type of the items in the array</typeparam>
    /// <param name="values">The values.</param>
    private static T[] ToArray<T>(ICollection<T> values)
    {
      T[] result;

      // avoid a forced .NET 3 dependency just for one call to Linq

      result = new T[values.Count];
      values.CopyTo(result, 0);

      return result;
    }

    #endregion  Private Class Methods
  }
}
