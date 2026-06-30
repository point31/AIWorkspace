# Design Document: Chat Enhancements

## Overview

This document describes the technical design for five enhancements to the AIWorkspace WPF desktop application:

1. **GPT Model Selector** – a per-chat model picker in `ChatHeader` limited to `gpt-4o-mini`, `gpt-4o`, and `gpt-o3`
2. **File Upload in Chat** – attach-file button in `MessageInput` that copies files to local storage and persists metadata
3. **AI Message Bubble Styling** – white-background assistant bubbles with dark text, border, and shadow
4. **Provider ComboBox Styling** – green/white styling for the provider and model pickers in `ChatHeader`
5. **Project Library** – replace Prompt Library with a project-scoped workspace that bundles files and a dedicated chat

The application stack is C# .NET 8, WPF, CommunityToolkit.Mvvm, EF Core 8 + SQLite, and the OpenAI SDK v2.

---

## Architecture

The application follows a layered MVVM pattern:

```
Views / Controls (XAML)
        ↕  DataContext / DataBinding
ViewModels (CommunityToolkit.Mvvm ObservableObject)
        ↕  IMessenger (WeakReferenceMessenger)
Services  (AIChatService, DatabaseService, FileService)
        ↕  Repository
Repositories (ChatRepository, MessageRepository, FileRepository, ProjectRepository)
        ↕  EF Core DbContext
SQLite database + Local File System
        ↕  AI Providers (OpenAI, Claude, Gemini)
```

**Cross-cutting concerns:**
- Messaging between ViewModels uses `CommunityToolkit.Mvvm.Messaging.IMessenger` (weak reference, publish/subscribe).
- Dependency injection is handled via `Microsoft.Extensions.Hosting` / `IServiceCollection` in `HostBuilder.cs`.
- All new entities require an EF Core migration.

---

## Components and Interfaces

### Requirement 1 – GPT Model Selector

**`ChatHeaderViewModel` changes:**
- Add `ObservableProperty string selectedModel` (bound to ModelSelector ComboBox).
- Add read-only `IReadOnlyList<string> AvailableModels` = `["gpt-4o-mini", "gpt-4o", "gpt-o3"]`.
- Add `ObservableProperty bool isModelSelectorEnabled` (computed from `SelectedProvider == OpenAI`).
- Extend `Receive(ChatSelectedMessage)` to also read `message.Value.Model` and apply fallback/validation.
- Add `partial void OnSelectedModelChanged(string value)` to persist via `ChatRepository.UpdateModelAsync`.

**`ChatRepository` changes:**
- Add `Task UpdateModelAsync(int chatId, string model)`.

**`AIChatService` changes:**
- In `AskAsync`, after resolving the provider, call `provider.Configure(chat.ApiKey, chat.Model)` using the per-chat model before `SendAsync` (for OpenAI only; other providers fall through to their stored settings).

**`ChatHeader.xaml` changes:**
- Add a ModelSelector `ComboBox` bound to `AvailableModels` / `SelectedModel` / `IsEnabled`.
- Arrange ProviderComboBox and ModelSelector side-by-side in the right-hand column.

**`ChatModel` changes:**
- Add `string Model` property so the model value travels in the `ChatSelectedMessage`.

### Requirement 2 – File Upload in Chat

**New entity: `FileAttachmentEntity`**
```csharp
public class FileAttachmentEntity
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public ChatEntity Chat { get; set; } = null!;
    public string FileName { get; set; } = "";
    public string LocalPath { get; set; } = "";
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public int? ProjectId { get; set; }            // null = standalone attachment
    public ProjectEntity? Project { get; set; }
}
```

**New service: `FileService`**
```csharp
public interface IFileService
{
    Task<string> CopyToStorageAsync(string sourcePath, CancellationToken ct = default);
    Task DeleteFromStorageAsync(string localPath);
    string ReadText(string localPath);            // used by AIChatService for project context
}
```
Storage root: `%LOCALAPPDATA%\AIWorkspace\Files\`

**New repository: `FileRepository`**
```csharp
Task<FileAttachmentEntity> AddAsync(FileAttachmentEntity entity);
Task<List<FileAttachmentEntity>> GetAllAsync();
Task<List<FileAttachmentEntity>> GetByChatAsync(int chatId);
Task<List<FileAttachmentEntity>> GetByProjectAsync(int projectId);
Task DeleteAsync(int id);
```

**`MessageInputViewModel` changes:**
- Inject `FileService` and `FileRepository`.
- Add `AttachFileCommand` (opens `OpenFileDialog`, copies, persists entity, sends `FileAttachedMessage`).
- Error display via a `string ErrorMessage` observable property (shown in a small inline banner).

**`MessageInput.xaml` changes:**
- Add paperclip `Button` left of the text input, bound to `AttachFileCommand`.

**`FilesViewModel` + `FilesView.xaml` implementation:**
- `FilesViewModel` loads all `FileAttachmentEntity` records and exposes `ObservableCollection<FileAttachmentModel> Files`.
- Each `FileAttachmentModel` exposes `FileName`, `FileSizeDisplay` (e.g. "1.4 MB"), `ChatTitle`, `UploadedAt`.
- `DeleteFileCommand` removes the DB record and local file.

### Requirement 3 – AI Message Bubble Styling

Changes confined to `MessageList.xaml`:

- Remove the hard-coded `Background="#2B2B2B"` on the `Border` named `Bubble`.
- Set a default `Background="White"` and `Foreground="#1A1A1A"` for all bubbles (assistant).
- Add `BorderBrush="#DDDDDD"`, `BorderThickness="1"` and a `DropShadowEffect` (blur 4, opacity 0.15, dark colour) to the `Border`.
- The existing `DataTrigger` for `Role="user"` overrides `Background` to `#0E639C`, `Foreground` to `White`, and clears the border/shadow.

