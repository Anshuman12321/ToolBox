using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NAudio.Wave;
using Vosk;

namespace Hackathon2025
{
    public partial class MainWindow : Window
    {
        private static readonly string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=AIzaSyAWyk3Mw2VxfMtAMOktdIWW8behBOioSrU";
        private string lastAnswerAndPrompt = "";
        private string helpingPrompt = " THIS PORTION OF THE PROMPT IS TO HELP GUIDE YOUR ANSWER. YOUR FUNCTION IS AS AN AI AID TO A GAMER PLAYING A VIDEO GAME ASKING A QUESTION ABOUT THE GAME. BE CONCISE AS POSSIBLE, NO LINEBREAKS. USE PREVIOUS PROMPTS AND ANSWERS GIVEN EARLIER TO MAKE AN ASSUMPTION ON WHAT THE TEXT AFTER \"NEW PROMPT:\" REFERS TO. IT MAY BE AN ANSWER TO A PREVIOUS RESPONSE YOU GAVE. BE ENTHUSIASTIC ANd SOUND HUMAN";

        private WaveInEvent waveIn;
        private bool isRecording = false;
        private Model voskModel;
        private VoskRecognizer voskRecognizer;
        private Canvas BrightnessOverlay;
        private Canvas ColorblindOverlay;
        private Canvas NightlightOverlay;
        private Button resetButton;
        private ComboBox colorblindComboBox;
        private bool isNightlightOn = false;
        private Button helpButton;
        private Grid helpOverlay;
        private Button commandsButton;
        private Grid commandsOverlay;
        private Button GeminiButton;
        private Grid GeminiOverlay;
        private StackPanel buttonPanel;
        private Border hoverZone;
        private TextBlock GeminiTextBlock; // Declare GeminiTextBlock as a member variable

        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            this.Background = Brushes.Transparent;
            this.Topmost = true;

            
            Vosk.Vosk.SetLogLevel(0);
            voskModel = new Model(@"C:\vosk-model-small-en-us-0.15"); // Replace with your model path
            voskRecognizer = new VoskRecognizer(voskModel, 16000);
            

            // Create main Grid
            Grid mainGrid = new Grid();
            this.Content = mainGrid;

            // Brightness Overlay
            BrightnessOverlay = new Canvas
            {
                Background = Brushes.Black,
                Opacity = 0,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsHitTestVisible = false

            };
            Panel.SetZIndex(BrightnessOverlay, -1);
            mainGrid.Children.Add(BrightnessOverlay);

            // Colorblind Overlay
            ColorblindOverlay = new Canvas
            {
                Background = Brushes.Transparent,
                Opacity = 1,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsHitTestVisible = false
            };
            Panel.SetZIndex(ColorblindOverlay, -1);
            mainGrid.Children.Add(ColorblindOverlay);

            // Nightlight Overlay
            NightlightOverlay = new Canvas
            {
                Background = new SolidColorBrush(Color.FromArgb(0, 255, 180, 100)), // Initially invisible
                Opacity = 0.4, // Light warm effect
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsHitTestVisible = false
            };
            Panel.SetZIndex(NightlightOverlay, -1);
            mainGrid.Children.Add(NightlightOverlay);
            // Controls Panel
            buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(30),
                Background = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                Visibility = Visibility.Collapsed,

            };

