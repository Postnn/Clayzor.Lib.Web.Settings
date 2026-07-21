# Clayzor.Lib.Web.Settings

Библиотека настроек web-приложения решения **Clayzor**. Содержит модель настроек `ClayAppSettings` и методы расширения для загрузки конфигурации из `appsettings.json` и `web.config` с последующим биндингом. Не зависит от базы данных и слоя доступа к данным — только конфигурационные биндинги.

> Базовый (leaf) проект решения: без зависимостей от других проектов Clayzor. Подключается как `ProjectReference`. Отдельного NuGet-пакета и релизов нет.

## Содержание

- [Что это](#что-это)
- [Место в решении](#место-в-решении)
- [Технологии и зависимости](#технологии-и-зависимости)
- [Состав](#состав)
- [ClayAppSettings](#clayappsettings)
- [Загрузка конфигурации](#загрузка-конфигурации)
- [Приоритет строк подключения](#приоритет-строк-подключения)
- [Разработка](#разработка)
- [Лицензия](#лицензия)

## Что это

Проект инкапсулирует чтение настроек приложения. Класс `ClayAppSettings` описывает типизированные параметры (строки подключения, имя приложения, LDAP, размер страницы, таймаут команд), а методы расширения `AddWebConfig` и `BindClaySettings` собирают их из стандартной конфигурации .NET, дополнительно поддерживая устаревший источник `web.config` в формате XML.

## Место в решении

```
Clayzor.Lib.Web.Settings  — этот проект: конфигурационные биндинги (без БД)
        ▲
        │ ProjectReference
Clayzor.Lib.Web.Controls  — UI-компоненты (использует настройки приложения)
```

Строку подключения из `ClayAppSettings.ConnectionString` приложение передаёт, например, в `DbManager` (`Clayzor.Lib.DALC`) при его регистрации. Сам этот проект от DALC и БД не зависит.

## Технологии и зависимости

- **.NET 10** (`net10.0`), `Microsoft.NET.Sdk`, включены `ImplicitUsings` и `Nullable`.
- `Microsoft.Extensions.Configuration` `10.*`
- `Microsoft.Extensions.Configuration.Binder` `10.*`
- `Microsoft.Extensions.Configuration.Xml` `10.*` (чтение `web.config`)
- `Microsoft.Extensions.Options` `10.*`

Внешних `ProjectReference` нет.

## Состав

```
Clayzor.Lib.Web.Settings/
├─ ClayAppSettings.cs   модель настроек + расширения AddWebConfig / BindClaySettings
├─ AGENTS.md            краткие правила проекта
└─ Clayzor.Lib.Web.Settings.csproj
```

## ClayAppSettings

Модель настроек связывается из секции `"ClayApp"` конфигурации.

| Свойство | Тип | По умолчанию | Назначение |
| --- | --- | --- | --- |
| `ConnectionString` | `string` | `""` | Строка подключения к основной БД. |
| `DictionaryConnectionString` | `string?` | `null` | Строка подключения к БД справочников (опционально). |
| `ApplicationName` | `string` | `"Clayzor"` | Наименование приложения. |
| `LdapPath` | `string?` | `null` | Путь к LDAP для аутентификации (опционально). |
| `DefaultPageSize` | `int` | `50` | Размер страницы по умолчанию для таблиц данных. |
| `CommandTimeout` | `int` | `30` | Таймаут команд БД в секундах. |

Пример секции в `appsettings.json`:

```json
{
  "ClayApp": {
    "ApplicationName": "Clayzor",
    "DefaultPageSize": 50,
    "CommandTimeout": 30,
    "LdapPath": "LDAP://example.local"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;",
    "DictionaryConnection": "Server=...;Database=...;"
  }
}
```

## Загрузка конфигурации

Два метода расширения (класс `WebConfigExtensions`):

| Метод | Назначение |
| --- | --- |
| `IConfigurationBuilder.AddWebConfig(path?)` | Добавляет `web.config` как дополнительный XML-источник конфигурации (если файл существует, `optional`, с `reloadOnChange`). По умолчанию берётся `web.config` из текущей директории. Вызывается **до** `BindClaySettings()`. |
| `IConfiguration.BindClaySettings()` | Связывает секцию `"ClayApp"` в новый `ClayAppSettings` и возвращает его. Если `ConnectionString` в секции не задана — берёт `ConnectionStrings:DefaultConnection`; если не задана `DictionaryConnectionString` — берёт `ConnectionStrings:DictionaryConnection`. |

Пример использования (иллюстративно; детали регистрации зависят от приложения):

```csharp
// Подключаем web.config как источник XML-конфигурации (если используется)
builder.Configuration.AddWebConfig();

// Биндим настройки и регистрируем их в DI
var claySettings = builder.Configuration.BindClaySettings();
builder.Services.AddSingleton(claySettings);

// Далее строку подключения можно передать в DbManager (Clayzor.Lib.DALC)
```

## Приоритет строк подключения

Согласно `AGENTS.md`, приоритет источника строки подключения — **`web.config` → `appsettings.json`**: значения из `web.config`, добавленного через `AddWebConfig`, перекрывают значения из `appsettings.json`. Внутри `BindClaySettings` дополнительно действует резервный порядок: если в секции `"ClayApp"` строка подключения не указана, используется соответствующая запись из `ConnectionStrings`. Полное описание приоритетов — в разделе «Configuration — connection string priority» корневого `AGENTS.md` решения.

## Разработка

`AGENTS.md` — краткий ориентир по проекту: назначение `ClayAppSettings`, метод `BindClaySettings()` и приоритет строк подключения. Проект намеренно не зависит от DALC и БД и содержит только конфигурационные биндинги. Глобальные правила решения — в корневом `AGENTS.md` вышестоящего репозитория.

## Лицензия

Проект распространяется под лицензией **Apache License 2.0** — полный текст в файле [`LICENSE`](LICENSE) в корне репозитория.

Copyright © 2026 Bulychev Nick
