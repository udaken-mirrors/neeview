namespace NeeView
{
    /// <summary>
    /// コマンドパラメータ引数管理用
    /// </summary>
    public class CommandParameterArgs
    {
        public CommandParameterArgs(object? param)
        {
            Parameter = param;
            AllowFlip = true;
        }

        public CommandParameterArgs(object? param, bool allowRecursive)
        {
            Parameter = param;
            AllowFlip = allowRecursive;
        }


        /// <summary>
        /// 標準パラメータ
        /// </summary>
        ////public static CommandParameterArgs Null { get; } = new CommandParameterArgs(null);

        /// <summary>
        /// パラメータ本体
        /// </summary>
        public object? Parameter { get; private set; }

        /// <summary>
        /// スライダー方向でのコマンド入れ替え許可
        /// </summary>
        public bool AllowFlip { get; set; }


        public static CommandParameterArgs Create(object param)
        {
            if (param is CommandParameterArgs parameterArgs)
            {
                return parameterArgs;
            }
            else
            {
                return new CommandParameterArgs(param);
            }
        }
    }
}
