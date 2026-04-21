# RealtimeChat

## Description
This is a full-stack web application built with ASP.NET Core Web API backend and a Blazor WebAssembly frontend. You can create chatrooms and message with other users in real-time. The application is designed for deployment on Azure.

## Key Features
The application includes a system for creating and managing public chatrooms. Each chatroom can be assigned a title and multiple tags, which can be searched for using the search feature. 

A user must create a password protected account with unique email, username and (not necessarily unique) full name in order to access the messaging service. 

A sentiment analysis feature is integrated into the chat feature. When a user sends a message, the text is evaluated and assigned a sentiment. This is stored with the message in the database and used to apply conditional styling to chat bubbles.

Chatroom owners have administrative privileges, which include being able to ban users, delete other user's messages, edit chatroom's title and tags in the appropriate tab in the left sidebar.

## Technical Stack
The backend is built with .NET and uses Entity Framework Core for data access, which is configured to use Microsoft SQL Server. 

Real-time functionality is achieved with the use of the Azure SignalR Service. 

The frontend is a Blazor WebAssembly application a custom AuthenticationStateProvider to manage JWT-based security.

## Installation and Local Setup
To run the project locally, follow these steps:

1. Clone the repository to your local machine.
2. Ensure a local instance of SQL Server is running.
3. Update the DefaultConnection string in the appsettings.json file within the API project to point to your local database.
4. Navigate to the API project directory and execute the command: dotnet ef database update.
5. Launch the API project using: ```dotnet run --launch-profile https```
6. Navigate to the UI project directory and execute: ```dotnet run --launch-profile https```
7. Access the application through the browser at the address specified in the terminal output.

## Azure Configuration
For Azure deployment the following services are required:

1. Azure SQL Database: Used for storing user accounts, chatrooms, and message history.
2. Azure SignalR Service: Required for handling connections.
3. Azure App Service: Used to host the Web API.
4. Azure Language Service: Required if the sentiment analysis feature is switched from the local placeholder to the real API.

## Project Structure
The repository is organized into two main folders:
- RealtimeChat.Api. Contains the controllers, database context, migrations, and SignalR hub logic.
- RealtimeChat.UI. Contains the Blazor components, shared models, and static assets. Each major component is split into a .razor file for logic and a .razor.css file for styling.