### Requirement 4 – Provider ComboBox Styling

New XAML style resource (added to `Styles/ComboBoxStyles.xaml` or inline in `ChatHeader`):

- `ControlTemplate` for `ComboBox`:
  - Toggle button background: `{DynamicResource PrimaryBrush}` (`#2E7D32`)
  - Toggle button foreground / arrow: `White`
  - Popup `Border` background: `#1A1F24` (dark, not green)
  - Popup item highlight: `{DynamicResource PrimaryHoverBrush}`
- Applied to both `ProviderComboBox` and `ModelSelector` via a shared `Style` key `GreenComboBoxStyle`.

### Requirement 5 – Project Library

**New entities:**

```csharp
public class ProjectEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    // Navigation
    public ChatEntity? Chat { get; set; }
    public ICollection<FileAttachmentEntity> Files { get; set; } = [];
}

// ProjectFileEntity is implicit through FileAttachmentEntity.ProjectId FK
```

**`ChatEntity` changes:**
- Add `int? ProjectId { get; set; }` and navigation `ProjectEntity? Project { get; set; }`.

**New repository: `ProjectRepository`**
```csharp
Task<ProjectEntity> CreateAsync(string name);
Task<List<ProjectEntity>> GetAllAsync();
Task<ProjectEntity?> GetAsync(int id);
Task UpdateNameAsync(int id, string name);
Task DeleteAsync(int id);           // cascades via DB rules
```

**`NavigationPage` enum:** replace `PromptLibrary` with `ProjectLibrary`.

**`MainWindowViewModel`:** add `case NavigationPage.ProjectLibrary:` routing to new `ProjectLibraryView`.

**New views / view models:**
- `ProjectLibraryView.xaml` + `ProjectLibraryViewModel` – lists projects, create/rename/delete/open.
- `ProjectDetailView.xaml` + `ProjectDetailViewModel` – split panel: file list (left) + embedded chat (right), with add/remove file buttons.

**`AIChatService` changes:**
- At the start of `AskAsync`, check if `chat.ProjectId != null`.
- If yes, load all `FileAttachmentEntity` records for the project via `FileRepository.GetByProjectAsync`.
- Build a system message string from each file's text content (or a "file unavailable" notice if the file cannot be read).
- Prepend this system message as `AIMessage { Role = "system", Content = ... }` before the chat history.

---

## Data Models

### Entity Relationship Diagram

```
ProjectEntity (1)──────────────(0..1) ChatEntity
      │
      │ (1)──────────────(*) FileAttachmentEntity
                                     │
                                     │ FK ChatId
                                     │
                               ChatEntity (1)────(*) MessageEntity
```

### New EF Core Configuration (AppDbContext)

```csharp
// Projects
modelBuilder.Entity<ProjectEntity>()
    .HasOne<ChatEntity>(p => p.Chat)
    .WithOne(c => c.Project)
    .HasForeignKey<ChatEntity>(c => c.ProjectId)
    .OnDelete(DeleteBehavior.Cascade);

// FileAttachments → Project (optional)
modelBuilder.Entity<FileAttachmentEntity>()
    .HasOne(f => f.Project)
    .WithMany(p => p.Files)
    .HasForeignKey(f => f.ProjectId)
    .OnDelete(DeleteBehavior.Cascade)
    .IsRequired(false);

// FileAttachments → Chat (required)
modelBuilder.Entity<FileAttachmentEntity>()
    .HasOne(f => f.Chat)
    .WithMany()
    .HasForeignKey(f => f.ChatId)
    .OnDelete(DeleteBehavior.Restrict);   // Chat deletion handled via Project cascade
```

### Migration strategy

Two new migrations are needed:

