using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace FreeSql.Tests.Sqlite
{
    public class SqliteCodeFirstTest
    {

        [Fact]
        public void 表名中有点()
        {
            var item = new tbdot01 { name = "insert" };
            g.sqlite.Insert(item).ExecuteAffrows();

            var find = g.sqlite.Select<tbdot01>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal("insert", find.name);

            Assert.Equal(1, g.sqlite.Update<tbdot01>().Set(a => a.name == "update").Where(a => a.id == item.id).ExecuteAffrows());
            find = g.sqlite.Select<tbdot01>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal("update", find.name);

            Assert.Equal(1, g.sqlite.Delete<tbdot01>().Where(a => a.id == item.id).ExecuteAffrows());
            find = g.sqlite.Select<tbdot01>().Where(a => a.id == item.id).First();
            Assert.Null(find);
        }
        [Table(Name = "\"sys.tbdot01\"")]
        class tbdot01
        {
            public Guid id { get; set; }
            public string name { get; set; }
        }

        [Fact]
        public void 中文表_字段()
        {
            var sql = g.sqlite.CodeFirst.GetComparisonDDLStatements<测试中文表>();
            g.sqlite.CodeFirst.SyncStructure<测试中文表>();

            var item = new 测试中文表
            {
                标题 = "测试标题",
                创建时间 = DateTime.Now
            };
            Assert.Equal(1, g.sqlite.Insert<测试中文表>().AppendData(item).ExecuteAffrows());
            Assert.NotEqual(Guid.Empty, item.编号);
            var item2 = g.sqlite.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新";
            Assert.Equal(1, g.sqlite.Update<测试中文表>().SetSource(item).ExecuteAffrows());
            item2 = g.sqlite.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新_repo";
            var repo = g.sqlite.GetRepository<测试中文表>();
            Assert.Equal(1, repo.Update(item));
            item2 = g.sqlite.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);

            item.标题 = "测试标题更新_repo22";
            Assert.Equal(1, repo.Update(item));
            item2 = g.sqlite.Select<测试中文表>().Where(a => a.编号 == item.编号).First();
            Assert.NotNull(item2);
            Assert.Equal(item.编号, item2.编号);
            Assert.Equal(item.标题, item2.标题);
        }
        class 测试中文表
        {
            [Column(IsPrimary = true)]
            public Guid 编号 { get; set; }

            public string 标题 { get; set; }

            [Column(ServerTime = DateTimeKind.Local, CanUpdate = false)]
            public DateTime 创建时间 { get; set; }

            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime 更新时间 { get; set; }
        }

        [Fact]
        public void AddUniques()
        {
            var sql = g.sqlite.CodeFirst.GetComparisonDDLStatements<AddUniquesInfo>();
            g.sqlite.CodeFirst.SyncStructure<AddUniquesInfo>();
        }
        [Table(Name = "AddUniquesInfo2", OldName = "AddUniquesInfo")]
        [Index("uk_phone", "phone", true)]
        [Index("uk_group_index", "group,index", true)]
        [Index("uk_group_index22", "group desc, index22", true)]
        class AddUniquesInfo
        {
            public Guid id { get; set; }
            public string phone { get; set; }

            public string group { get; set; }
            public int index { get; set; }
            public string index22 { get; set; }
        }

        public class Topic
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public DateTime CreateTime { get; set; }
        }
        [Table(Name = "xxxtb.Comment")]
        public class Comment
        {
            public Guid Id { get; set; }
            public Guid TopicId { get; set; }
            public virtual Topic Topic { get; set; }
            public string Nickname { get; set; }
            public string Content { get; set; }
            public DateTime CreateTime { get; set; }
        }


        [Fact]
        public void AddField()
        {

            //秀一波 FreeSql.Repository 扩展包，dotnet add package FreeSql.Repository
            var topicRepository = g.sqlite.GetGuidRepository<Topic>();
            var commentRepository = g.sqlite.GetGuidRepository<Comment>();

            //添加测试文章
            var topic = topicRepository.Insert(new Topic
            {
                Title = "文章标题1",
                Content = "文章内容1",
                CreateTime = DateTime.Now
            });

            //添加10条测试评论
            var comments = Enumerable.Range(0, 10).Select(a => new Comment
            {
                TopicId = topic.Id,
                Nickname = $"昵称{a}",
                Content = $"评论内容{a}",
                CreateTime = DateTime.Now
            }).ToArray();
            var affrows = commentRepository.Insert(comments);

            var find = commentRepository.Select.Where(a => a.Topic.Title == "文章标题1").ToList();




            var sql = g.sqlite.CodeFirst.GetComparisonDDLStatements<TopicAddField>();

            var id = g.sqlite.Insert<TopicAddField>().AppendData(new TopicAddField { }).ExecuteIdentity();

            //var inserted = g.Sqlite.Insert<TopicAddField>().AppendData(new TopicAddField { }).ExecuteInserted();
        }

        [Table(Name = "xxxtb.TopicAddField", OldName = "TopicAddField")]
        public class TopicAddField
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }

            public string name { get; set; }

            [Column(DbType = "varchar(200) not null", OldName = "title2")]
            public string title3223 { get; set; } = "10";

            [Column(IsIgnore = true)]
            public DateTime ct { get; set; } = DateTime.Now;
        }

        [Fact]
        public void GetComparisonDDLStatements()
        {

            var sql = g.sqlite.CodeFirst.GetComparisonDDLStatements<TableAllType>();
            if (string.IsNullOrEmpty(sql) == false)
            {
                Assert.Equal(@"CREATE TABLE IF NOT EXISTS ""main"".""tb_alltype"" ( 
  ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
  ""Bool"" BOOLEAN NOT NULL, 
  ""SByte"" SMALLINT NOT NULL, 
  ""Short"" SMALLINT NOT NULL, 
  ""Int"" INTEGER NOT NULL, 
  ""Long"" INTEGER NOT NULL, 
  ""Byte"" INT2 NOT NULL, 
  ""UShort"" UNSIGNED NOT NULL, 
  ""UInt"" DECIMAL(10,0) NOT NULL, 
  ""ULong"" DECIMAL(21,0) NOT NULL, 
  ""Double"" DOUBLE NOT NULL, 
  ""Float"" FLOAT NOT NULL, 
  ""Decimal"" DECIMAL(10,2) NOT NULL, 
  ""TimeSpan"" BIGINT NOT NULL, 
  ""DateTime"" DATETIME NOT NULL, 
  ""DateTimeOffSet"" DATETIME NOT NULL, 
  ""Bytes"" BLOB, 
  ""String"" NVARCHAR(255), 
  ""Guid"" CHARACTER(36) NOT NULL, 
  ""BoolNullable"" BOOLEAN, 
  ""SByteNullable"" SMALLINT, 
  ""ShortNullable"" SMALLINT, 
  ""IntNullable"" INTEGER, 
  ""testFielLongNullable"" INTEGER, 
  ""ByteNullable"" INT2, 
  ""UShortNullable"" UNSIGNED, 
  ""UIntNullable"" DECIMAL(10,0), 
  ""ULongNullable"" DECIMAL(21,0), 
  ""DoubleNullable"" DOUBLE, 
  ""FloatNullable"" FLOAT, 
  ""DecimalNullable"" DECIMAL(10,2), 
  ""TimeSpanNullable"" BIGINT, 
  ""DateTimeNullable"" DATETIME, 
  ""DateTimeOffSetNullable"" DATETIME, 
  ""GuidNullable"" CHARACTER(36), 
  ""Enum1"" MEDIUMINT NOT NULL, 
  ""Enum1Nullable"" MEDIUMINT, 
  ""Enum2"" BIGINT NOT NULL, 
  ""Enum2Nullable"" BIGINT
) 
;
", sql);
            }

            //sql = g.Sqlite.CodeFirst.GetComparisonDDLStatements<Tb_alltype>();
        }

        IInsert<TableAllType> insert => g.sqlite.Insert<TableAllType>();
        ISelect<TableAllType> select => g.sqlite.Select<TableAllType>();

        [Fact]
        public void CurdAllField()
        {
            var item = new TableAllType { };
            item.Id = (int)insert.AppendData(item).ExecuteIdentity();

            var newitem = select.Where(a => a.Id == item.Id).ToOne();

            var item2 = new TableAllType
            {
                Bool = true,
                BoolNullable = true,
                Byte = 255,
                ByteNullable = 127,
                Bytes = Encoding.UTF8.GetBytes("我是中国人"),
                DateTime = DateTime.Now,
                DateTimeNullable = DateTime.Now.AddHours(-1),
                Decimal = 99.99M,
                DecimalNullable = 99.98M,
                Double = 999.99,
                DoubleNullable = 999.98,
                Enum1 = TableAllTypeEnumType1.e5,
                Enum1Nullable = TableAllTypeEnumType1.e3,
                Enum2 = TableAllTypeEnumType2.f2,
                Enum2Nullable = TableAllTypeEnumType2.f3,
                Float = 19.99F,
                FloatNullable = 19.98F,
                Guid = Guid.NewGuid(),
                GuidNullable = Guid.NewGuid(),
                Int = int.MaxValue,
                IntNullable = int.MinValue,
                SByte = 100,
                SByteNullable = 99,
                Short = short.MaxValue,
                ShortNullable = short.MinValue,
                String = "我是中国人string",
                TimeSpan = TimeSpan.FromSeconds(999),
                TimeSpanNullable = TimeSpan.FromSeconds(60),
                UInt = uint.MaxValue,
                UIntNullable = uint.MinValue,
                ULong = ulong.MaxValue - 10000000,
                ULongNullable = ulong.MinValue,
                UShort = ushort.MaxValue,
                UShortNullable = ushort.MinValue,
                testFielLongNullable = long.MinValue
            };
            item2.Id = (int)insert.AppendData(item2).ExecuteIdentity();
            var newitem2 = select.Where(a => a.Id == item2.Id).ToOne();

            var items = select.ToList();
        }

        [Table(Name = "tb_alltype")]
        class TableAllType
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }

            //public string id2 { get; set; } = "id2=10";

            public bool Bool { get; set; }
            public sbyte SByte { get; set; }
            public short Short { get; set; }
            public int Int { get; set; }
            public long Long { get; set; }
            public byte Byte { get; set; }
            public ushort UShort { get; set; }
            public uint UInt { get; set; }
            public ulong ULong { get; set; }
            public double Double { get; set; }
            public float Float { get; set; }
            public decimal Decimal { get; set; }
            public TimeSpan TimeSpan { get; set; }

            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime DateTime { get; set; }
            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime DateTimeOffSet { get; set; }

            public byte[] Bytes { get; set; }
            public string String { get; set; }
            public Guid Guid { get; set; }

            public bool? BoolNullable { get; set; }
            public sbyte? SByteNullable { get; set; }
            public short? ShortNullable { get; set; }
            public int? IntNullable { get; set; }
            public long? testFielLongNullable { get; set; }
            public byte? ByteNullable { get; set; }
            public ushort? UShortNullable { get; set; }
            public uint? UIntNullable { get; set; }
            public ulong? ULongNullable { get; set; }
            public double? DoubleNullable { get; set; }
            public float? FloatNullable { get; set; }
            public decimal? DecimalNullable { get; set; }
            public TimeSpan? TimeSpanNullable { get; set; }

            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime? DateTimeNullable { get; set; }
            [Column(ServerTime = DateTimeKind.Local)]
            public DateTime? DateTimeOffSetNullable { get; set; }

            public Guid? GuidNullable { get; set; }

            public TableAllTypeEnumType1 Enum1 { get; set; }
            public TableAllTypeEnumType1? Enum1Nullable { get; set; }
            public TableAllTypeEnumType2 Enum2 { get; set; }
            public TableAllTypeEnumType2? Enum2Nullable { get; set; }
        }

        public enum TableAllTypeEnumType1 { e1, e2, e3, e5 }
        [Flags] public enum TableAllTypeEnumType2 { f1, f2, f3 }
    }
}
