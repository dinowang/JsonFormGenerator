namespace JsonFormGenerator
{
    public class Resources
    {
        public IEnumerable<Component> Components { get; set; }
        public IEnumerable<Form> Forms { get; set; }
    }

    public class QuickAccessResource
    {
        public static QuickAccessResource Create(Resources resources) => new QuickAccessResource
        {
            Components = resources
                            .Components
                            .Where(x => !string.IsNullOrEmpty(x.Name))
                            .ToDictionary(x => x.Name, x => x),
            Forms = resources
                            .Forms
                            .Where(x => !string.IsNullOrEmpty(x.Name))
                            .ToDictionary(x => x.Name, x => x)
        };

        public IDictionary<string, Component> Components { get; set; }
        public IDictionary<string, Form> Forms { get; set; }
    }
}
