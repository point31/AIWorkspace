# Bugfix Requirements Document

## Introduction

This document captures five bugs found in the AIWorkspace WPF application. Together they prevent the app from starting correctly, cause runtime crashes, and make provider routing and DI wiring non-functional. The bugs span the ViewModel layer, AI provider layer, database initialization, and dependency injection setup.

---

## Bug 1: Duplicate Constructor / Null `_chatService` in `MessageInputViewModel`

### Current Behavior (Defect)

1.1 WHEN `MessageInputViewModel` is compiled THEN the system fails with a compile error because two constructors share the same signature `(MessageRepository, IMessenger)`.

1.2 WHEN the first constructor `(MessageRepository, IMessenger)` is resolved by the DI container THEN the system leaves `_chatService` as `null` and `SendMessage()` throws a `NullReferenceException` when it calls `_chatService.AskAsync()`.

### Expected Behavior (Correct)

2.1 WHEN `MessageInputViewModel` is compiled THEN the system SHALL compile without errors because only one constructor exists.

2.2 WHEN `MessageInputViewModel` is instantiated via DI THEN the system SHALL inject `AIChatService` so that `_chatService` is never null and `SendMessage()` can call `_chatService.AskAsync()` without throwing.

### Unchanged Behavior (Regression Prevention)

3.1 WHEN `SendMessage()` is called with a valid chat and non-empty message THEN the system SHALL CONTINUE TO save the message via `MessageRepository` and broadcast a `MessageAddedMessage`.

3.2 WHEN a `ChatSelectedMessage` is received THEN the system SHALL CONTINUE TO update `_currentChatId` correctly.

---

## Bug 2: Copy-Paste Wrong `ProviderType` in `ClaudeProvider` and `GeminiProvider`

### Current Behavior (Defect)

1.1 WHEN `ClaudeProvider.Provider` is read THEN the system returns `ProviderType.OpenAI` instead of `ProviderType.Claude`.

1.2 WHEN `GeminiProvider.Provider` is read THEN the system returns `ProviderType.OpenAI` instead of `ProviderType.Gemini`.

1.3 WHEN `ProviderManager.GetProvider(ProviderType.Claude)` or `ProviderManager.GetProvider(ProviderType.Gemini)` is called THEN the system throws an `InvalidOperationException` (no matching element) or silently returns the wrong provider.

### Expected Behavior (Correct)

2.1 WHEN `ClaudeProvider.Provider` is read THEN the system SHALL return `ProviderType.Claude`.

2.2 WHEN `GeminiProvider.Provider` is read THEN the system SHALL return `ProviderType.Gemini`.

2.3 WHEN `ProviderManager.GetProvider(ProviderType.Claude)` is called THEN the system SHALL return the `ClaudeProvider` instance.

2.4 WHEN `ProviderManager.GetProvider(ProviderType.Gemini)` is called THEN the system SHALL return the `GeminiProvider` instance.

### Unchanged Behavior (Regression Prevention)

3.1 WHEN `ProviderManager.GetProvider(ProviderType.OpenAI)` is called THEN the system SHALL CONTINUE TO return the `OpenAIProvider` instance.

---

## Bug 3: `DatabaseService.Initialize()` Does Not Apply EF Core Migrations

### Current Behavior (Defect)

1.1 WHEN the application starts for the first time THEN the system does not apply EF Core migrations because `DatabaseService.Initialize()` has an empty body, leaving the SQLite database with no tables.

1.2 WHEN any repository method performs a database query on first run THEN the system crashes or silently fails because the required tables do not exist.

### Expected Behavior (Correct)

2.1 WHEN `DatabaseService.Initialize()` is called on startup THEN the system SHALL apply all pending EF Core migrations so that all required database tables exist before any repository is used.

2.2 WHEN the database is already up to date THEN the system SHALL complete initialization without error or data loss.

### Unchanged Behavior (Regression Prevention)

3.1 WHEN the database already exists and is fully migrated THEN the system SHALL CONTINUE TO leave existing data intact.

3.2 WHEN the application starts on a subsequent run THEN the system SHALL CONTINUE TO connect and query the database without re-creating or dropping tables.

---

## Bug 4: `MainWindowViewModel` Instantiates Views Directly, Bypassing DI

### Current Behavior (Defect)

1.1 WHEN `MainWindowViewModel` sets its initial view THEN the system creates `new HomeView()` directly, bypassing the DI container so no `DataContext` or ViewModel is injected into the view.

1.2 WHEN a `NavigationChangedMessage` is received THEN the system creates `new ChatsView()`, `new AIProvidersView()`, etc. directly, so all navigation targets are created outside the DI container with no injected dependencies.

### Expected Behavior (Correct)

2.1 WHEN `MainWindowViewModel` sets its initial view THEN the system SHALL resolve `HomeView` (and its ViewModel) from the DI container so that all dependencies are properly injected.

