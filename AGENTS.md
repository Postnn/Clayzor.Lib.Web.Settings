> Глобальные правила и обзор решения — в корневом /AGENTS.md. Здесь — только специфика проекта Clayzor.Lib.Web.Settings.

Сборка `Clayzor.Lib.Web.Settings` содержит классы настроек и конфигурации Clayzor:
- `ClayAppSettings` — биндинг конфигурации (`BindClaySettings()`), приоритет connection string (web.config → appsettings.json)
- Не зависит от DALC и БД — только конфигурационные биндинги
- Зависимость `System.Configuration.ConfigurationManager` — для чтения `<connectionStrings>` и `<appSettings>` из `web.config`

## Connection string

`ConnectionString` заполняется по цепочке:
1. `configuration.GetSection("ClayApp").Bind(settings)` — если ключ `ClayApp:ConnectionString` задан
2. Иначе: имя читается из `configuration["ClayGrid:Dynamic:ConnectionStringName"]` (по умолчанию `"DefaultConnection"`)
3. Строка подключения с этим именем читается из `web.config` через `ConfigurationManager.OpenMappedExeConfiguration().ConnectionStrings`

## AppSettings из web.config

`URI_help_clayGrid` (URL справки ClayGrid) читается из `web.config` через `ConfigurationManager.OpenMappedExeConfiguration().AppSettings`. Имя ключа: `URI_help_clayGrid`. Свойство: `ClayAppSettings.UriHelpClayGrid`. Если ключ отсутствует или пуст — кнопка справки на тулбаре грида не отображается.

Детали конфигурации — в корневом /AGENTS.md, раздел «## Configuration — connection string priority».
