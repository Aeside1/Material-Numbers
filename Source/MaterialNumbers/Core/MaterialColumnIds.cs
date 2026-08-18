namespace MaterialNumbers.Core
{
    public static class MaterialColumnIds
    {
        public const string Amount = "builtin:amount";
        public const string StackLimit = "builtin:stack-limit";
        public const string SourceMod = "builtin:source-mod";

        public static string StatBase(string defName) => "stat-base:" + defName;

        public static string StuffFactor(string defName) => "stuff-factor:" + defName;

        public static string StuffOffset(string defName) => "stuff-offset:" + defName;

        public static string Extension(string typeName, string memberName, string defName)
        {
            return "extension:" + typeName + ":" + memberName + ":" + defName;
        }
    }
}
