# TrelloApiDemo
## 🔗 Trello API Integration

This project demonstrates automated interaction with Trello boards, lists, and cards using the official [Trello REST API](https://developer.atlassian.com/cloud/trello/rest/). It is designed for testing and learning purposes, with a focus on clean structure, secure configuration, and reproducible results.

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
```

### ✅ Test Coverage
The project includes automated tests for:

* Board creation and retrieval
* List creation within boards
* Card creation, update, and deletion
* Edge cases: empty fields, invalid IDs, partial updates

Tests are written using MSTest and are extended with DynamicData for broader coverage.

### 🚀 Running the Tests
* Clone the repository
* Add your appsettings.json file
* Run tests via Visual Studio Test Explorer or CLI:
```bash
dotnet test
``` 

### 🧰 Tools & Technologies
* C# – Core language for test implementation
* RestSharp – HTTP client for interacting with Trello API
* MSTest – Unit testing framework