# TranslateDeepLXCmdPal (DeepLX Version)

> [!IMPORTANT]
> **This is a fork of [TranslateCmdPal](https://github.com/patcher454/TranslateCmdPal).**
>
> 🚧 **Work In Progress**: This project is currently under active development. Features may change, and bugs are to be expected.

[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](https://choosealicense.com/licenses/mit/)

This project is a modified version of [TranslateCmdPal](https://github.com/patcher454/TranslateCmdPal) that replaces the official DeepL API with **DeepLX** support.

Seamlessly translate text directly from the PowerToys command palette using your own DeepLX instance, free from official API key limits.

<img width="821" height="506" alt="Extension Introduce" src="https://github.com/user-attachments/assets/7b439b37-7d0e-4e09-acf9-666c097be616" />

## ✨ Features

-   **Instant Translation**: Translate text directly from the **Command Palette** (`Win` + `Alt` + `Space`).
-   **DeepLX Integration**: Uses DeepLX for unlimited translations without an official DeepL Pro/Free API key.
-   **Clipboard Ready**: The translated text is copied to your clipboard, ready to be pasted with `Ctrl + V`.
-   **Translation History**: View your past translations and copy them again directly from the **Command Palette** window.
-   **Default Language**: Set a default target language to avoid typing the language code every time.
-   **Broad Language Support**: Utilizes all languages supported by DeepL.

## ⚙️ Prerequisites

Before you begin, ensure you have the following:

1.  **PowerToys**: A set of utilities for Windows power users. [Install it from here](https://learn.microsoft.com/en-us/windows/powertoys/install).
2.  **DeepLX Instance**: You need a running DeepLX service.
    -   You can host it yourself via Docker (Recommended): [OwO-Network/DeepLX](https://github.com/OwO-Network/DeepLX).
    -   Or use a public DeepLX endpoint (if available).
    -   *Standard local endpoint example:* `http://127.0.0.1:1188/translate`

## 🚀 Installation & Setup

1.  **Install the Plugin**
    -   Download and install the latest release of this modified TranslateDeepLXCmdPal.

2.  **Configure DeepLX Endpoint**
    -   Open the plugin settings in PowerToys.
    -   Locate the **DeepLX Endpoint** field (formerly API Key).
    -   Enter your DeepLX API URL (e.g., `http://127.0.0.1:1188/translate`).
    
    *(Note: Screenshots below show the original interface, please look for the Endpoint/URL configuration in the actual settings)*
    <img width="781" height="82" alt="image" src="https://github.com/user-attachments/assets/45ded910-1c11-4f85-a1e6-c7075370e77e" /> 
    <img width="780" height="471" alt="image" src="https://github.com/user-attachments/assets/e59781b7-7155-4a4e-a9bd-ee670a9d6a9a" />

## 💡 How to Use

1.  **Open Command palette**
    -   Press `Win` + `Alt` + `Space` to launch the Command palette window.
2.  **Run TranslateDeepLXCmdPal from the list**
3.  **Enter Your Translation Query**
    -   **Format**: `{target_language_code} {text_to_translate}`
        -   **To specify a language:**
            ```
            ko hello world
            ```
        -   **To use the default language (omit the code):**
            ```
            안녕하세요!
            ```
            *(This assumes your Default Target Language is set to `en`)*

3.  **Get the Result**
    -   Press `Enter`. The translated text is copied to your clipboard for pasting with `Ctrl + V`, and the result is saved to your translation history.

## 🌐 Supported Languages and Codes

This is a list of languages supported by DeepL (via DeepLX).

| Code | Language | Code | Language |
| :--- | :--- | :--- | :--- |
| `ar` | Arabic | `it` | Italian |
| `bg` | Bulgarian | `ja` | Japanese |
| `cs` | Czech | `ko` | Korean |
| `da` | Danish | `lt` | Lithuanian |
| `de` | German | `lv` | Latvian |
| `el` | Greek | `nb` | Norwegian (Bokmål) |
| `en` | English (unspecified) | `nl` | Dutch |
| `en-gb` | English (British) | `pl` | Polish |
| `en-us` | English (American) | `pt` | Portuguese (unspecified) |
| `es` | Spanish | `pt-br` | Portuguese (Brazilian) |
| `et` | Estonian | `pt-pt` | Portuguese (non-Brazilian) |
| `fi` | Finnish | `ro` | Romanian |
| `fr` | French | `ru` | Russian |
| `hu` | Hungarian | `sk` | Slovak |
| `id` | Indonesian | `sl` | Slovenian |
| `sv` | Swedish | `tr` | Turkish |
| `uk` | Ukrainian | `zh` | Chinese (simplified) |

