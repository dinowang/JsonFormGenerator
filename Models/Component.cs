using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace JsonFormGenerator
{
    public class Component
    {
        public string Name { get; set; }
        public string Slack { get; set; }
        public string SlackBlockKit { get; set; }
        public string AdaptiveCards { get; set; }

        public string CreateComponent(UiStyle uiStyle, IDictionary<string, object> properties)
        {
            var template = string.Empty;

            switch (uiStyle)
            {
                case UiStyle.SlackDialog:
                    template = Slack;
                    break;
                case UiStyle.SlackBlockKit:
                    template = SlackBlockKit;
                    break;
                default:
                    template = AdaptiveCards;
                    break;
            }

            template = Regex.Replace(template, @"\$\{([^:\}]+):([^}]+)\}", match =>
            {
                var cmd = match.Groups[1].Value;
                var key = match.Groups[2].Value;

                switch (cmd)
                {
                    case "select":
                        var availableOptions = properties[key] as IEnumerable<dynamic>;

                        var options = availableOptions
                                            .Cast<IDictionary<object, object>>()
                                            .Select(x => new
                                            {
                                                value = x["value"],
                                                text = x["text"]
                                            });

                        var json = "[]";

                        switch (uiStyle)
                        {
                            case UiStyle.SlackDialog:
                                json = JsonSerializer.Serialize(options.Select(x => new { label = x.text, value = x.value }));
                                break;

                            case UiStyle.SlackBlockKit:
                                json = JsonSerializer.Serialize(options.Select(x => new { text = new { type = "plain_text", text = x.text, emoji = true }, value = x.value }));
                                break;

                            case UiStyle.AdaptiveCards:
                                json = JsonSerializer.Serialize(options.Select(x => new { title = x.text, value = x.value }));
                                break;
                        }
                        return json;
                }

                return "[]";
            });


            template = Regex.Replace(template, @"\$\{([^}]+)\}", match =>
            {
                var key = match.Groups[1].Value;

                if (properties.TryGetValue(key, out var value))
                    return value.ToString();

                return match.Value;
            });

            return template;
        }
    }
}