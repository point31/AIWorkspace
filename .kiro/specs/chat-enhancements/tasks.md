# Implementation Plan: Chat Enhancements

## Overview

Implements five enhancements to the AIWorkspace WPF application in C# (.NET 8): a per-chat GPT model selector, file upload from the chat input, restyled AI message bubbles, a restyled provider/model ComboBox, and a Project Library replacing the Prompt Library. The plan follows the layered MVVM architecture (Views → ViewModels → Services → Repositories → EF Core → SQLite).

## Tasks

- [ ] 1. Database migrations and new entities
  - [ ] 1.1 Create `ProjectEntity` and scaffold `AddProjects` EF Core migration
    - Add `Entities/ProjectEntity.cs` with `Id`, `Name`, `CreatedAt`, `UpdatedAt`, `Chat` navigation, and `Files` collection
    - Add `int? ProjectId` and `ProjectEntity? Project` navigation to `Entities/ChatEntity.cs`
    - Configure `HasOne<ChatEntity>…OnDelete(Cascade)` and related fluent API in `Data/AppDbContext.cs`
    - Scaffold migration `AddProjects` via `dotnet ef migrations add`
    - _Requirements: 5.15, 5.16_

  - [ ] 1.2 Create `FileAttachmentEntity` and scaffold `AddFileAttachments` EF Core migration
    - Add `Entities/FileAttachmentEntity.cs` with all fields including optional `ProjectId` FK
    - Configure both FK relationships (`Chat` → Restrict, `Project` → Cascade) in `AppDbContext`
    - Scaffold migration `AddFileAttachments`
    - _Requirements: 2.4, 5.16_

  - [ ] 1.3 Add `Model` field to `ChatEntity` and update `ChatModel`
    - Add `string Model { get; set; } = ""` to `Entities/ChatEntity.cs`
    - Add `[ObservableProperty] private string model = ""` and `[ObservableProperty] private int? projectId` to `Models/ChatModels.cs`
    - Scaffold migration `AddChatModel`
    - _Requirements: 1.2, 1.4_

- [ ] 2. Repository layer
  - [ ] 2.1 Add `UpdateModelAsync` to `ChatRepository`
    - Implement `Task UpdateModelAsync(int chatId, string model)` in `Repositories/ChatRepository.cs`
    - _Requirements: 1.4_

  - [ ] 2.2 Implement `FileRepository`
    - Create `Repositories/FileRepository.cs` implementing all five methods: `AddAsync`, `GetAllAsync`, `GetByChatAsync`, `GetByProjectAsync`, `DeleteAsync`
    - Register in `Infrastructure/HostBuilder.cs`
    - _Requirements: 2.9_

  - [ ] 2.3 Implement `ProjectRepository`
    - Create `Repositories/ProjectRepository.cs` implementing `CreateAsync`, `GetAllAsync`, `GetAsync`, `UpdateNameAsync`, `DeleteAsync`
    - Register in `Infrastructure/HostBuilder.cs`
    - _Requirements: 5.4, 5.6, 5.8, 5.9_

  - [ ]* 2.4 Write property tests for `FileRepository` (Properties 6, 7, 8)
    - **Property 6: File upload entity has correct metadata** — for any file path × size, `AddAsync` round-trip returns entity with matching `FileName`, `LocalPath`, `FileSize`, `ChatId`
    - **Property 7: FilesViewModel exposes all persisted records** — for any list of `FileAttachmentEntity` records, `GetAllAsync` returns one entry per entity
    - **Property 8: Delete removes DB record** — after `DeleteAsync`, entity is absent from `GetAllAsync`
    - Use SQLite in-memory with FK enforcement; tag comments with `// Feature: chat-enhancements, Property N`
    - _Requirements: 2.4, 2.6, 2.7_

  - [ ]* 2.5 Write property tests for `ProjectRepository` (Properties 9, 10, 11)
    - **Property 9: Project name persisted accurately** — for any non-empty string, `CreateAsync` and `UpdateNameAsync` store the exact string
    - **Property 10: Creating a project auto-creates chat** — after `CreateAsync` + chat creation, exactly one `ChatEntity` has the new project's `Id`
    - **Property 11: Project cascade delete removes all related data** — after `DeleteAsync`, zero rows remain in `Chats`, `FileAttachments`, and `Messages` for that project
    - _Requirements: 5.4, 5.5, 5.7, 5.9_

