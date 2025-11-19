public static class StringExtension
{
    public static string TruncateWithEllipsis(this string input, int maxLength)
    {
        // 如果字符串为空或长度小于等于最大长度，则直接返回原字符串
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
        {
            return input;
        }

        // 如果最大长度小于等于 3，则直接返回省略号
        if (maxLength <= 3)
        {
            return "...";
        }

        // 截断字符串并添加省略号
        return input.Substring(0, maxLength - 3) + "...";
    }
}