            hoverZone = new Border
            {
                Width = 750,
                Height = 40,
                Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)), // Nearly invisible but still active
                BorderBrush = Brushes.Transparent, // No visible border
                //Background = new SolidColorBrush(Color.FromArgb(80, 0, 0, 0)), // Nearly invisible but still active
                //BorderBrush = Brushes.White,
                BorderThickness = new Thickness(0), // Hide border
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(30),
                IsHitTestVisible = true // Ensures it captures mouse events
            };

            mainGrid.Children.Add(hoverZone); // Add this line after creating hoverZone


            hoverZone.MouseEnter += (s, e) => buttonPanel.Visibility = Visibility.Visible;
            buttonPanel.MouseLeave += (s, e) =>
            {
                if (!hoverZone.IsMouseOver) // Prevent immediate hiding
                {
                    buttonPanel.Visibility = Visibility.Collapsed;
                }
            };

            //Ask Gemini Button
            GeminiButton = CreateMinecraftButton("Ask Gemini");
            GeminiButton.Click += GeminiButtonClick;

            //Commands Button
            commandsButton = CreateMinecraftButton("Shortcuts");
            commandsButton.Click += CommandsButton_Click;


            // Outer Black Border
            Border outerBrightnessBorder = new Border
            {
                Background = Brushes.Black, // Outer border background
                //BorderThickness = new Thickness(2),
                //Padding = new Thickness(1), // Spacing inside the outer border
                Margin = new Thickness(7)
            };

            // Inner Light Gray Border
            Border innerBrightnessBorder = new Border
            {
                Background = Brushes.LightGray, // Inner border
                BorderThickness = new Thickness(1.6),
                Padding = new Thickness(2), // GRAY BORDER SZIE
                Margin = new Thickness(1.6) // BLACK BORDRER
            };

            // Background Panel (Dark Gray)
            Border brightnessBackground = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(100, 100, 100)), // Background color
                //BorderThickness = new Thickness(0), //ALSO GREY BORDER SIZE
                //Padding = new Thickness(1) // INSIDE GREY SPACING
            };

            // Create a StackPanel to hold the label and slider
            StackPanel brightnessPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(1)
            };

            // Create the Brightness label
            Label brightnessLabel = new Label
            {
                Content = "Brightness:",
                Foreground = Brushes.White, // Text color
                FontSize = 13,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Minecraft"),
            };

            // Create the brightness slider
            Slider brightnessSlider = new Slider
            {
                Width = 100,
                Minimum = 0,
                Maximum = 1,
                Value = 1, // Default brightness level
                Margin = new Thickness(1),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Bottom,
            };
            brightnessSlider.ValueChanged += BrightnessSlider_ValueChanged;

            // Add the label and slider to the panel
            brightnessPanel.Children.Add(brightnessLabel);
            brightnessPanel.Children.Add(brightnessSlider);

            // Nest elements: brightnessBackground -> innerBorder -> outerBorder
            brightnessBackground.Child = brightnessPanel;
            innerBrightnessBorder.Child = brightnessBackground;
            outerBrightnessBorder.Child = innerBrightnessBorder;


            // Colorblind Filter Dropdown
            colorblindComboBox = new ComboBox
            {
                Margin = new Thickness(0.5),
                Width = 130,
                Background = new SolidColorBrush(Color.FromRgb(150, 0, 0)), // Default background color
                Foreground = Brushes.Black,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2),
                IsEditable = true,  // Allows the placeholder text to be visible
                Text = "Select a filter...",
                FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Minecraft"),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontSize = 13
            };

            // Add items to the ComboBox
            colorblindComboBox.Items.Add("Protanopia");
            colorblindComboBox.Items.Add("Deuteranopia");
            colorblindComboBox.Items.Add("Tritanopia");

            // Handle selection change
            colorblindComboBox.SelectionChanged += ColorblindComboBox_SelectionChanged;

            // Reset Button
            resetButton = CreateMinecraftButton("Reset");
            resetButton.Click += ResetButton_Click;

            // Nightlight Button
            //nightlightButton = CreateMinecraftButton("Toggle Nightlight");
            //nightlightButton.Click += ToggleNightlight;

            ToggleButton NightLightToggle = CreateMinecraftToggleButton("Toggle Nightlight");
            NightLightToggle.Click += ToggleNightlight;
            NightLightToggle.Checked += ToggleButton_Checked;
            NightLightToggle.Unchecked += ToggleButton_Unchecked;

            //Help Button
            helpButton = CreateMinecraftButton("?");
            helpButton.Click += HelpButton_Click;

            // Add controls to panel
            buttonPanel.Children.Add(GeminiButton);
            buttonPanel.Children.Add(outerBrightnessBorder);
            buttonPanel.Children.Add(colorblindComboBox);
            buttonPanel.Children.Add(NightLightToggle);
            buttonPanel.Children.Add(resetButton);
            buttonPanel.Children.Add(commandsButton);
            buttonPanel.Children.Add(helpButton);

            // Add UI elements to Grid
            mainGrid.Children.Add(buttonPanel);



            // Help Overlay
            helpOverlay = new Grid
            {
                Background = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)),
                Visibility = Visibility.Collapsed,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,

                Width = 500,
                Height = 260,
                Margin = new Thickness(10),
            };


            TextBlock helpTextBlock = new TextBlock
            {
                Text = "Help:\n\n" +
                       "• Ask Gemini: Click to record and ask geneimi and game related questions. \n" +
                       "• Brightness: Slide to make screen brighter or darker.\n" +
                       "• Select a Filter: Choose from a dropdown menu to apply a selection of color-blind accommodating filters.\n" +
                       "• Toggle Nightlight: Toggles a night shift filter to make night viewing easier.\n" +
                       "• Reset: Clears all filter overlays.\n" +
                       "• Shortcuts: Display common windows accessibility keyboard shortcuts.\n" +
                       "• ?: Display this help message.",

                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Foreground = Brushes.Black,
                Padding = new Thickness(10),
            };

            Button closeHelpButton = CreateMinecraftButton("Close");
            closeHelpButton.Click += (s, e) => helpOverlay.Visibility = Visibility.Collapsed;

            StackPanel helpPanel = new StackPanel();
            helpPanel.Children.Add(helpTextBlock);
            helpPanel.Children.Add(closeHelpButton);
            helpOverlay.Children.Add(helpPanel);
            mainGrid.Children.Add(helpOverlay);

            // Commands Overlay
            commandsOverlay = new Grid
            {
                Background = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)),
                Visibility = Visibility.Collapsed,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 450,
                Height = 220,
                Margin = new Thickness(10),
            };

            // Gemini Overlay - Allow dynamic sizing
            GeminiOverlay = new Grid
            {
                Background = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)),
                Visibility = Visibility.Collapsed,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(10),
                MinWidth = 200, // Prevents it from being too small
                MinHeight = 100, // Prevents it from being too small
            };

            // Make the text block auto-expand inside GeminiOverlay
            GeminiTextBlock = new TextBlock
            {
                Text = "Start Speaking! Gemini Output Will Appear Here...",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Foreground = Brushes.Black,
                Padding = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                MaxWidth = 400, // Prevents it from growing too wide
            };


            TextBlock commandsTextBlock = new TextBlock
            {
                Text = "Helpful Accessibility Windows Shortcuts:\n\n" +
                       "• Windows + U – Open Ease of Access settings\n" +
                       "• Windows + Ctrl + Enter – Toggle Narrator (screen reader)\n" +
                       "• Caps Lock + Esc – Exit Narrator2\n" +
                       "• Windows + Plus (+)/Windows + Minus (-) - Zoom In / Zoom Out \n" +
                       "• Left Alt + Left Shift + Print Screen – Toggle High Contrast mode\n" +
                       "• Windows + Ctrl + O – Open On-Screen Keyboard",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Foreground = Brushes.Black,
                Padding = new Thickness(10),
            };


            Button closeCommandsButton = CreateMinecraftButton("Close");
            closeCommandsButton.Click += (s, e) => commandsOverlay.Visibility = Visibility.Collapsed;

            StackPanel commandsPanel = new StackPanel();
            commandsPanel.Children.Add(commandsTextBlock);
            commandsPanel.Children.Add(closeCommandsButton);
            commandsOverlay.Children.Add(commandsPanel);
            mainGrid.Children.Add(commandsOverlay);


            Button closeGeminiButton = CreateMinecraftButton("Close");
            closeGeminiButton.Click += (s, e) => GeminiOverlay.Visibility = Visibility.Collapsed;

            StackPanel GeminiPanel = new StackPanel
            {
                Orientation = Orientation.Vertical, 
            };
            GeminiPanel.Children.Add(GeminiTextBlock);
            GeminiPanel.Children.Add(closeGeminiButton);
            GeminiOverlay.Children.Add(GeminiPanel);
            mainGrid.Children.Add(GeminiOverlay);
        }

        private Button CreateMinecraftButton(string text)
        {
            Button button = new Button
            {
                Content = text,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Color.FromRgb(100, 100, 100)), // Default dark gray background
                Foreground = Brushes.White,
                //FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Minecraft"),
                BorderThickness = new Thickness(0), // We'll use custom borders instead
                Padding = new Thickness(10),
                OverridesDefaultStyle = true // Ensures WPF doesn't apply default styles
            };

            // Create Custom Control Template
            ControlTemplate template = new ControlTemplate(typeof(Button));

            // Outer Black Border
            FrameworkElementFactory outerBorder = new FrameworkElementFactory(typeof(Border));
            outerBorder.SetValue(Border.BackgroundProperty, Brushes.Black);
            outerBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1));

            // Inner Light Gray Border
            FrameworkElementFactory innerBorder = new FrameworkElementFactory(typeof(Border));
            innerBorder.SetValue(Border.BackgroundProperty, Brushes.LightGray);
            innerBorder.SetValue(Border.BorderThicknessProperty, new Thickness(2));
            innerBorder.SetValue(MarginProperty, new Thickness(0.7)); // Space between outer and inner

            // Button Background (Dark Gray, changes on hover)
            FrameworkElementFactory buttonBackground = new FrameworkElementFactory(typeof(Border));
            buttonBackground.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { Source = button });
            buttonBackground.SetValue(Border.BorderThicknessProperty, new Thickness(0));
            buttonBackground.SetValue(MarginProperty, new Thickness(1.6)); // Space between inner border and content

            // ContentPresenter (Text inside button)
            FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            contentPresenter.SetValue(MarginProperty, new Thickness(7)); // Space between inner border and content


            // Nest elements properly
            buttonBackground.AppendChild(contentPresenter);
            innerBorder.AppendChild(buttonBackground);
            outerBorder.AppendChild(innerBorder);
            template.VisualTree = outerBorder;

            // Mouse Hover Events (only change inner background)
            button.MouseEnter += (s, e) =>
            {
                button.Background = new SolidColorBrush(Color.FromRgb(0, 128, 0)); // Dark Green (inner background)
                button.Foreground = Brushes.Yellow;
                button.FontWeight = FontWeights.Bold;
            };

            button.MouseLeave += (s, e) =>
            {
                button.Background = new SolidColorBrush(Color.FromRgb(100, 100, 100)); // Restore Dark Gray
                button.Foreground = Brushes.White;
                button.FontWeight = FontWeights.Normal;
            };

            button.Template = template;
            return button;
        }


        private ToggleButton CreateMinecraftToggleButton(string text)
        {
            ToggleButton toggleButton = new ToggleButton
            {
                Content = text,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Color.FromRgb(100, 100, 100)), // Default dark gray
                Foreground = Brushes.White,
                FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Minecraft"),
                BorderThickness = new Thickness(0), // Custom borders instead
                Padding = new Thickness(10),
                OverridesDefaultStyle = true
            };

            // Custom Control Template
            ControlTemplate template = new ControlTemplate(typeof(ToggleButton));

            // Outer Black Border
            FrameworkElementFactory outerBorder = new FrameworkElementFactory(typeof(Border));
            outerBorder.SetValue(Border.BackgroundProperty, Brushes.Black);
            outerBorder.SetValue(Border.BorderThicknessProperty, new Thickness(2));

            // Inner Light Gray Border
            FrameworkElementFactory innerBorder = new FrameworkElementFactory(typeof(Border));
            innerBorder.SetValue(Border.BackgroundProperty, Brushes.LightGray);
            innerBorder.SetValue(Border.BorderThicknessProperty, new Thickness(2));
            innerBorder.SetValue(MarginProperty, new Thickness(1));

            // Background (Dark Gray)
            FrameworkElementFactory buttonBackground = new FrameworkElementFactory(typeof(Border));
            buttonBackground.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { Source = toggleButton });
            buttonBackground.SetValue(Border.BorderThicknessProperty, new Thickness(0));
            buttonBackground.SetValue(MarginProperty, new Thickness(1.6));

            // Text Content
            FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            contentPresenter.SetValue(MarginProperty, new Thickness(7));

            // Nest elements
            buttonBackground.AppendChild(contentPresenter);
            innerBorder.AppendChild(buttonBackground);
            outerBorder.AppendChild(innerBorder);
            template.VisualTree = outerBorder;

            // Apply template
            toggleButton.Template = template;

            // Mouse Hover Effects
            toggleButton.MouseEnter += (s, e) =>
            {
                toggleButton.Background = new SolidColorBrush(Color.FromRgb(0, 128, 0)); // Dark Green
                toggleButton.Foreground = Brushes.Yellow;
                toggleButton.FontWeight = FontWeights.Bold;
            };

            toggleButton.MouseLeave += (s, e) =>
            {
                if (toggleButton.IsChecked == true)
                {
                    toggleButton.Background = new SolidColorBrush(Color.FromRgb(0, 168, 0)); // Stay Green
                    toggleButton.Foreground = Brushes.Yellow;
                    toggleButton.FontWeight = FontWeights.Normal;
                }
                else
                {
                    toggleButton.Background = new SolidColorBrush(Color.FromRgb(100, 100, 100)); // Restore Dark Gray
                    toggleButton.Foreground = Brushes.White;
                }
                toggleButton.FontWeight = FontWeights.Normal;
            };

            // Toggle Behavior (Checked/Unchecked)
            toggleButton.Checked += (s, e) =>
            {
                toggleButton.Background = new SolidColorBrush(Color.FromRgb(0, 128, 0)); // Dark Green
                toggleButton.Foreground = Brushes.Yellow;
            };

            toggleButton.Unchecked += (s, e) =>
            {
                toggleButton.Background = new SolidColorBrush(Color.FromRgb(100, 100, 100)); // Restore Dark Gray
                toggleButton.Foreground = Brushes.White;
            };

            return toggleButton;
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton)
            {
                //MessageBox.Show($"{toggleButton.Content} is ON", "Toggle State");
            }
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton)
            {
                // MessageBox.Show($"{toggleButton.Content} is OFF", "Toggle State");
            }
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (BrightnessOverlay != null)
            {
                BrightnessOverlay.Opacity = 1 - e.NewValue;
            }
        }

        private void ApplyColorblindFilterEffect(string filterType)
        {
            if (filterType == "Protanopia")
            {
                ColorblindOverlay.Background = new SolidColorBrush(Color.FromArgb(10, 255, 150, 15));
            }
            else if (filterType == "Deuteranopia")
            {
                ColorblindOverlay.Background = new SolidColorBrush(Color.FromArgb(10, 150, 255, 150));
            }
            else if (filterType == "Tritanopia")
            {
                ColorblindOverlay.Background = new SolidColorBrush(Color.FromArgb(10, 150, 150, 255));
            }
            else
            {
                ColorblindOverlay.Background = Brushes.Transparent;
            }
        }

        private void ColorblindComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (colorblindComboBox.SelectedItem is string selectedFilter)
            {
                ApplyColorblindFilterEffect(selectedFilter);
            }
        }

        private void ToggleNightlight(object sender, RoutedEventArgs e)
        {
            isNightlightOn = !isNightlightOn;

            if (isNightlightOn)
            {
                NightlightOverlay.Background = new SolidColorBrush(Color.FromArgb(100, 255, 180, 100));
                NightlightOverlay.Visibility = Visibility.Visible;
            }
            else
            {
                NightlightOverlay.Visibility = Visibility.Collapsed;
                NightlightOverlay.Background = Brushes.Transparent; // Ensure fully transparent background
            }

            NightlightOverlay.IsHitTestVisible = false;
            Panel.SetZIndex(NightlightOverlay, -1);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            NightlightOverlay.Background = Brushes.Transparent;
            BrightnessOverlay.Opacity = 0;
            ColorblindOverlay.Background = Brushes.Transparent;
            //MessageBox.Show("Filters reset to default.", "Reset");
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            helpOverlay.Visibility = Visibility.Visible;
        }

        private void CommandsButton_Click(object sender, RoutedEventArgs e)
        {
            commandsOverlay.Visibility = Visibility.Visible;
        }

        private void GeminiButtonClick(object sender, RoutedEventArgs e)
        {
            GeminiOverlay.Visibility = Visibility.Visible;
            GeminiButton.Content = "Listening...";
            StartGeminiProcess(); // Call your Gemini process function
        }

        private void StartGeminiProcess()
        {
            // Simulate Gemini process (replace with your actual logic)
            // Example

                startRecordingFunc();
        }

        private static async Task<string> SendPromptToGemini(string prompt)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text = prompt
                                }
                            }
                        }
                    }
                };

                string jsonRequestBody = JsonSerializer.Serialize(requestBody);
                StringContent content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(ApiUrl, content);
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                JsonDocument jsonDocument = JsonDocument.Parse(responseContent);

                if (jsonDocument.RootElement.TryGetProperty("candidates", out JsonElement candidates) &&
                    candidates.GetArrayLength() > 0 &&
                    candidates[0].TryGetProperty("content", out JsonElement contentElement) &&
                    contentElement.TryGetProperty("parts", out JsonElement parts) &&
                    parts.GetArrayLength() > 0 &&
                    parts[0].TryGetProperty("text", out JsonElement text))
                {
                    return text.GetString();
                }
                else
                {
                    throw new Exception("Failed to parse Gemini API response.");
                }
            }
        }
        private void startRecordingFunc()
        {
            if (!isRecording)
            {
                waveIn = new WaveInEvent();
                waveIn.DeviceNumber = 0;
                waveIn.WaveFormat = new WaveFormat(16000, 1);
                waveIn.BufferMilliseconds = 1000;

                waveIn.DataAvailable += async (s, p) =>
                {
                    if (voskRecognizer.AcceptWaveform(p.Buffer, p.BytesRecorded))
                    {
                        string resultFromVosk = voskRecognizer.Result();
                        string spokenText = "";
                        for (int i = 14; resultFromVosk[i] != '"'; i++)
                            spokenText += resultFromVosk[i];
                        string response = await SendPromptToGemini(lastAnswerAndPrompt + "This is the new prompt: " + spokenText + helpingPrompt + " VERY IMPORTANT: THIS IS A TRANSCRIBED AUDIO, make a reasonable assumption that new prompt is a question");
                        //AppendOutput(response);
                        lastAnswerAndPrompt += "Prompt Given: " + spokenText + " Answer Given: " + response;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            GeminiTextBlock.Text = response;
                        });
                        StopRecording();
                    }
                };

                waveIn.StartRecording();
                isRecording = true;
            }
            else
            {
                StopRecording();
            }
        }

        private void StopRecording()
        {
            if (waveIn != null) // Check if waveIn is assigned
            {
                waveIn.StopRecording();
                waveIn.Dispose();
                waveIn = null;
                //AppendOutput("Recording stopped.");
                isRecording = false;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    GeminiButton.Content = "Ask Gemini";
                });
            }
            else
            {
                //AppendOutput("No recording in progress.");
            }
        }
    }
}
