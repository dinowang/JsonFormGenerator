using System.Dynamic;
using System.Text.Json;

namespace JsonFormGenerator
{
    public class Form
    {
        public string Name { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public IEnumerable<ExpandoObject> Layouts { get; set; }

        public string CreateLayout(IEnumerable<Component> components, UiStyle uiStyle, string title = null)
        {
            var layouts = new List<string>();

            foreach (var properties in Layouts.Cast<IDictionary<string, object>>())
            {
                var use = properties["use"].ToString();
                var component = components.First(c => c.Name == use);
                var template = component.CreateComponent(uiStyle, properties);

                layouts.Add(template);
            }

            var json = "{}";

            switch (uiStyle)
            {
                case UiStyle.SlackDialog:
                    json = SlackDialog(layouts, title);
                    break;
                case UiStyle.SlackBlockKit:
                    json = SlackBlockKit(layouts, title);
                    break;
                default:
                    json = AdaptiveCards(layouts, title);
                    break;
            }

            return JsonBeautifier(json);
        }

        private string SlackDialog(IEnumerable<string> layouts, string title = null)
        {
            return @$"
            {{
                ""dialog"": {{
                    ""title"": ""{title ?? Name}"",
                    ""submit_label"": ""Submit"",
                    ""callback_id"": """",
                    ""elements"": [
                        {string.Join(",", layouts)}
                    ]
                }}
            }}";
        }

        private string SlackBlockKit(IEnumerable<string> layouts, string title = null)
        {
            return @$"
            {{
                ""type"": ""modal"",
                ""submit"": {{
                    ""type"": ""plain_text"",
                    ""text"": ""Submit"",
                    ""emoji"": true
                }},
                ""close"": {{
                    ""type"": ""plain_text"",
                    ""text"": ""Cancel"",
                    ""emoji"": true
                }},
                ""title"": {{
                    ""type"": ""plain_text"",
                    ""text"": ""{title ?? Name}"",
                    ""emoji"": true
                }},                
                ""blocks"": [
                    {string.Join(",", layouts)}
                ]
            }}";
        }

        private string AdaptiveCards(IEnumerable<string> layouts, string title = null)
        {
            return @$"
            {{
                ""type"": ""AdaptiveCard"",
                ""body"": [
                    {{
                        ""type"": ""TextBlock"",
                        ""text"": ""{title ?? Name}"",
                        ""wrap"": true,
                        ""style"": ""heading""
                    }},
                    {string.Join(",", layouts)},
                    {{
                        ""type"": ""ActionSet"",
                        ""actions"": [
                            {{
                                ""type"": ""Action.Submit"",
                                ""title"": ""Submit""
                            }}
                        ]
                    }}
                ]
            }}";
        }

        private static string JsonBeautifier(string json)
        {
            // Console.WriteLine(json);

            var options = new JsonSerializerOptions { WriteIndented = true };
            var obj = JsonSerializer.Deserialize<dynamic>(json);
            return JsonSerializer.Serialize(obj, obj.GetType(), options);
        }
    }
}