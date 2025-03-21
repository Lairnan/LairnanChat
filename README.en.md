# LairnanChat

[Русский](README.md)

A cross-platform WebSocket-based chat built with C# (ASP.NET) and WPF. It allows users to connect to a shared or local server, exchange messages in chats, authorize, and supports plugin architecture and local server deployment.

The project is actively being developed.

Features

* WebSocket-based server connection
* User authentication and authorization
* Sending and receiving chat messages
* Support for multiple rooms
* Option to run a local chat server
* Plugin-based architecture (via LairnanChat.Plugins.Layer)
* Graphical client (WPF)
* Unit tests for code stability

Installation for use
```
Requires .NET 8 installed
Download the latest stable version
Run ChatClient.WPF.exe or the server if needed
```

Installation for development
```
Requires .NET 8 installed
Clone the repository
git clone https://github.com/Lairnan/LairnanChat.git
Open the LairnanChat.sln solution in your IDE
```

Usage
> 1. Launch the client or server
> 2. Connect to a chat server (local or external)
> 3. Authorize or register a new user
> 4. Send and receive messages
> 5. Manage connection and plugin settings

Configuration
> You can change the server address in the client settings if needed.

License
> LairnanChat is available under the MIT license. See [LICENSE](LICENSE) for details.
