using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Unity.UIWidgets.ui;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace ConnectApp.Components.Markdown.syntax {
    public interface IConfiguration {
        IDictionary<string, Definition> Definitions { get; }
    }


    public class XmlConfiguration : IConfiguration {
        IDictionary<string, Definition> definitions;

        public IDictionary<string, Definition> Definitions {
            get { return this.GetDefinitions(); }
        }

        public XDocument XmlDocument { get; protected set; }

        public XmlConfiguration(XDocument xmlDocument) {
            if (xmlDocument == null) {
                throw new ArgumentNullException("xmlDocument");
            }

            this.XmlDocument = xmlDocument;
        }

        protected XmlConfiguration() {
        }

        IDictionary<string, Definition> GetDefinitions() {
            if (this.definitions == null) {
                this.definitions = this.XmlDocument
                    .Descendants("definition")
                    .Select(selector: this.GetDefinition)
                    .ToDictionary(x => x.Name);
            }

            return this.definitions;
        }

        Definition GetDefinition(XElement definitionElement) {
            var name = definitionElement.GetAttributeValue("name");
            var patterns = this.GetPatterns(definitionElement: definitionElement);
            var caseSensitive = bool.Parse(definitionElement.GetAttributeValue("caseSensitive"));
            var style = this.GetDefinitionStyle(definitionElement: definitionElement);

            return new Definition(name: name, caseSensitive: caseSensitive, style: style, patterns: patterns);
        }

        IDictionary<string, Pattern> GetPatterns(XContainer definitionElement) {
            var patterns = definitionElement
                .Descendants("pattern")
                .Select(selector: this.GetPattern)
                .ToDictionary(x => x.Name);

            return patterns;
        }

        Pattern GetPattern(XElement patternElement) {
            const StringComparison stringComparison = StringComparison.OrdinalIgnoreCase;
            var patternType = patternElement.GetAttributeValue("type");
            if (patternType.Equals("block", comparisonType: stringComparison)) {
                return this.GetBlockPattern(patternElement: patternElement);
            }

            if (patternType.Equals("markup", comparisonType: stringComparison)) {
                return this.GetMarkupPattern(patternElement: patternElement);
            }

            if (patternType.Equals("word", stringComparison)) {
                return this.GetWordPattern(patternElement: patternElement);
            }

            throw new InvalidOperationException(string.Format("Unknown pattern type: {0}", patternType));
        }

        BlockPattern GetBlockPattern(XElement patternElement) {
            var name = patternElement.GetAttributeValue("name");
            var style = this.GetPatternStyle(patternElement: patternElement);
            var beginsWith = patternElement.GetAttributeValue("beginsWith");
            var endsWith = patternElement.GetAttributeValue("endsWith");
            var escapesWith = patternElement.GetAttributeValue("escapesWith");

            return new BlockPattern(name: name, style: style, beginsWith: beginsWith, endsWith: endsWith,
                escapesWith: escapesWith);
        }

        MarkupPattern GetMarkupPattern(XElement patternElement) {
            var name = patternElement.GetAttributeValue("name");
            var style = this.GetPatternStyle(patternElement: patternElement);
            var highlightAttributes = bool.Parse(patternElement.GetAttributeValue("highlightAttributes"));
            var bracketColors = this.GetMarkupPatternBracketColors(patternElement: patternElement);
            var attributeNameColors = this.GetMarkupPatternAttributeNameColors(patternElement: patternElement);
            var attributeValueColors = this.GetMarkupPatternAttributeValueColors(patternElement: patternElement);

            return new MarkupPattern(name: name, style: style, highlightAttributes: highlightAttributes,
                bracketColors: bracketColors, attributeNameColors: attributeNameColors,
                attributeValueColors: attributeValueColors);
        }

        WordPattern GetWordPattern(XElement patternElement) {
            var name = patternElement.GetAttributeValue("name");
            var style = this.GetPatternStyle(patternElement: patternElement);
            var words = this.GetPatternWords(patternElement: patternElement);

            return new WordPattern(name: name, style: style, words: words);
        }

        IEnumerable<string> GetPatternWords(XContainer patternElement) {
            var words = new List<string>();
            var wordElements = patternElement.Descendants("word");
            if (wordElements != null) {
                words.AddRange(from wordElement in wordElements select Regex.Escape(str: wordElement.Value));
            }

            return words;
        }

        Style GetPatternStyle(XContainer patternElement) {
            var fontElement = patternElement.Descendants("font").Single();
            var colors = this.GetPatternColors(fontElement: fontElement);
            var font = this.GetPatternFont(fontElement: fontElement);

            return new Style(colors: colors, font: font);
        }

        ColorPair GetPatternColors(XElement fontElement) {
            var foreColor = this.ColorFromName(fontElement.GetAttributeValue("foreColor"));
            var backColor = this.ColorFromName(fontElement.GetAttributeValue("backColor"));

            return new ColorPair(foreColor: foreColor, backColor: backColor);
        }

        // A helper function to get ui.Color directly from color name
        // with the help of System.Drawing.Color
        Color ColorFromName(string name) {
            return new Color(ColorsFromName.ColorFromName(name: name));
        }

        TextStyle GetPatternFont(XElement fontElement, TextStyle defaultFont = null) {
            var fontFamily = fontElement.GetAttributeValue("name");
            if (fontFamily != null) {
                var emSize = fontElement.GetAttributeValue("size").ToSingle(11f);
                var height = fontElement.GetAttributeValue("height").ToSingle(1.46f);
                var style = Enum<FontStyle>.Parse(fontElement.GetAttributeValue("style"),
                    defaultValue: FontStyle.normal, true);

                return new TextStyle(
                    fontFamily: fontFamily,
                    fontSize: emSize,
                    fontStyle: style,
                    height: height
                );
            }

            return defaultFont;
        }

        ColorPair GetMarkupPatternBracketColors(XContainer patternElement) {
            const string descendantName = "bracketStyle";
            return this.GetMarkupPatternColors(patternElement: patternElement, descendantName: descendantName);
        }

        ColorPair GetMarkupPatternAttributeNameColors(XContainer patternElement) {
            const string descendantName = "attributeNameStyle";
            return this.GetMarkupPatternColors(patternElement: patternElement, descendantName: descendantName);
        }

        ColorPair GetMarkupPatternAttributeValueColors(XContainer patternElement) {
            const string descendantName = "attributeValueStyle";
            return this.GetMarkupPatternColors(patternElement: patternElement, descendantName: descendantName);
        }

        ColorPair GetMarkupPatternColors(XContainer patternElement, XName descendantName) {
            var fontElement = patternElement.Descendants("font").Single();
            var element = fontElement.Descendants(name: descendantName).SingleOrDefault();
            if (element != null) {
                var colors = this.GetPatternColors(fontElement: element);

                return colors;
            }

            return null;
        }

        Style GetDefinitionStyle(XNode definitionElement) {
            const string xpath = "default/font";
            var fontElement = definitionElement.XPathSelectElement(expression: xpath);
            var colors = this.GetDefinitionColors(fontElement: fontElement);
            var font = this.GetDefinitionFont(fontElement: fontElement);
            return new Style(colors: colors, font: font);
        }

        ColorPair GetDefinitionColors(XElement fontElement) {
            var foreColor = this.ColorFromName(fontElement.GetAttributeValue("foreColor"));
            var backColor = this.ColorFromName(fontElement.GetAttributeValue("backColor"));
            return new ColorPair(foreColor: foreColor, backColor: backColor);
        }

        TextStyle GetDefinitionFont(XElement fontElement) {
            var fontName = fontElement.GetAttributeValue("name");
            var fontSize = Convert.ToSingle(fontElement.GetAttributeValue("size"));
            var fontStyle = (FontStyle) Enum.Parse(typeof(FontStyle), fontElement.GetAttributeValue("style"), true);

            return new TextStyle(fontFamily: fontName, fontSize: fontSize, fontStyle: fontStyle);
        }
    }

    static class XmlExtensions {
        public static string GetAttributeValue(this XElement element, XName name) {
            if (element == null) {
                throw new ArgumentNullException("element");
            }

            var attribute = element.Attribute(name: name);
            if (attribute == null) {
                return null;
            }

            return attribute.Value;
        }
    }

    static class StringExtensions {
        public static float ToSingle(this string input, float defaultValue) {
            var result = default(float);
            if (float.TryParse(s: input, result: out result)) {
                return result;
            }

            return defaultValue;
        }
    }

    static class Enum<T> where T : struct {
        public static T Parse(string value, T defaultValue) {
            return Parse(value: value, defaultValue: defaultValue, false);
        }

        public static T Parse(string value, T defaultValue, bool ignoreCase) {
            var result = default(T);
            if (Enum.TryParse(value: value, ignoreCase: ignoreCase, result: out result)) {
                return result;
            }

            return defaultValue;
        }
    }
}