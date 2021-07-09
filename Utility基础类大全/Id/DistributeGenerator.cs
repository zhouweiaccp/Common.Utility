using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DotNetCommon
{
    /// <summary>
    /// 基于分布式的 <c>Id</c> 和 <c>流水号</c> 生成控制器 (Id生成使用雪花算法 <seealso cref="SnowflakeIdWorker"/>)
    /// </summary>
    /// <remarks>
    /// 使用时需要先设置当前计算机节点的 <c>MachineId</c> 和 <c>MachineIdString</c> ，参照： <seealso cref="Machine.SetMachineId(int,string)"/>
    /// </remarks>
    public class DistributeGenerator
    {
        private DistributeGenerator() { }

        private const string defaultKey = "__inner_distribute_key";

        #region Id生成控制
        private static ConcurrentDictionary<string, SnowflakeIdWorker> ht_workers = new ConcurrentDictionary<string, SnowflakeIdWorker>();

        /// <summary>
        /// 根据指定的Key值生成Id (同key值的Id生成有先后顺序,可以将数据库名+表名联合起来作为key值)
        /// </summary>
        /// <param name="key">id类型关键字(可以组合数据库+表名+列名)</param>
        /// <remarks>
        /// 这里使用的雪花算法参数为: 
        /// <list type="bullet">
        /// <item>时间精度: 毫秒<para></para></item>
        /// <item>时间位数: 41,可使用69年<para></para></item>
        /// <item>机器位数: 10,可表示1024台机器<para></para></item>
        /// <item>序列号位数: 12，可表示4096/ms，即：409万QPS</item>
        /// </list>
        /// 如果想使用其他的雪花参数，请直接使用 <seealso cref="SnowflakeIdWorker.NextId()"/>
        /// </remarks>
        /// <returns></returns>
        public static long NewId(string key = null)
        {
            if (string.IsNullOrWhiteSpace(key)) key = defaultKey;
            var work = ht_workers.GetOrAdd(key, _ => new SnowflakeIdWorker(Machine.MachineId));
            return work.NextId();
        }
        #endregion

        #region 流水号生成控制
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, long>> snoCaches = new ConcurrentDictionary<string, ConcurrentDictionary<string, long>>();

        /// <summary>
        /// 生成流水号
        /// </summary>
        /// <param name="key">流水号关键字(用户区分不同的流水号类型)</param>
        /// <param name="format">流水号格式</param>
        /// <returns></returns>
        public static string NewSNO(string key, SerialFormat format)
        {
            if (string.IsNullOrWhiteSpace(key)) key = defaultKey;
            key = key.ToString();
            lock (key)
            {
                var caches = snoCaches.GetOrAdd(key, new ConcurrentDictionary<string, long>());
                var chunk = format.Chunks.FirstOrDefault(i => i.Type == SerialFormatChunkType.DateText);
                var nowstr = DateTime.Now.ToString(chunk.FormatString);
                var id = caches.GetOrAdd(nowstr, 0);
                id++;
                caches[nowstr] = id;
                //组装流水号
                var chunks = format.Chunks;
                var sno = "";
                for (int i = 0, len = chunks.Count; i < len; i++)
                {
                    chunk = chunks[i];
                    if (chunk.Type == SerialFormatChunkType.StaticText)
                    {
                        sno += chunk.FormatString;
                    }
                    else if (chunk.Type == SerialFormatChunkType.DateText)
                    {
                        sno += nowstr;
                    }
                    else if (chunk.Type == SerialFormatChunkType.MachineText)
                    {
                        sno += Machine.MachineIdString.PadLeft(4, '0');
                    }
                    else if (chunk.Type == SerialFormatChunkType.SerialNo)
                    {
                        var s = id.ToString();
                        if (s.Length > chunk.Length)
                        {
                            sno += s;
                        }
                        else
                        {
                            sno += s.PadLeft(chunk.Length, '0');
                        }
                    }
                }
                return sno;
            }
        }
        #endregion
    }

    #region 流水号格式

    /// <summary>
    /// 流水块定义格式
    /// </summary>
    public class SerialFormatChunk
    {
        /// <summary>
        /// 流水块类型
        /// </summary>
        public SerialFormatChunkType Type { set; get; }
        /// <summary>
        /// 流水块格式
        /// </summary>
        public string FormatString { set; get; }
        /// <summary>
        /// 流水块长度
        /// </summary>
        public int Length { set; get; }
    }

    /// <summary>
    /// 流水块类型
    /// </summary>
    public enum SerialFormatChunkType
    {
        /// <summary>
        /// 静态文本
        /// </summary>
        StaticText,
        /// <summary>
        /// 日期
        /// </summary>
        DateText,
        /// <summary>
        /// 机器码
        /// </summary>
        MachineText,
        /// <summary>
        /// 序列号
        /// </summary>
        SerialNo
    }

    /// <summary>
    ///流水号格式
    ///有且只有一个时间格式/一个序列号块,并且序列号必须在末尾
    /// </summary>
    public class SerialFormat
    {
        private SerialFormat() { }
        /// <summary>
        /// 创建流水号格式
        /// </summary>
        /// <param name="prefix">流水号前缀</param>
        /// <param name="dateFormat">日期格式串</param>
        /// <param name="snoLen">序列号长度</param>
        /// <param name="hasMachine">是否启用机器码(默认不启用,分布式环境下务必启用!)</param>
        /// <returns></returns>
        public static SerialFormat CreateFast(string prefix, string dateFormat = "yyyyMMdd", int snoLen = 6, bool hasMachine = false)
        {
            var format = new SerialFormat()
            {
                Chunks = new List<SerialFormatChunk>()
                {
                    new SerialFormatChunk()
                    {
                        Type=SerialFormatChunkType.StaticText,
                        FormatString=prefix
                    },
                    new SerialFormatChunk()
                    {
                        Type=SerialFormatChunkType.DateText,
                        FormatString=dateFormat
                    }
                }
            };
            if (hasMachine)
            {
                format.Chunks.Add(new SerialFormatChunk()
                {
                    Type = SerialFormatChunkType.MachineText
                });
            }
            format.Chunks.Add(new SerialFormatChunk()
            {
                Type = SerialFormatChunkType.SerialNo,
                Length = snoLen
            });
            return format;
        }

        /// <summary>
        /// 创建流水号格式,启用分布式生成
        /// </summary>
        /// <param name="prefix">流水号前缀</param>
        /// <param name="dateFormat">日期格式串</param>
        /// <param name="snoLen">序列号长度</param>
        /// <returns></returns>
        public static SerialFormat CreateDistributeFast(string prefix, string dateFormat = "yyyyMMdd", int snoLen = 6) => CreateFast(prefix, dateFormat, snoLen, true);

        /// <summary>
        /// 创建流水号格式
        /// </summary>
        /// <param name="chunks"></param>
        /// <returns></returns>
        public static SerialFormat CreateByChunks(List<SerialFormatChunk> chunks)
        {
            var format = new SerialFormat() { Chunks = chunks };
            var msg = ValidFormat(format);
            if (!string.IsNullOrWhiteSpace(msg)) throw new Exception(msg);
            return format;
        }

        /// <summary>
        /// 解析指定的流水号规则
        /// </summary>
        /// <param name="format">流水号格式</param>
        /// <param name="now">当前日期</param>
        /// <param name="machineIdString">机器Id串</param>
        /// <returns>(string likeStr, DateTime snoNow, int startIndex)</returns>
        public static (string likeStr, DateTime snoNow, int startIndex) Parse(SerialFormat format, DateTime? now, string machineIdString)
        {
            if (now == null) now = DateTime.Now;
            var msg = ValidFormat(format);
            if (!string.IsNullOrWhiteSpace(msg)) throw new Exception(msg);
            int startindex = 0;
            DateTime snoNow = DateTime.MinValue;
            var likestr = "";
            var chunks = format.Chunks;
            for (int i = 0, len = chunks.Count; i < len; i++)
            {
                var chunk = chunks[i];
                if (chunk.Type == SerialFormatChunkType.StaticText)
                {
                    likestr += chunk.FormatString;
                }
                else if (chunk.Type == SerialFormatChunkType.DateText)
                {
                    var nowstr = now.Value.ToString(chunk.FormatString);
                    likestr += nowstr;
                    snoNow = DateTime.ParseExact(nowstr, format.Chunks.FirstOrDefault(k => k.Type == SerialFormatChunkType.DateText).FormatString, System.Globalization.CultureInfo.CurrentCulture);
                }
                else if (chunk.Type == SerialFormatChunkType.MachineText)
                {
                    likestr += machineIdString.PadLeft(4, '0').Replace("'", "''");
                }
                else if (chunk.Type == SerialFormatChunkType.SerialNo)
                {
                    startindex = likestr.Length;
                    likestr += "%";
                }
            }
            return (likestr, snoNow, startindex);
        }

        /// <summary>
        /// 流水号定义格式块结合
        /// </summary>
        public List<SerialFormatChunk> Chunks { private set; get; }

        /// <summary>
        /// 默认的流水号格式
        /// </summary>
        public static SerialFormat Default = CreateFast("");

        /// <summary>
        /// 检测流水号是否符合格式规则
        /// </summary>
        /// <param name="format">流水号格式</param>
        /// <returns></returns>
        public static string ValidFormat(SerialFormat format)
        {
            if (format == null) return "流水号格式为空!";
            var str = "流水号格式中必须包含1个日期格式和1个序列号格式,并且序列号格式排在最后!";
            if (format.Chunks == null && format.Chunks.Count == 0) return str;
            var chunks = format.Chunks;
            if (chunks.FirstOrDefault(i => i.Type == SerialFormatChunkType.DateText) == null) return str;
            if (chunks[chunks.Count - 1].Type != SerialFormatChunkType.SerialNo) return str;
            return null;
        }
    }
    #endregion

    #region 雪花算法
    /// <summary>
    /// 扩展雪花Id生成算法<para></para>
    /// Twitter_Snowflake（标准雪花Id）: <c>long</c> 型, 64bit位整数，分成：1 + 41 + 10 + 12 四部分<para></para>
    /// <term>1bit</term>固定为1，表示正整数<para></para>
    /// <term>41bit</term>记录从2020-01-01到现在的毫秒数，最多可表示69年<para></para>
    /// <term>10bit</term>机器Id，最多可表示1024台机器<para></para>
    /// <term>12bit</term>序列号，最大并发，4096/ms，即：409万QPS<para></para>
    /// 扩展后：<para></para>
    /// <list type="bullet">
    /// <item>可自定义时间精度，秒或毫秒；</item>
    /// <item>可自定义时间戳位数；</item>
    /// <item>可自定义机器Id位数；</item>
    /// <item>可自定义序列号位数；</item>
    /// <item>可根据Id反向解析时间_机器Id_序列号；</item>
    /// </list>
    /// </summary>
    public class SnowflakeIdWorker
    {
        /// <summary>
        /// 雪花算法的时间精度
        /// </summary>
        public enum WorkerTimeModel
        {
            /// <summary>
            /// 秒级
            /// </summary>
            Second,
            /// <summary>
            /// 毫秒级
            /// </summary>
            Millisecond
        }

        /// <summary>
        /// 开始时间戳，默认：2020-01-01 (UTC)
        /// </summary>
        public DateTime StartTime { get; }

        /// <summary>
        /// 时间精度，默认: 毫秒级
        /// </summary>
        public WorkerTimeModel TimeModel { get; }

        /// <summary>
        /// 时间戳占用的bit位数
        /// </summary>
        public int TimeLength { get; }

        /// <summary>
        /// 机器Id占用的bit位数
        /// </summary>
        public int MachineLength { get; }

        /// <summary>
        /// 序列号占用的bit位数
        /// </summary>
        public int SequenceLength { get; }

        /// <summary>
        /// 机器Id，>=0， 默认: 0
        /// </summary>
        public long MachineId { get; }

        /// <summary>
        /// 构建雪花Id生成器
        /// </summary>
        /// <param name="machineId">机器Id, >=0 </param>
        /// <param name="startTime">起始时间, >=2020年, 默认：2020-01-01 </param>
        /// <param name="timeModel">时间精度，默认：毫秒</param>
        /// <param name="timeLength">时间戳bit位数：[28,44]，默认：41，即: 69年</param>
        /// <param name="machineLength">机器Id的bit位数：[5,18]，默认：10，即：1024台</param>
        /// <param name="sequenceLength">序列号的bit位数：[2,24]，默认：12，即：408万QPS</param>
        public SnowflakeIdWorker(
            int machineId,
            DateTime? startTime = null,
            WorkerTimeModel timeModel = WorkerTimeModel.Millisecond,
            int timeLength = 41,
            int machineLength = 10,
            int sequenceLength = 12)
        {
            //设置machineId
            if (machineId < 0) throw new Exception("机器Id必须大于等于0!");
            MachineId = machineId;
            //设置startTime
            if (startTime == null) StartTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            else
            {
                if (startTime < DateTime.Parse("2020-01-01")) throw new Exception("时间戳的起始时间不能小于 \"2020-01-01\"!");
                StartTime = startTime.Value.ToUniversalTime();
            }
            //设置timeModel
            TimeModel = timeModel;
            //设置timeLength
            if (timeLength < 28 || timeLength > 44) throw new Exception("时间戳bit位数必须在28-44范围内!");
            TimeLength = timeLength;
            //设置machineLength
            if (machineLength < 5 || machineLength > 18) throw new Exception("机器Id的bit位数必须在5-18范围内!");
            MachineLength = machineLength;
            //设置sequenceLength
            if (sequenceLength < 2 || sequenceLength > 24) throw new Exception("序列号的bit位数必须在2-24范围内!");
            SequenceLength = sequenceLength;

            //最后检查machineId和machineLength是否匹配
            if (Math.Pow(2, machineLength) <= machineId) throw new Exception($"当前设置的机器Id的bit位数:{machineLength} 不能表示机器Id的值: {machineId} !");

            //序列号掩码
            sequenceMask = -1L ^ (-1L << sequenceLength);
        }



        private long lastTimestamp = 0;
        private long sequence = 0;

        private long sequenceMask = 0;

        /// <summary>
        /// 获得下一个ID
        /// </summary>
        /// <returns></returns>
        public long NextId()
        {
            lock (this)
            {
                //获取当前时间戳
                long timestamp = GetCurrentTimestamp();
                if (timestamp != lastTimestamp)
                {
                    //时间戳改变，序列重置,不管是否回拨，总数依赖当前时间戳生成Id
                    sequence = 1L;
                }
                else if (timestamp == lastTimestamp)
                {
                    //时间戳相同, 序列自增
                    sequence = (sequence + 1) & sequenceMask;
                    if (sequence == 0)
                    {
                        //序列溢出，阻塞获得新的时间戳
                        timestamp = GetNextTimestamp(lastTimestamp);
                        sequence = 1L;
                    }
                }

                //移位并通过或运算拼到一起组成64位的ID
                var id = (timestamp << (SequenceLength + MachineLength))
                        | (MachineId << SequenceLength)
                        | sequence;

                //重置时间戳
                lastTimestamp = timestamp;

                return id;
            }
        }

        /// <summary>
        /// 解析雪花ID
        /// </summary>
        /// <returns></returns>
        public string AnalyzeId(long Id)
        {
            StringBuilder sb = new StringBuilder();

            var timestamp = (Id >> SequenceLength + MachineLength);
            var time = StartTime + (TimeModel == WorkerTimeModel.Second ? TimeSpan.FromSeconds(timestamp) : TimeSpan.FromMilliseconds(timestamp));
            sb.Append(time.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fffzzz"));

            var machineId = (Id ^ (timestamp << (SequenceLength + MachineLength))) >> SequenceLength;
            sb.Append("_" + machineId);

            var sequence = Id & sequenceMask;
            sb.Append("_" + sequence);

            return sb.ToString();
        }

        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        /// <returns></returns>
        private long GetCurrentTimestamp()
        {
            var span = DateTime.UtcNow - StartTime;
            return TimeModel == WorkerTimeModel.Millisecond ? (long)span.TotalMilliseconds : (long)span.TotalSeconds;
        }

        /// <summary>
        /// 阻塞直到获得新的时间戳
        /// </summary>
        /// <param name="lastTimestamp">上次生成ID的时间戳</param>
        /// <returns>当前时间戳</returns>
        private long GetNextTimestamp(long lastTimestamp)
        {
            long timestamp = GetCurrentTimestamp();
            while (timestamp <= lastTimestamp)
            {
                if (TimeModel == WorkerTimeModel.Second)
                {
                    Thread.Sleep(200);
                }
                timestamp = GetCurrentTimestamp();
            }
            return timestamp;
        }
    }
    #endregion
}
