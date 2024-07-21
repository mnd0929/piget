using System;
using System.Collections.Generic;

namespace QislEngine.Utils
{
    public static class Notify
    {
        public static void SavePackageNotify(string dir) =>
            Logs.LogCustom(new Dictionary<string, ConsoleColor> {
                    { "Ресурсы сохранены в ", System.Console.ForegroundColor }, { dir + Environment.NewLine, ConsoleColor.Blue } });

        public static void ResourceAuthenticationDisabledNotify() =>
            Logs.Log("<?> ", "Аутентификация хэшей ресурсов отключена.");

        public static void ResourcesAlreadyLoadedNotify() =>
            Logs.Log("<?> ", "Скрипт будет инициализирован с сохраненными ресурсами.");

        public static void UnknownPublisherNotify() =>
            Logs.Log("<?> ", "Издатель PIGET (QISL) не несет ответственности за сторонние сценарии и не предоставляет для них никакие лицензии.");

        public static void ProcessStartNotify(int id) =>
            Logs.Log("<?> ", $"Процесс инициализируется со следующим идентификатором: {id}.");
    }
}