2.2 WHEN a `NavigationChangedMessage` is received THEN the system SHALL resolve the target view from the DI container so that each view receives its injected `DataContext` and ViewModel.

### Unchanged Behavior (Regression Prevention)

3.1 WHEN navigation changes to any registered page THEN the system SHALL CONTINUE TO update `CurrentView` so that the correct view is displayed in the main window.

3.2 WHEN the application starts THEN the system SHALL CONTINUE TO display the Home view as the default page.

---

## Bug 5: `ProviderManager` Does Not Apply Saved API Keys to Provider Instances

### Current Behavior (Defect)

1.1 WHEN providers are registered as singletons in the DI container THEN the system creates them with no API key configuration because `HostBuilder` does not read from `ProviderSettingsRepository`.

1.2 WHEN a user has saved an API key via the settings UI THEN the system does not apply that key to the corresponding provider instance, so all AI requests fail with authentication errors.

### Expected Behavior (Correct)

2.1 WHEN a provider is used to send a request THEN the system SHALL have applied the API key stored in `ProviderSettingsRepository` for that provider type before the request is made.

2.2 WHEN a user saves a new API key via the settings UI THEN the system SHALL update the corresponding provider instance so subsequent requests use the new key.

### Unchanged Behavior (Regression Prevention)

3.1 WHEN a provider has no saved API key THEN the system SHALL CONTINUE TO handle the missing key gracefully (e.g., disable the provider or surface an error) without crashing.

3.2 WHEN `ProviderManager.GetProvider()` is called THEN the system SHALL CONTINUE TO return the correct provider instance by type.

---

## Bug Condition Summary

### Bug 1 — `MessageInputViewModel` Duplicate Constructor

```pascal
FUNCTION isBugCondition(X)
  INPUT: X of type ConstructorCall
  OUTPUT: boolean
  RETURN X.constructorUsed = FirstConstructor  // (MessageRepository, IMessenger)
END FUNCTION

// Fix Checking
FOR ALL X WHERE isBugCondition(X) DO
  ASSERT no_compile_error AND _chatService IS NOT NULL
END FOR

// Preservation Checking
FOR ALL X WHERE NOT isBugCondition(X) DO
  ASSERT F(X) = F'(X)  // second constructor behavior unchanged
END FOR
```

### Bug 2 — Wrong `ProviderType` on Claude/Gemini Providers

```pascal
FUNCTION isBugCondition(X)
  INPUT: X of type ProviderLookup
  OUTPUT: boolean
  RETURN X.requestedType = ProviderType.Claude OR X.requestedType = ProviderType.Gemini
END FUNCTION

// Fix Checking
FOR ALL X WHERE isBugCondition(X) DO
  result ← ProviderManager.GetProvider'(X.requestedType)
  ASSERT result.Provider = X.requestedType
END FOR

// Preservation Checking
FOR ALL X WHERE NOT isBugCondition(X) DO
  ASSERT F(X) = F'(X)  // OpenAI lookup unchanged
END FOR
```

### Bug 3 — Empty `DatabaseService.Initialize()`

```pascal
FUNCTION isBugCondition(X)
  INPUT: X of type AppStartup
  OUTPUT: boolean
  RETURN X.isFirstRun = true OR X.hasPendingMigrations = true
END FUNCTION

// Fix Checking
FOR ALL X WHERE isBugCondition(X) DO
  result ← DatabaseService.Initialize'(X)
  ASSERT all_tables_exist(result) AND no_crash(result)
END FOR

// Preservation Checking
FOR ALL X WHERE NOT isBugCondition(X) DO
  ASSERT F(X) = F'(X)  // existing data and schema untouched
END FOR
```

### Bug 4 — Views Created Outside DI

```pascal
FUNCTION isBugCondition(X)
  INPUT: X of type ViewCreation
  OUTPUT: boolean
  RETURN X.createdViaNewKeyword = true
END FUNCTION

// Fix Checking
FOR ALL X WHERE isBugCondition(X) DO
  result ← NavigateTo'(X.page)
  ASSERT result.DataContext IS NOT NULL AND result.resolvedFromDI = true
END FOR

// Preservation Checking
FOR ALL X WHERE NOT isBugCondition(X) DO
  ASSERT F(X) = F'(X)  // CurrentView still updates on navigation
END FOR
```

### Bug 5 — Saved API Keys Not Applied to Providers

```pascal
FUNCTION isBugCondition(X)
  INPUT: X of type AIRequest
  OUTPUT: boolean
  RETURN X.provider.apiKey = null OR X.provider.apiKey = ""
END FUNCTION

// Fix Checking
FOR ALL X WHERE isBugCondition(X) DO
  result ← ConfigureProvider'(X.providerType)
  ASSERT result.apiKey = ProviderSettingsRepository.GetAsync(X.providerType).ApiKey
END FOR

// Preservation Checking
FOR ALL X WHERE NOT isBugCondition(X) DO
  ASSERT F(X) = F'(X)  // providers already configured remain unchanged
END FOR
```
