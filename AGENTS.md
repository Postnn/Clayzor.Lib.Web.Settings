> Глобальные правила и обзор решения — в корневом /AGENTS.md. Здесь — только специфика проекта Clayzor.Lib.Web.Settings.

Сборка `Clayzor.Lib.Web.Settings` содержит классы настроек и конфигурации Clayzor:
- `ClayAppSettings` — биндинг конфигурации (`BindClaySettings()`), приоритет connection string (web.config → appsettings.json)
- Не зависит от DALC и БД — только конфигурационные биндинги

Детали конфигурации — в корневом /AGENTS.md, раздел «## Configuration — connection string priority».
