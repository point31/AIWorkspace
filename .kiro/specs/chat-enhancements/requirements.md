# Requirements Document

## Introduction

This document defines requirements for a set of enhancements to the AIWorkspace WPF application. The enhancements cover five areas: a per-chat GPT model selector, file upload from the chat input, restyled AI message bubbles, a restyled provider/model ComboBox, and replacement of the Prompt Library with a Project Library that associates files and chat context to project-scoped conversations.

The application is built on C# .NET 8, WPF, CommunityToolkit.Mvvm, EF Core 8 + SQLite, and the OpenAI SDK v2.

---

## Glossary

- **Application**: The AIWorkspace WPF desktop application.
- **Chat**: A persisted conversation between the user and an AI provider, stored as a `ChatEntity` in SQLite.
- **ChatHeader**: The WPF `UserControl` (`Controls/ChatHeader.xaml`) displayed at the top of the active chat panel, containing the chat title and provider/model pickers.
- **ChatSidebar**: The sidebar listing all existing chats.
- **MessageInput**: The WPF `UserControl` (`Controls/MessageInput.xaml`) that contains the text input box and send button.
- **MessageBubble**: A rendered chat message in `MessageList.xaml`, visually styled differently for user vs. assistant roles.
- **GPT_Model**: One of the supported OpenAI model identifiers: `gpt-4o-mini`, `gpt-4o`, `gpt-o3`.
- **ModelSelector**: The new ComboBox added to `ChatHeader` that lets the user choose a GPT_Model for the active Chat.
- **ProviderComboBox**: The existing ComboBox in `ChatHeader` used to select the AI provider (OpenAI, Claude, Gemini).
- **FileAttachment**: A file the user uploads from the chat input area; its metadata is persisted in the database and the binary is stored on the local file system.
- **FileAttachmentEntity**: The new EF Core entity that stores a FileAttachment's metadata (id, chat id, file name, local path, size, uploaded timestamp).
- **FileRepository**: The repository responsible for CRUD operations on `FileAttachmentEntity`.
- **FilesView**: The existing placeholder view (`Views/FilesView.xaml`) that will be implemented to list all FileAttachments.
- **Project**: A named container that groups a set of FileAttachments and an associated Chat. Stored as a `ProjectEntity` in SQLite.
- **ProjectEntity**: The new EF Core entity for a Project (id, name, created/updated timestamps).
- **ProjectFileEntity**: The join entity linking a FileAttachment to a Project.
- **ProjectLibrary**: The new navigation page and view that replaces "Prompt Library", allowing users to manage Projects.
- **ProjectLibraryView**: The WPF view (`Views/ProjectLibraryView.xaml`) that implements the ProjectLibrary page.
- **ProjectChat**: A Chat that belongs to a Project (i.e., `ChatEntity.ProjectId` is set).
- **AIChatService**: The service (`Services/AI/AIChatService.cs`) that orchestrates message history retrieval and AI provider calls.
- **ProviderManager**: The service (`AI/ProviderManager.cs`) that resolves and configures AI provider instances.
- **PrimaryBrush**: The application's primary green colour, defined as `#2E7D32` in the resource dictionary.

---

## Requirements

### Requirement 1: GPT Model Selector

**User Story:** As a user, I want to choose between `gpt-4o-mini`, `gpt-4o`, and `gpt-o3` for each chat, so that I can control the cost and capability of the AI response without changing the global provider settings.

#### Acceptance Criteria

1. THE `ChatHeader` SHALL display a ModelSelector ComboBox containing exactly the three GPT_Model values: `gpt-4o-mini`, `gpt-4o`, and `gpt-o3`.
2. WHEN the user selects a Chat, THE `ChatHeaderViewModel` SHALL populate the ModelSelector with the GPT_Model stored on that Chat's `ChatEntity.Model` field.
3. WHEN the ProviderComboBox has `OpenAI` selected and no GPT_Model has previously been saved for the Chat, THE `ChatHeaderViewModel` SHALL default the ModelSelector selection to `gpt-4o-mini`.
4. WHEN the user changes the ModelSelector selection, THE `ChatHeaderViewModel` SHALL persist the new GPT_Model to `ChatEntity.Model` via `ChatRepository`.
5. WHEN `AIChatService` builds an AI request for a Chat whose `Provider` is `OpenAI`, THE `AIChatService` SHALL call `IAIProvider.Configure` with the GPT_Model stored on that Chat's `ChatEntity.Model` before calling `IAIProvider.SendAsync`.
6. WHILE the active Chat's provider is `OpenAI`, THE `ChatHeader` SHALL display the ModelSelector in an enabled state regardless of application load state or error conditions; WHILE the active Chat's provider is not `OpenAI`, THE `ChatHeader` SHALL display the ModelSelector in a visually disabled state so users are aware the feature exists.
7. IF `ChatEntity.Model` contains a value that is not one of the three valid GPT_Model strings, THEN THE `ChatHeaderViewModel` SHALL set the ModelSelector selection to `gpt-4o-mini`.

---

### Requirement 2: File Upload in Chat

**User Story:** As a user, I want to attach files from the chat input area, so that the files are saved to my PC and visible in the Files tab for later reference.

#### Acceptance Criteria

