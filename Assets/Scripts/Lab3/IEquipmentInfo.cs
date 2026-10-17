namespace Lab3
{
    /// <summary>
    /// Джерело живого опису обладнання для спливаючої інформаційної картки.
    /// Реалізується вузлами трубопроводу, вентилями та манометрами.
    /// </summary>
    public interface IEquipmentInfo
    {
        /// <summary>Назва обладнання у заголовку картки.</summary>
        string InfoTitle { get; }

        /// <summary>Поточний стан обладнання — перераховується щоразу при наведенні.</summary>
        string BuildInfoBody();
    }
}
