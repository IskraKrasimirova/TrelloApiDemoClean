# TrelloApiDemo
## 🔗 Trello API Integration

This project uses the official [Trello REST API](https://developer.atlassian.com/cloud/trello/rest/) to interact with boards, lists, and cards programmatically.

### 🛠️ Authentication

To use the API, you need to generate your own **API Key** and **Token** from Trello:

1. **Generate API Key**  
   Visit: [https://trello.com/app-key](https://trello.com/app-key)  
   Copy your personal API key.

2. **Generate Token**  
   On the same page, click the link under _“To generate a token…”_  
   Authorize access and copy your token.

> ⚠️ **Important:** Never commit your API key or token to a public repository. Use environment variables or a local `appsettings.json` file (excluded via `.gitignore`) to store credentials securely.

### 🔐 Example Configuration

Create a local `appsettings.json` file:

```json
{
  "Trello": {
    "Key": "your-key-here",
    "Token": "your-token-here",
    "BaseUrl": "https://api.trello.com/1/"
  }
}