1. `AddFileAttachments` – adds `FileAttachments` table with FK to `Chats` and nullable FK to `Projects`.
2. `AddProjects` – adds `Projects` table, adds `ProjectId` nullable FK column to `Chats`.

> The migrations should be ordered so `Projects` is created before `FileAttachments` since `FileAttachmentEntity.ProjectId` references it. In practice, scaffold `AddProjects` first, then `AddFileAttachments`.

### Updated `ChatModel`

```csharp
public partial class ChatModel : ObservableObject
{
    // existing fields…
    [ObservableProperty] private string model = "";
    [ObservableProperty] private int? projectId;
}
```

### New `FileAttachmentModel`

```csharp
public class FileAttachmentModel
{
    public int Id { get; set; }
    public string FileName { get; set; } = "";
    public long FileSize { get; set; }
    public string FileSizeDisplay => FileSize < 1_048_576
        ? $"{FileSize / 1024.0:F1} KB"
        : $"{FileSize / 1_048_576.0:F1} MB";
    public string ChatTitle { get; set; } = "";
    public DateTime UploadedAt { get; set; }
    public string LocalPath { get; set; } = "";
}
```

---

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system — essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

**Property Reflection:**

Before listing properties, redundancies are eliminated:
- 1.2 (model loaded from chat) and 1.4 (model persisted on change) are both about the ViewModel ↔ Model round-trip for the model field. They test different directions and are kept separate.
- 5.4 (project name persisted) and 5.7 (project rename persisted) both test name persistence but in different operations (create vs. update). They are combined into a single "name round-trip" property.
- 5.5 (chat auto-created for project) and 5.11 (files loaded when project opened) are distinct and kept separate.
- 2.3 (file copied to storage) and 2.4 (entity has correct metadata) can be combined since both validate outcomes of the same upload operation.

### Property 1: Model selection is reflected in the ViewModel

*For any* `ChatModel` carrying a valid GPT_Model string (`gpt-4o-mini`, `gpt-4o`, or `gpt-o3`), when a `ChatSelectedMessage` with that model is received by `ChatHeaderViewModel`, the `SelectedModel` property SHALL equal the model string from the chat.

**Validates: Requirements 1.2**

### Property 2: Invalid model values fall back to the default

*For any* string that is not one of the three valid GPT_Model identifiers (including empty string, null-equivalent, or an arbitrary non-matching string), the `ChatHeaderViewModel` SHALL set `SelectedModel` to `"gpt-4o-mini"`.

**Validates: Requirements 1.3, 1.7**

### Property 3: Model selection change triggers persistence

*For any* of the three valid GPT_Model strings, changing `SelectedModel` in `ChatHeaderViewModel` (when a chat is active) SHALL invoke `ChatRepository.UpdateModelAsync` with that exact model string.

**Validates: Requirements 1.4**

### Property 4: AIChatService passes per-chat model to the provider

*For any* OpenAI chat with any valid GPT_Model string and any non-empty message history, `AIChatService.AskAsync` SHALL call `IAIProvider.Configure` with the chat's model string before calling `IAIProvider.SendAsync`.

**Validates: Requirements 1.5**

### Property 5: ModelSelector enabled state tracks provider

*For any* `ProviderType` value, `ChatHeaderViewModel.IsModelSelectorEnabled` SHALL be `true` if and only if the provider is `ProviderType.OpenAI`.

**Validates: Requirements 1.6**

### Property 6: File upload produces a correctly populated entity

*For any* valid file selection (any file path, any file size, any file name), the upload operation SHALL produce a `FileAttachmentEntity` where `FileName` equals the original file name, `LocalPath` points to a file inside the application storage directory, `FileSize` equals the actual byte length of the source file, and `ChatId` matches the active chat.

**Validates: Requirements 2.3, 2.4**

### Property 7: FilesViewModel exposes all persisted file records

*For any* set of `FileAttachmentEntity` records in the database, `FilesViewModel.Files` SHALL contain exactly one `FileAttachmentModel` entry for each entity, with `FileName`, `FileSize`, `ChatTitle`, and `UploadedAt` matching the entity data.

**Validates: Requirements 2.6**

### Property 8: File delete removes both the DB record and the local file

*For any* `FileAttachmentEntity` whose `LocalPath` points to an existing file, invoking the delete operation SHALL remove the entity from the database AND delete the file from local storage.

**Validates: Requirements 2.7**

### Property 9: Project name is always persisted accurately

*For any* non-empty project name string (at create or rename time), the `ProjectEntity.Name` stored in the database SHALL equal the string provided by the user — no truncation, normalisation, or transformation applied.

**Validates: Requirements 5.4, 5.7**

### Property 10: Creating a project automatically produces an associated chat

*For any* successfully created `ProjectEntity`, there SHALL exist exactly one `ChatEntity` in the database whose `ProjectId` equals the new project's `Id`.

