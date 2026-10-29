using System.Text;
using ConsoleIO = System.Console;

namespace UtpPos.Console.Tools.Helpers
{
    public static class ConsolePasswordReader
    {
        public static string Read(string prompt)
        {
            if (ConsoleIO.IsInputRedirected)
                throw new InvalidOperationException("La contraseña solo puede leerse desde una consola interactiva.");

            ConsoleIO.Write(prompt);
            var buffer = new StringBuilder();

            while (true)
            {
                var key = ConsoleIO.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Enter)
                    break;

                if (key.Key == ConsoleKey.Backspace)
                {
                    if (buffer.Length > 0)
                        buffer.Length--;
                    continue;
                }

                if (!char.IsControl(key.KeyChar))
                    buffer.Append(key.KeyChar);
            }

            ConsoleIO.WriteLine();
            var password = buffer.ToString();
            for (var index = 0; index < buffer.Length; index++)
                buffer[index] = '\0';

            return password;
        }
    }
}
