using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace Clayzor.Lib.Web.Settings;

/// <summary>
/// Настройки приложения Clayzor.
/// Связываются из секции "ClayApp" конфигурации (appsettings.json + web.config).
/// </summary>
public class ClayAppSettings
{
    /// <summary>Строка подключения к основной БД.</summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Строка подключения к БД справочников (опционально).</summary>
    public string? DictionaryConnectionString { get; set; }

    /// <summary>Наименование приложения.</summary>
    public string ApplicationName { get; set; } = "Clayzor";

    /// <summary>Путь к LDAP для аутентификации (опционально).</summary>
    public string? LdapPath { get; set; }

    /// <summary>Размер страницы по умолчанию для таблиц данных.</summary>
    public int DefaultPageSize { get; set; } = 50;

    /// <summary>Таймаут команд БД в секундах.</summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// URL страницы пользовательской документации ClayGrid.
    /// Значение читается из секции <c>appSettings</c> файла <c>web.config</c>, ключ <c>URI_help_clayGrid</c>.
    /// Если ключ отсутствует или пуст — кнопка справки на тулбаре грида не отображается.
    /// </summary>
    public string? UriHelpClayGrid { get; set; }
}

/// <summary>
/// Методы расширения для загрузки конфигурации из web.config и связывания в <see cref="ClayAppSettings"/>.
/// </summary>
public static class WebConfigExtensions
{
    /// <summary>
    /// Добавляет web.config как дополнительный источник XML-конфигурации.
    /// Вызывается перед BindClaySettings().
    /// </summary>
    /// <param name="builder">Построитель конфигурации.</param>
    /// <param name="path">Путь к web.config (по умолчанию — текущая директория).</param>
    /// <returns>Построитель конфигурации для цепочки вызовов.</returns>
    public static IConfigurationBuilder AddWebConfig(this IConfigurationBuilder builder, string? path = null)
    {
        var webConfigPath = path ?? Path.Combine(Directory.GetCurrentDirectory(), "web.config");
        if (File.Exists(webConfigPath))
            builder.AddXmlFile(webConfigPath, optional: true, reloadOnChange: true);
        return builder;
    }

    /// <summary>
    /// Связывает секцию "ClayApp" в <see cref="ClayAppSettings"/>.
    /// Если ConnectionString не задана в секции, читается из web.config через
    /// <see cref="ConfigurationManager.ConnectionStrings"/>.
    /// Если DictionaryConnectionString не задана — аналогично.
    /// </summary>
    /// <param name="configuration">Корневая конфигурация приложения.</param>
    /// <returns>Заполненный объект <see cref="ClayAppSettings"/>.</returns>
    public static ClayAppSettings BindClaySettings(this IConfiguration configuration)
    {
        var settings = new ClayAppSettings();

        settings.ApplicationName = ReadAppSettingFromWebConfig("AppName") ?? "Clayzor";
        settings.DefaultPageSize = int.TryParse(ReadAppSettingFromWebConfig("ClayGrid:DefaultPageSize"), out var ps) ? ps : 50;
        settings.CommandTimeout = int.TryParse(ReadAppSettingFromWebConfig("Sql:CommandTimeout"), out var ct) ? ct : 30;
        settings.LdapPath = ReadAppSettingFromWebConfig("ClayApp:LdapPath");

        if (string.IsNullOrEmpty(settings.ConnectionString))
        {
            var csName = configuration["ClayGrid:Dynamic:ConnectionStringName"] ?? "DefaultConnection";
            settings.ConnectionString = ReadConnectionStringFromWebConfig(csName) ?? string.Empty;
        }
        if (string.IsNullOrEmpty(settings.DictionaryConnectionString))
            settings.DictionaryConnectionString = ReadConnectionStringFromWebConfig("DictionaryConnection");

        if (string.IsNullOrEmpty(settings.UriHelpClayGrid))
            settings.UriHelpClayGrid = ReadAppSettingFromWebConfig("URI_help_clayGrid");

        return settings;
    }

    /// <summary>
    /// Читает строку подключения с указанным именем из <c>web.config</c>
    /// через <see cref="ConfigurationManager.ConnectionStrings"/>.
    /// </summary>
    /// <param name="name">Имя строки подключения (атрибут name в секции connectionStrings).</param>
    /// <returns>Строка подключения или null, если файл отсутствует или имя не найдено.</returns>
    private static string? ReadConnectionStringFromWebConfig(string name)
    {
        var webConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "web.config");
        if (!File.Exists(webConfigPath))
            return null;

        var map = new ExeConfigurationFileMap { ExeConfigFilename = webConfigPath };
        var config = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
        return config.ConnectionStrings.ConnectionStrings[name]?.ConnectionString;
    }

    /// <summary>
    /// Читает значение из секции <c>appSettings</c> файла <c>web.config</c>
    /// через <see cref="ConfigurationManager.AppSettings"/>.
    /// </summary>
    /// <param name="key">Ключ (атрибут key в секции appSettings).</param>
    /// <returns>Значение или null, если файл отсутствует или ключ не найден.</returns>
    private static string? ReadAppSettingFromWebConfig(string key)
    {
        var webConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "web.config");
        if (!File.Exists(webConfigPath))
            return null;

        var map = new ExeConfigurationFileMap { ExeConfigFilename = webConfigPath };
        var config = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
        return config.AppSettings.Settings[key]?.Value;
    }
}
