PickHacks 2025

Winner - 3rd Place Overall

Winner - Best use of Gemini API

## Inspiration
We wanted to make gaming more accessible by providing tools that help users with vision impairments, light sensitivity, and other accessibility needs. Many games lack built-in accessibility features, so we created a customizable overlay to enhance the experience for everyone!

## What it does
- Colorblind filters through a dropdown allow users to apply filters for Protanopia, Deuteranopia, and Tritanopia.
- A brightness adjustment slider helps reduce brightness for those with light sensitivity.
- A hotkeys button provides a list of accessibility commands for quick access.
- The help menu describes all the features and how to use them.
- An always-on-top overlay ensures accessibility tools remain visible over the game.
- A nightlight filter adds a warm color tone for eye comfort during nighttime gaming.
- The Gemini AI assistant allows users to input a question and receive responses using the Gemini API.
- The overlay remains opaque until hovered over, similar to YouTube’s navigation bar.

## How we built it
We used C# with WPF to create an overlay application that applies accessibility filters without modifying the game files. The interface adjusts to the user’s needs, and we integrated the Gemini API to provide an AI-powered assistant within the overlay. 

## Challenges we ran into
When working with the backend, we had trouble using Visual Studio's native speech recognition. We then decided to use Google Cloud's speech recognition software, but unfortunately, we ran into compatibility issues. Finally, we switched to using an API with Vosk, another speech recognition software. 

## Accomplishments that we're proud of
The use of Gemini's API for real-time responses to the program's users has to be one of the best accomplishments. It allows the users to be given an answer for whatever questions they may have in no time at all. Our next accomplishment was the development and design of our UI for the overlay (add more details). 

## What we learned
We learned how to create a powerful, functioning desktop application using C#,  .NET, and WPF in Visual Studio.

## What's next for ToolBox
We would like to bring more features to the application and ask those who use the application what they would like to see added or changed.
