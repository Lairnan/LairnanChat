namespace ChatClient.WPF.Behaviors
{
    public static class PasswordGenerator
    {
        public static string GetNewPassword()
        {
            const string text = "abcdefghijklmnopqrstuvwxyz" +
                                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                                "0123456789" +
                                "!@#$%^&*()";

            var random = new Random();
            string newPassword = null;
            while (string.IsNullOrWhiteSpace(newPassword)
                   || PasswordDifficulty.CheckDifficultyPassword(newPassword) == DifficultyPassword.Easy)
            {
                newPassword = "";
                for (var i = 0; i < random.Next(5, 15); i++)
                    newPassword += text[random.Next(0, text.Length)];
            }
            return newPassword;
        }
    }
}