using System;
using ConnectApp.Common.Visual;
using Unity.UIWidgets.painting;

namespace ConnectApp.Components.Markdown.syntax {
    public class Highlighter {
        public IEngine Engine { get; set; }
        public IConfiguration Configuration { get; set; }

        public Highlighter() {
            this.Engine = new Engine();
            this.Configuration = new DefaultConfiguration();
        }

        public TextSpan Highlight(string definitionName, string input) {
            if (definitionName == null) {
                throw new ArgumentNullException("definitionName");
            }

            if (this.Configuration.Definitions.ContainsKey(key: definitionName)) {
                var definition = this.Configuration.Definitions[key: definitionName];
                return this.Engine.Highlight(definition: definition, input: input);
            }

            return new TextSpan(text: input, style: CTextStyle.PCodeStyle);
        }
    }
}