using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Framework.Core.UniqueID
{
    /// <summary>
    /// 时间戳ID
    /// </summary>
    public class TimestampID
    {
        private long _lastTimestamp;
        private long _sequence; //计数从零开始
        private readonly DateTime? _initialDateTime;
        private static TimestampID _timestampID;
        private const int MAX_END_NUMBER = 9999;
        private readonly bool _isFill;
        private static readonly object _syncRoot = new object(); //加锁对象

        private TimestampID(DateTime? initialDateTime, bool isFill)
        {
            _initialDateTime = initialDateTime;
            _isFill = isFill;
        }
        /// <summary>
        /// 获取单个实例对象
        /// </summary>
        /// <param name="initialDateTime">最初时间，与当前时间做个相差取时间戳</param>
        /// <param name="isFill">是否需要补位</param>
        /// <returns></returns>
        public static TimestampID GetInstance(DateTime? initialDateTime = null, bool isFill = false)
        {
            if (_timestampID == null) Interlocked.CompareExchange(ref _timestampID, new TimestampID(initialDateTime, isFill), null);
            return _timestampID;
        }
        /// <summary>
        /// 最初时间，作用时间戳的相差
        /// </summary>
        protected DateTime InitialDateTime
        {
            get
            {
                if (_initialDateTime == null || _initialDateTime.Value == DateTime.MinValue) return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return _initialDateTime.Value;
            }
        }
        /// <summary>
        /// 获取时间戳ID
        /// </summary>
        /// <returns></returns>
        public string GetID()
        {
            long temp;
            var timestamp = GetUniqueTimeStamp(out temp);
            return $"{timestamp}{Fill(temp)}";
        }
        private string Fill(long temp)
        {
            if (!_isFill) return temp.ToString();
            var num = temp.ToString();
            IList<char> chars = new List<char>();
            for (int i = 0; i < MAX_END_NUMBER.ToString().Length - num.Length; i++)
            {
                chars.Add('0');
            }
            return new string(chars.ToArray()) + num;
        }
        private long GetUniqueTimeStamp(out long temp)
        {
            lock (_syncRoot)
            {
                temp = 1;
                var timeStamp = GetTimestamp();
                if (timeStamp == _lastTimestamp)
                {
                    _sequence = _sequence + 1;
                    temp = _sequence;
                    if (temp >= MAX_END_NUMBER)
                    {
                        timeStamp = GetTimestamp();
                        _lastTimestamp = timeStamp;
                        temp = _sequence = 1;
                    }
                }
                else
                {
                    _sequence = 1;
                    _lastTimestamp = timeStamp;
                }
                return timeStamp;
            }
        }
        private long GetTimestamp()
        {
            if (InitialDateTime >= DateTime.Now) throw new Exception("最初时间比当前时间还大，不合理");
            var ts = DateTime.UtcNow - InitialDateTime;
            return (long)ts.TotalMilliseconds;
        }
    }
}
