using Microsoft.Maui.Controls.Shapes;

namespace MauiApp2.Pages;

public partial class FaqPage : ContentPage
{
    private readonly List<FaqItem> _faqItems = new()
    {
        new FaqItem
        {
            Question = "How do I discover new music?",
            Answer = "Simply open the app and start scrolling through the feed! Each swipe reveals a new song. Songs are automatically played when they appear on screen, making discovery effortless and intuitive."
        },
        new FaqItem
        {
            Question = "How do I like a song?",
            Answer = "Tap the heart icon on the right side of the song info while viewing it in the feed. The heart will turn blue when liked. You can also unlike songs by tapping the heart again."
        },
        new FaqItem
        {
            Question = "Where can I find my liked songs?",
            Answer = "All your liked songs are saved in the 'Liked Songs' section, accessible via the heart icon in the bottom navigation bar. You can play, filter, and manage your collection there."
        },
        new FaqItem
        {
            Question = "How do I filter songs by genre?",
            Answer = "In the Home feed, tap the genre filter button in the top right corner. Select your preferred genre from the dropdown menu. You can also filter liked songs by genre in the Liked Songs page."
        },
        new FaqItem
        {
            Question = "What do the stats show?",
            Answer = "The Stats page displays your total listening time, number of liked songs, and your top genres based on liked songs. This helps you understand your music preferences and listening habits."
        },
        new FaqItem
        {
            Question = "How do I connect music streaming services?",
            Answer = "Go to the Account page and toggle the switches next to Spotify, SoundCloud, or Apple Music. Note: This feature is coming soon and the toggles are currently for demonstration purposes."
        },
        new FaqItem
        {
            Question = "Is my data stored locally?",
            Answer = "Yes! All your data including liked songs, listening history, and preferences are stored locally on your device. No account or internet connection is required for basic functionality."
        }
    };

    public FaqPage()
    {
        InitializeComponent();
        CreateFaqItems();
    }

    private void CreateFaqItems()
    {
        foreach (var faq in _faqItems)
        {
            var faqView = CreateFaqItemView(faq);
            FaqContainer.Children.Add(faqView);
        }
    }

    private View CreateFaqItemView(FaqItem faq)
    {
        var border = new Border
        {
            BackgroundColor = Color.FromArgb("#16181B"),
            StrokeThickness = 0,
            Padding = new Thickness(15)
        };
        border.StrokeShape = new RoundRectangle { CornerRadius = 12 };

        var mainStack = new VerticalStackLayout { Spacing = 0 };

        // Question row with expand icon
        var questionGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var questionLabel = new Label
        {
            Text = faq.Question,
            FontFamily = "RobotoMedium",
            FontSize = 15,
            TextColor = Colors.White,
            VerticalOptions = LayoutOptions.Center
        };

        var expandIcon = new Label
        {
            Text = "▼",
            FontSize = 12,
            TextColor = Color.FromArgb("#2A7FE6"),
            VerticalOptions = LayoutOptions.Center
        };

        questionGrid.Children.Add(questionLabel);
        Grid.SetColumn(questionLabel, 0);
        questionGrid.Children.Add(expandIcon);
        Grid.SetColumn(expandIcon, 1);

        // Answer (hidden by default)
        var answerLabel = new Label
        {
            Text = faq.Answer,
            FontFamily = "RobotoMedium",
            FontSize = 13,
            TextColor = Color.FromArgb("#9CA3AF"),
            LineHeight = 1.4,
            Margin = new Thickness(0, 10, 0, 0),
            IsVisible = false
        };

        mainStack.Children.Add(questionGrid);
        mainStack.Children.Add(answerLabel);

        border.Content = mainStack;

        // Tap handler for accordion
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) =>
        {
            answerLabel.IsVisible = !answerLabel.IsVisible;
            expandIcon.Text = answerLabel.IsVisible ? "▲" : "▼";
        };
        border.GestureRecognizers.Add(tapGesture);

        return border;
    }

    private async void OnBackClicked(object? sender, TappedEventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}

public class FaqItem
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}

