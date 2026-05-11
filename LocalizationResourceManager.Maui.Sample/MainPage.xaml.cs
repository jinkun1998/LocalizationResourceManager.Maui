using LocalizationResourceManager.Maui.Sample.Resources;
using System.Globalization;

namespace LocalizationResourceManager.Maui.Sample
{
    public record FoundProfile(string Name);
    public record MissingProfile();

    public partial class MainPage : ContentPage
    {
        private int count = 0;
        private string? userName;
        private object profile = new MissingProfile();
        private readonly ILocalizationResourceManager resourceManager;

        public int Count
        {
            get => count;
            set
            {
                count = value;
                OnPropertyChanged(nameof(Count));
            }
        }

        public string? UserName
        {
            get => userName;
            set
            {
                userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        public object Profile
        {
            get => profile;
            set
            {
                profile = value;
                OnPropertyChanged(nameof(Profile));
            }
        }

        public LocalizedString HelloWorld { get; }
        public LocalizedString CurrentCulture { get; }
        public LocalizedString ToggleBinding { get; }

        public MainPage(ILocalizationResourceManager resourceManager)
        {
            InitializeComponent();
            this.resourceManager = resourceManager;

            HelloWorld = new(() => $"{resourceManager["Hello"]}, {resourceManager["World"]}!");
            CurrentCulture = new(() => resourceManager.CurrentCulture.NativeName);
            ToggleBinding = new(() => $"{resourceManager["Toggle"]} Binding");

            BindingContext = this;
        }

        private void OnCounterClicked(object sender, EventArgs e) => Count++;

        private void OnDecrement(object sender, EventArgs e) => Count--;

        private void OnIncrement(object sender, EventArgs e) => Count++;

        private void OnToggleUser(object sender, EventArgs e) => UserName = UserName is null ? "Ada" : null;

        private void OnToggleFallback(object sender, EventArgs e) =>
            Profile = Profile is MissingProfile ? new FoundProfile("Bound!") : new MissingProfile();

        private void OnToggleLanguage(object sender, EventArgs e)
        {
            var languages = new List<string>() { "en", "fr", "de", "es", "sv" };
            var culture = resourceManager.CurrentCulture;
            var index = languages.IndexOf(culture.TwoLetterISOLanguageName);
            resourceManager.CurrentCulture = new CultureInfo(languages[++index < languages.Count ? index : 0]);
        }
    }
}