1. THE `MessageInput` SHALL display an attach-file button (paperclip icon or equivalent) in the input toolbar.
2. WHEN the user clicks the attach-file button, THE Application SHALL open a system file-picker dialog allowing selection of one or more files.
3. WHEN the user confirms file selection in the dialog, THE Application SHALL copy each selected file to a local storage directory under the application's data folder (e.g., `%LOCALAPPDATA%\AIWorkspace\Files\`).
4. WHEN both the file copy to local storage and the subsequent database write succeed, THE Application SHALL create a `FileAttachmentEntity` record containing the original file name, the local storage path, the file size in bytes, the associated Chat id, and the upload timestamp.
5. IF the file copy fails, THEN THE Application SHALL display an error message to the user and SHALL NOT attempt the database write; IF the database write fails after a successful file copy, THEN THE Application SHALL delete the copied file and SHALL display an error message to the user.
6. THE `FilesView` SHALL display a list of all `FileAttachmentEntity` records, showing at minimum the file name, file size, associated chat title, and upload timestamp.
7. WHEN the user selects a file entry in the `FilesView` and invokes delete, THE Application SHALL delete the `FileAttachmentEntity` record from the database AND delete the copied file from local storage.
8. IF the local file is missing at delete time, THEN THE Application SHALL still delete the `FileAttachmentEntity` record from the database and SHALL notify the user that the local file was not found.
9. THE `FileRepository` SHALL expose methods to: add a `FileAttachmentEntity`, retrieve all `FileAttachmentEntity` records, retrieve records by Chat id, and delete a `FileAttachmentEntity` by id.

---

### Requirement 3: AI Message Bubble Styling

**User Story:** As a user, I want assistant message bubbles to have a white background with dark text, so that they are clearly distinguishable from user messages and easier to read.

#### Acceptance Criteria

1. THE `MessageList` SHALL render assistant (role = `"assistant"`) MessageBubbles with a white (`#FFFFFF`) background.
2. THE `MessageList` SHALL render assistant MessageBubbles with a dark foreground text colour (`#1A1A1A` or equivalent near-black).
3. THE `MessageList` SHALL continue to render user (role = `"user"`) MessageBubbles with the existing blue (`#0E639C`) background and white text.
4. THE `MessageList` SHALL always apply a 1px light-grey border and a subtle drop shadow to assistant MessageBubbles to maintain visual separation from any window background colour, including white.

---

### Requirement 4: Provider ComboBox Styling

**User Story:** As a user, I want the provider and model selectors in the chat header to look polished and consistent with the application's green theme, so that the interface feels cohesive.

#### Acceptance Criteria

1. THE `ProviderComboBox` in `ChatHeader` SHALL be styled using the application's `PrimaryBrush` (`#2E7D32`) as its background colour in the closed (non-open) state.
2. THE `ProviderComboBox` SHALL display its selected item text in white to ensure contrast against the green background.
3. WHEN the `ProviderComboBox` dropdown is open and visible, THE dropdown panel SHALL use a dark background (near-black, explicitly excluding the `PrimaryBrush` green `#2E7D32`) consistent with the rest of the application's dark theme.
4. THE `ChatHeader` layout SHALL arrange the ProviderComboBox and the ModelSelector side-by-side on the right-hand side of the header, with consistent height and spacing.
5. THE ModelSelector ComboBox SHALL share the same visual style as the `ProviderComboBox` (green background, white text, matching height).

---

### Requirement 5: Project Library

**User Story:** As a user, I want a Project Library where I can create named projects that bundle files and a dedicated chat, so that the AI can reference my project files automatically when I ask it questions in the project chat.

#### Acceptance Criteria

1. THE `NavigationMenu` SHALL display a "Project Library" navigation item in place of the current "Prompt Library" item.
2. WHEN the user navigates to the Project Library, THE Application SHALL display the `ProjectLibraryView` showing a list of all existing Projects.
3. THE `ProjectLibraryView` SHALL allow the user to create a new Project by providing a name.
4. WHEN the user creates a Project, THE Application SHALL persist a `ProjectEntity` record to the database with the provided name and current timestamp.
5. WHEN the user creates a Project, THE Application SHALL automatically create a new Chat associated with that Project (i.e., a `ChatEntity` with its `ProjectId` set to the new Project's id).
6. THE `ProjectLibraryView` SHALL allow the user to rename an existing Project.
7. WHEN the user renames a Project, THE Application SHALL update the `ProjectEntity.Name` field in the database.
8. THE `ProjectLibraryView` SHALL allow the user to delete an existing Project.
9. WHEN the user deletes a Project, THE Application SHALL delete the `ProjectEntity` record, all associated `ProjectFileEntity` records, all associated `FileAttachmentEntity` records (and their local files), and the associated Chat (with its messages) from the database using cascade rules.
10. THE `ProjectLibraryView` SHALL allow the user to open a Project, displaying the Project's files and its associated Chat in a split or tabbed layout.
11. WHEN the user opens a Project, THE Application SHALL display all `FileAttachmentEntity` records linked to that Project and allow the user to add or remove files in the same way as Requirement 2.
12. WHEN the user sends a message in a ProjectChat, THE `AIChatService` SHALL prepend a system message containing the text content of all files attached to the Project before the chat history when calling `IAIProvider.SendAsync`.
13. IF a file attached to the Project cannot be read from local storage at send time, THEN THE `AIChatService` SHALL include a notice in the system message indicating that the file was unavailable, and SHALL continue sending the request.
14. THE `NavigationPage` enum SHALL contain a `ProjectLibrary` value replacing the `PromptLibrary` value.
15. THE `ProjectEntity` SHALL have fields: `Id` (int, PK), `Name` (string, required), `CreatedAt` (DateTime), `UpdatedAt` (DateTime).
16. THE `ChatEntity` SHALL have an optional `ProjectId` (int?, FK to `ProjectEntity`) field to associate a Chat with a Project.