**Validates: Requirements 5.5**

### Property 11: Project cascade delete removes all related data

*For any* `ProjectEntity` with any number of associated `FileAttachmentEntity` records and any number of `MessageEntity` records in its linked chat, deleting the project SHALL result in zero remaining rows referencing that project's `Id` in `FileAttachments`, `Messages`, and `Chats`, and zero remaining local files for those attachments.

**Validates: Requirements 5.9**

### Property 12: Project file list is complete when a project is opened

*For any* project with any set of linked `FileAttachmentEntity` records, opening that project in the UI SHALL cause `ProjectDetailViewModel.Files` to contain exactly the same set of file entries (matched by `Id`).

**Validates: Requirements 5.11**

### Property 13: ProjectChat messages prepend all project file contents as a system message

*For any* `ProjectChat` with any set of readable file attachments and any user message history, `AIChatService.AskAsync` SHALL pass a message list to `IAIProvider.SendAsync` whose first element has `Role = "system"` and whose `Content` contains the text of every attached file.

**Validates: Requirements 5.12**

---

## Error Handling

| Scenario | Behaviour |
|---|---|
| File copy to local storage fails (Req 2.5) | Show inline error in `MessageInput`; abort database write; no partial state. |
| Database write fails after successful file copy (Req 2.5) | Delete the copied local file; show inline error message. |
| Local file missing at delete time (Req 2.8) | Delete DB record, show info toast "Local file was not found". |
| Project file unreadable at AI send time (Req 5.13) | Include a line `[File '<name>' was unavailable]` in the system message; continue sending. |
| Invalid GPT model string on chat load (Req 1.7) | Silently replace with `gpt-4o-mini`; persist the corrected value. |
| ProviderManager has no configured OpenAI provider | `AIChatService` throws `InvalidOperationException`; `MessageInputViewModel` catches and sets `ErrorMessage`. |
| Project creation DB failure | Roll back any auto-created chat; surface error in `ProjectLibraryViewModel.ErrorMessage`. |

All user-facing error messages are displayed inline (observable string property bound to a visible `TextBlock`) rather than modal dialogs, consistent with the existing approach in `MessageInputViewModel`.

---

## Testing Strategy

### Unit Tests (example-based)

The project uses **xUnit** (standard for .NET 8). Unit tests live in a new `AIWorkspace.Tests` project.

Tests cover:

- `ChatHeaderViewModel` — model fallback/validation, provider change propagation, model selector enabled state.
- `AIChatService` — model passed to `Configure`, system message prepended for project chats, graceful handling of unreadable files.
- `FileService` — path construction, storage directory creation.
- `FileRepository` — CRUD operations against an in-memory SQLite instance.
- `ProjectRepository` — create, rename, delete cascade.
- `FilesViewModel` — file list population, delete command, size display formatting.

Use `Moq` for mocking `IAIProvider`, `IFileService`, and `IMessenger`. Use `Microsoft.EntityFrameworkCore.InMemory` or SQLite in-memory mode for repository tests.

### Property-Based Tests

The project uses **FsCheck** (property-based testing library for .NET/F#, usable from C# via `FsCheck.Xunit`). Each property test runs a minimum of **100 iterations**.

Each test is tagged with a comment referencing its design property:
```
// Feature: chat-enhancements, Property N: <property_text>
```

**Properties to implement as PBT:**

| # | Property | Generator inputs |
|---|---|---|
| 1 | Model selection reflected in ViewModel | Valid GPT_Model strings |
| 2 | Invalid model falls back to gpt-4o-mini | Arbitrary strings ∉ valid set |
| 3 | Model change triggers persistence | Three valid model strings |
| 4 | AIChatService passes per-chat model to provider | Model string × message history list |
| 5 | ModelSelector enabled iff OpenAI | All ProviderType values |
| 6 | File upload entity has correct metadata | File paths × file sizes |
| 7 | FilesViewModel exposes all persisted records | List of FileAttachmentEntity |
| 8 | Delete removes DB record and local file | FileAttachmentEntity with existing file |
| 9 | Project name persisted accurately | Non-empty strings |
| 10 | Creating project auto-creates chat | Project name strings |
| 11 | Project cascade delete removes all data | Projects with N files and M messages |
| 12 | Project file list is complete | Projects with N file attachments |
| 13 | ProjectChat prepends all file contents | Files × message histories |

### Integration Tests

- File copy + DB write end-to-end (real temp directory, in-memory SQLite).
- Project cascade delete end-to-end (SQLite in-memory with FK enforcement enabled).

### UI / Structural Verification

Requirements 3 (bubble styling), 4 (ComboBox styling), and structural requirements (2.1, 5.1–5.3, 5.14–5.16) are verified by code review of the XAML/C# changes and do not require automated tests.