- [ ] 3. File service
  - [ ] 3.1 Implement `IFileService` and `FileService`
    - Create `Services/IFileService.cs` interface with `CopyToStorageAsync`, `DeleteFromStorageAsync`, `ReadText`
    - Create `Services/FileService.cs`; storage root `%LOCALAPPDATA%\AIWorkspace\Files\`
    - Register in `Infrastructure/HostBuilder.cs`
    - _Requirements: 2.3, 5.12, 5.13_

  - [ ]* 3.2 Write unit tests for `FileService`
    - Test path construction generates a path inside the storage root
    - Test storage directory is created if absent
    - Use a real temp directory; clean up in test teardown
    - _Requirements: 2.3_

- [ ] 4. GPT Model Selector – ViewModel and service
  - [ ] 4.1 Update `ChatHeaderViewModel` for model selection
    - Add `IReadOnlyList<string> AvailableModels` = `["gpt-4o-mini", "gpt-4o", "gpt-o3"]`
    - Add `[ObservableProperty] private string selectedModel` with validation/fallback in `OnSelectedModelChanged`
    - Add `[ObservableProperty] private bool isModelSelectorEnabled` computed from `SelectedProvider == OpenAI`
    - Extend `Receive(ChatSelectedMessage)` to read and validate `Model` from the incoming chat
    - Persist changes via `ChatRepository.UpdateModelAsync`
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.6, 1.7_

  - [ ]* 4.2 Write property tests for `ChatHeaderViewModel` model selection (Properties 1, 2, 3, 5)
    - **Property 1: Valid model is reflected in SelectedModel** — for each of the three valid strings, `Receive(ChatSelectedMessage)` sets `SelectedModel` to that string
    - **Property 2: Invalid model falls back to gpt-4o-mini** — for any arbitrary string not in the valid set, `SelectedModel` becomes `"gpt-4o-mini"`
    - **Property 3: Model change triggers persistence** — for any valid model string, `OnSelectedModelChanged` calls `ChatRepository.UpdateModelAsync` with the exact string
    - **Property 5: ModelSelector enabled iff OpenAI** — for all `ProviderType` values, `IsModelSelectorEnabled == (provider == OpenAI)`
    - Use `Moq` for `ChatRepository` and `IMessenger`
    - _Requirements: 1.2, 1.3, 1.4, 1.6, 1.7_

  - [ ] 4.3 Update `AIChatService` to pass per-chat model to provider
    - In `AskAsync`, after resolving the provider and before `SendAsync`, call `provider.Configure(chat.ApiKey, chat.Model)` for OpenAI chats
    - _Requirements: 1.5_

  - [ ]* 4.4 Write property test for `AIChatService` model forwarding (Property 4)
    - **Property 4: AIChatService passes per-chat model to provider** — for any OpenAI chat with any valid model string and any non-empty message history, `Configure` is called with that exact model string before `SendAsync`
    - Use `Moq` for `IAIProvider`
    - _Requirements: 1.5_

- [ ] 5. Checkpoint — Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 6. File Upload – MessageInput integration
  - [ ] 6.1 Update `MessageInputViewModel` with `AttachFileCommand`
    - Inject `IFileService` and `FileRepository` into `MessageInputViewModel`
    - Add `AttachFileCommand`: opens `OpenFileDialog` (multi-select), copies each file via `IFileService.CopyToStorageAsync`, persists `FileAttachmentEntity`, sends `FileAttachedMessage`
    - Add `[ObservableProperty] private string errorMessage` for inline error display
    - Handle error cases: file copy failure (do not write DB), DB failure (delete copied file), per Requirements 2.5
    - _Requirements: 2.2, 2.3, 2.4, 2.5_

  - [ ] 6.2 Add attach-file button to `MessageInput.xaml`
    - Add paperclip `Button` left of the existing text input, bound to `AttachFileCommand`
    - Add inline error `TextBlock` bound to `ErrorMessage`, collapsed when empty
    - _Requirements: 2.1_

  - [ ]* 6.3 Write property test for file upload metadata (Property 6 – ViewModel side)
    - **Property 6: File upload produces a correctly populated entity** — for any file path × size, the entity created by `AttachFileCommand` has `FileName`, `LocalPath`, `FileSize`, `ChatId` matching the source file
    - Use a real temp directory; mock `FileRepository` to capture the written entity
    - _Requirements: 2.3, 2.4_

- [ ] 7. Files View implementation
  - [ ] 7.1 Implement `FilesViewModel` and `FilesView.xaml`
    - Implement `FilesViewModel` loading all records from `FileRepository.GetAllAsync` into `ObservableCollection<FileAttachmentModel>`
    - Add `FileAttachmentModel` to `Models/` with `FileSizeDisplay` computed property
    - Implement `DeleteFileCommand`: delete DB record via `FileRepository.DeleteAsync`, delete local file via `IFileService.DeleteFromStorageAsync`; show info message if local file is missing
    - Implement `FilesView.xaml` displaying `FileName`, `FileSizeDisplay`, `ChatTitle`, `UploadedAt` per row with a delete button
    - _Requirements: 2.6, 2.7, 2.8_

  - [ ]* 7.2 Write property test for `FilesViewModel` file list completeness (Property 7)
    - **Property 7: FilesViewModel exposes all persisted records** — for any set of `FileAttachmentEntity` records, `Files` contains exactly one `FileAttachmentModel` per entity with matching `FileName`, `FileSize`, `ChatTitle`, `UploadedAt`
    - _Requirements: 2.6_

  - [ ]* 7.3 Write property test for `FilesViewModel` delete (Property 8)
    - **Property 8: Delete removes both DB record and local file** — for any entity with an existing `LocalPath`, invoking `DeleteFileCommand` removes the entity and the file
    - _Requirements: 2.7_

- [ ] 8. AI Message Bubble Styling
  - [ ] 8.1 Restyle assistant `MessageBubble` in `MessageList.xaml`
    - Set default `Background="White"`, `Foreground="#1A1A1A"`, `BorderBrush="#DDDDDD"`, `BorderThickness="1"`, and a `DropShadowEffect` (blur 4, opacity 0.15) on the `Border` named `Bubble`
    - Ensure existing `DataTrigger` for `Role="user"` overrides to `Background="#0E639C"`, `Foreground="White"`, clears border and shadow
    - _Requirements: 3.1, 3.2, 3.3, 3.4_

- [ ] 9. Provider ComboBox Styling
  - [ ] 9.1 Create `GreenComboBoxStyle` and apply to header ComboBoxes
    - Define `GreenComboBoxStyle` in `Styles/ComboBoxStyles.xaml` (or inline in `ChatHeader.xaml`) with toggle button `Background="{DynamicResource PrimaryBrush}"`, `Foreground="White"`, popup `Border Background="#1A1F24"`
    - Apply style to existing `ProviderComboBox` and new `ModelSelector` in `ChatHeader.xaml`
    - Arrange both ComboBoxes side-by-side in the right-hand column with consistent height and spacing
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5_

- [ ] 10. GPT Model Selector – ChatHeader XAML
  - [ ] 10.1 Add `ModelSelector` ComboBox to `ChatHeader.xaml`
    - Add `ComboBox` bound to `AvailableModels`, `SelectedModel`, `IsEnabled="{Binding IsModelSelectorEnabled}"`
    - Apply `GreenComboBoxStyle` and place side-by-side with `ProviderComboBox`
    - _Requirements: 1.1, 1.6_

- [ ] 11. Checkpoint — Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 12. Project Library – entities, repositories, and navigation
  - [ ] 12.1 Replace `PromptLibrary` with `ProjectLibrary` in navigation
    - Replace `PromptLibrary` with `ProjectLibrary` in `Models/NavigationPage.cs` enum
    - Update `NavigationMenu.xaml` / `NavigationMenu.xaml.cs` to show "Project Library" item
    - Add `case NavigationPage.ProjectLibrary:` routing in `MainWindowViewModel` to `ProjectLibraryView`
    - _Requirements: 5.1, 5.14_

  - [ ] 12.2 Implement `ProjectLibraryViewModel` and `ProjectLibraryView.xaml`
    - `ProjectLibraryViewModel` loads all projects via `ProjectRepository.GetAllAsync` into `ObservableCollection<ProjectModel>`
    - Add `CreateProjectCommand` (prompts for name, calls `ProjectRepository.CreateAsync`, auto-creates `ChatEntity` with new `ProjectId`, broadcasts message)
    - Add `RenameProjectCommand` (calls `ProjectRepository.UpdateNameAsync`)
    - Add `DeleteProjectCommand` (calls `ProjectRepository.DeleteAsync`, removes local files for associated attachments, refreshes list)
    - Add `OpenProjectCommand` (navigates to `ProjectDetailView` for the selected project)
    - `ProjectLibraryView.xaml` lists projects with create/rename/delete/open affordances
    - Handle DB failure on create: roll back auto-created chat; show `ErrorMessage`
    - _Requirements: 5.2, 5.3, 5.4, 5.5, 5.6, 5.7, 5.8, 5.9_

  - [ ]* 12.3 Write property tests for project name persistence (Property 9)
    - **Property 9: Project name persisted accurately** — for any non-empty name string, `CreateAsync` and `UpdateNameAsync` store the exact string without transformation
    - Use in-memory SQLite
    - _Requirements: 5.4, 5.7_

  - [ ]* 12.4 Write property test for auto-chat creation (Property 10)
    - **Property 10: Creating a project auto-creates an associated chat** — for any project name, after create there is exactly one `ChatEntity` with `ProjectId == newProject.Id`
    - _Requirements: 5.5_

  - [ ]* 12.5 Write property test for project cascade delete (Property 11)
    - **Property 11: Project cascade delete removes all related data** — for any project with N files and M messages in its chat, `DeleteAsync` leaves zero rows referencing the project in `FileAttachments`, `Messages`, and `Chats`
    - _Requirements: 5.9_

- [ ] 13. Project Library – detail view and file management
  - [ ] 13.1 Implement `ProjectDetailViewModel` and `ProjectDetailView.xaml`
    - `ProjectDetailViewModel` loads all `FileAttachmentEntity` records via `FileRepository.GetByProjectAsync` into `ObservableCollection<FileAttachmentModel>`
    - Embed the project's `ChatEntity` in a right-hand chat panel (re-use existing chat controls)
    - Add `AddFileCommand` (same file-pick + copy + persist flow as Requirement 2, with `ProjectId` set)
    - Add `RemoveFileCommand` (calls `FileRepository.DeleteAsync` + `IFileService.DeleteFromStorageAsync`)
    - `ProjectDetailView.xaml` shows split layout: file list left, embedded chat right
    - _Requirements: 5.10, 5.11_

  - [ ]* 13.2 Write property test for project file list completeness (Property 12)
    - **Property 12: Project file list is complete when a project is opened** — for any project with any set of linked `FileAttachmentEntity` records, `ProjectDetailViewModel.Files` contains exactly those entries (matched by `Id`)
    - _Requirements: 5.11_

- [ ] 14. AIChatService – project context injection
  - [ ] 14.1 Inject project file contents as system message in `AIChatService`
    - At the start of `AskAsync`, check `chat.ProjectId != null`
    - Load all `FileAttachmentEntity` records via `FileRepository.GetByProjectAsync`
    - Build system message from `IFileService.ReadText` for each file; include `[File '<name>' was unavailable]` for unreadable files
    - Prepend `AIMessage { Role = "system", Content = ... }` before the chat history passed to `IAIProvider.SendAsync`
    - _Requirements: 5.12, 5.13_

  - [ ]* 14.2 Write property tests for `AIChatService` project context (Properties 13)
    - **Property 13: ProjectChat messages prepend all file contents as a system message** — for any project chat with any set of readable file attachments and any message history, the first message in the list passed to `SendAsync` has `Role = "system"` and `Content` contains the text of every file
    - Also test graceful handling: unreadable files produce the `[File '...' was unavailable]` notice and the request still proceeds
    - Use `Moq` for `IAIProvider` and `IFileService`
    - _Requirements: 5.12, 5.13_

- [ ] 15. xUnit test project setup
  - [ ] 15.1 Create `AIWorkspace.Tests` xUnit project
    - Add new `AIWorkspace.Tests` project targeting .NET 8 to the solution
    - Add package references: `xunit`, `xunit.runner.visualstudio`, `Moq`, `FsCheck.Xunit`, `Microsoft.EntityFrameworkCore.InMemory`
    - Reference `AIWorkspace` main project
    - _Requirements: (cross-cutting testing infrastructure)_

- [ ] 16. Final checkpoint — Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for a faster MVP
- Each task references specific requirements for traceability
- Migrations must be applied in order: `AddProjects` before `AddFileAttachments` (FK dependency)
- Property-based tests use FsCheck (via `FsCheck.Xunit`); each test file includes a comment `// Feature: chat-enhancements, Property N: <description>`
- Unit tests use Moq for `IAIProvider`, `IFileService`, and `IMessenger`; repository tests use EF Core in-memory SQLite
- The xUnit test project (Task 15.1) should be created first so test sub-tasks can be committed alongside their implementation tasks
- Requirements 3 (bubble styling), 4 (ComboBox styling), and structural requirements 5.1–5.3 / 5.14–5.16 are verified by code review of XAML/C# changes

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1", "1.2", "1.3", "15.1"] },
    { "id": 1, "tasks": ["2.1", "2.2", "2.3", "3.1"] },
    { "id": 2, "tasks": ["2.4", "2.5", "3.2", "4.1"] },
    { "id": 3, "tasks": ["4.2", "4.3", "6.1", "7.1", "8.1", "9.1", "12.1"] },
    { "id": 4, "tasks": ["4.4", "6.2", "6.3", "7.2", "7.3", "10.1", "12.2"] },
    { "id": 5, "tasks": ["12.3", "12.4", "12.5", "13.1"] },
    { "id": 6, "tasks": ["13.2", "14.1"] },
    { "id": 7, "tasks": ["14.2"] }
  ]
}
```
