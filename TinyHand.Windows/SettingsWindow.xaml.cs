using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TinyHand.Windows;

public partial class SettingsWindow : Window
{
    private readonly OverlayWindow overlay;
    private readonly InputController input;
    private bool initialized;
    private readonly DispatcherTimer statusTimer;

    internal SettingsWindow(OverlayWindow overlay, InputController input)
    {
        InitializeComponent();
        this.overlay = overlay;
        this.input = input;
        HandPicker.ItemsSource = new KeyValuePair<string, string>[]
        {
            new("Lavender toy", "lavender"), new("Cursor glove", "glove"), new("Coral vinyl", "coral"),
            new("Plush", "plush"), new("Robot", "robot"), new("Astronaut", "astro"), new("Midnight", "black"),
            new("Sky", "blue"), new("Cactus", "cactus"), new("Chocolate", "choco"), new("Chrome", "chrome"),
            new("Cloud", "cloud"), new("Gingerbread", "cookie"), new("Fuzzy", "fuzzy"), new("Glass", "glass"),
            new("Grass", "grass"), new("Holo", "holo"), new("Jelly", "jelly"), new("Knit", "knit"),
            new("Lava", "lava"), new("Leaf", "leaf"), new("Lemon", "lemon"), new("Low poly", "lowpoly"),
            new("Marble", "marble"), new("Mech", "mech"), new("Mint", "mint"), new("Monster", "monster"),
            new("Neon", "neon"), new("Bubblegum", "pink"), new("Pixel", "pixel"), new("Skeleton", "skeleton"),
            new("Space", "space"), new("Terrazzo", "terrazzo"), new("Water", "water"), new("Wood", "wood"),
            new("Laser dot", "laser")
        };
        HandPicker.DisplayMemberPath = "Key";
        HandPicker.SelectedValuePath = "Value";
        HandPicker.SelectedValue = "lavender";
        initialized = true;
        overlay.EnabledChanged += UpdateToggle;
        statusTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        statusTimer.Tick += (_, _) => UpdateToggle();
        statusTimer.Start();
        Closed += (_, _) =>
        {
            statusTimer.Stop();
            overlay.EnabledChanged -= UpdateToggle;
        };
        UpdateToggle();
    }

    private void Toggle_Click(object sender, RoutedEventArgs e)
    {
        input.Toggle();
        UpdateToggle();
    }

    private void HandPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (initialized && HandPicker.SelectedValue is string name)
            overlay.SetHand(name);
    }

    private void UpdateToggle()
    {
        ToggleButton.Content = overlay.PointerActive ? "Turn pointer off" : "Turn pointer on";
    }
}
