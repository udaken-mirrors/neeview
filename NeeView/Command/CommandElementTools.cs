using System;
using System.Text.RegularExpressions;

namespace NeeView
{
    public static class CommandElementTools
    {
        private static readonly Regex _trimCommand = new(@"Command$", RegexOptions.Compiled);

        /// <summary>
        /// コマンド名をクラスタイプから生成
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string CreateCommandName<T>()
            where T : CommandElement
        {
            return CreateCommandName(typeof(T));
        }

        /// <summary>
        /// コマンド名をクラスタイプから生成
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string CreateCommandName(Type type)
        {
            return _trimCommand.Replace(type.Name, "");
        }
    }
}

