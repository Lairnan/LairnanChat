using System.Text.RegularExpressions;

namespace ChatClient.WPF.Behaviors
{
    public static class PasswordDifficulty
    {
        public static DifficultyPassword CheckDifficultyPassword(string password)
        {
            if (IsEasyPassword(password)) return DifficultyPassword.Easy;

            if (password.ToLower().Trim().Contains("password")
                || password.ToLower().Trim().Contains("пароль")
                || password.ToLower().Trim().Contains("йцукен")
                || password.ToLower().Trim().Contains("qwerty"))
                return DifficultyPassword.Easy;

            if (IsCombinationRepeat(password) || IsRepeatContains(password)) return DifficultyPassword.Easy;
            
            var hasUpper = Regex.IsMatch(password, "[A-ZА-Я]");
            var hasLower = Regex.IsMatch(password, "[a-zа-я]");
            var hasDigit = Regex.IsMatch(password, @"[\d]");
            var hasSpecial = Regex.IsMatch(password, @"!|@|#|\$|%|\^|&|\*|\(|\)");

            var kDiff = 0;

            if (!hasDigit || !hasUpper || !hasLower) return DifficultyPassword.Easy;
            
            kDiff = !hasSpecial ? 1 : 2;

            if (password.Length < 12 && kDiff == 1) return DifficultyPassword.Medium;
            if (password.Length < 12 && kDiff == 2) return DifficultyPassword.Medium;

            return DifficultyPassword.Hard;
        }

        private static bool IsCombinationRepeat(string password)
        {
            var lastChar = '\0';
            var combination = 0;
            var count = 0;
            
            foreach (var ch in password.ToLower().Trim())
            {
                if (lastChar == ch) combination++;
                else combination = 0;
                
                lastChar = ch;

                if (combination > 2) count++;
            }

            return count > 1;
        }
        
        private static bool IsRepeatContains(string password)
        {
            var array = new List<char>();
            var count = 0;

            foreach (var ch in password.ToLower().Trim())
            {
                if (!array.Contains(ch)) array.Add(ch);
                else count++;

                if (count > 3) return true;
            }

            return false;
        }

        private static bool IsEasyPassword(string password)
        {
            if (password.Length < 6) return true;

            var startPass = password.Substring(0, password.Length / 2);
            var endPass = password.Substring(password.Length / 2, password.Length / 2);

            return startPass.ToLower().Trim() == endPass.ToLower().Trim();
        }
    }
    
    public enum DifficultyPassword
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }
}